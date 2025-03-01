// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Shared.Options;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.SolutionCrawler;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Versions;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.Implementation.TodoComments
{
    internal partial class TodoCommentIncrementalAnalyzer : IIncrementalAnalyzer
    {
        public const string Name = "Todo Comment Document Worker";

        private readonly TodoCommentIncrementalAnalyzerProvider _owner;
        private readonly Workspace _workspace;
        private readonly IOptionService _optionService;
        private readonly TodoCommentTokens _todoCommentTokens;
        private readonly TodoCommentState _state;

        public TodoCommentIncrementalAnalyzer(Workspace workspace, IOptionService optionService, TodoCommentIncrementalAnalyzerProvider owner, TodoCommentTokens todoCommentTokens)
        {
            _workspace = workspace;
            _optionService = optionService;

            _owner = owner;
            _todoCommentTokens = todoCommentTokens;

            _state = new TodoCommentState();
        }

        public Task DocumentResetAsync(Document document, CancellationToken cancellationToken)
        {
            // remove cache
            _state.Remove(document.Id);
            return _state.PersistAsync(document, new Data(VersionStamp.Default, VersionStamp.Default, ImmutableArray<TodoItem>.Empty), cancellationToken);
        }

        public async Task AnalyzeSyntaxAsync(Document document, CancellationToken cancellationToken)
        {
            Contract.ThrowIfFalse(document.IsFromPrimaryBranch());

            // it has an assumption that this will not be called concurrently for same document.
            // in fact, in current design, it won't be even called concurrently for different documents.
            // but, can be called concurrently for different documents in future if we choose to.
            if (!_optionService.GetOption(InternalFeatureOnOffOptions.TodoComments))
            {
                return;
            }

            // use tree version so that things like compiler option changes are considered
            var textVersion = await document.GetTextVersionAsync(cancellationToken).ConfigureAwait(false);
            var syntaxVersion = await document.GetSyntaxVersionAsync(cancellationToken).ConfigureAwait(false);

            var existingData = await _state.TryGetExistingDataAsync(document, cancellationToken).ConfigureAwait(false);
            if (existingData != null)
            {
                // check whether we can use the data as it is (can happen when re-using persisted data from previous VS session)
                if (CheckVersions(document, textVersion, syntaxVersion, existingData))
                {
                    Contract.Requires(_workspace == document.Project.Solution.Workspace);
                    RaiseTaskListUpdated(_workspace, document.Project.Solution, document.Id, existingData.Items);
                    return;
                }
            }

            var service = document.GetLanguageService<ITodoCommentService>();
            if (service == null)
            {
                return;
            }

            var comments = await service.GetTodoCommentsAsync(document, _todoCommentTokens.GetTokens(_workspace), cancellationToken).ConfigureAwait(false);
            var items = await CreateItemsAsync(document, comments, cancellationToken).ConfigureAwait(false);

            var data = new Data(textVersion, syntaxVersion, items);
            await _state.PersistAsync(document, data, cancellationToken).ConfigureAwait(false);

            // * NOTE * cancellation can't throw after this point.
            if (existingData == null || existingData.Items.Length > 0 || data.Items.Length > 0)
            {
                Contract.Requires(_workspace == document.Project.Solution.Workspace);
                RaiseTaskListUpdated(_workspace, document.Project.Solution, document.Id, data.Items);
            }
        }

        private async Task<ImmutableArray<TodoItem>> CreateItemsAsync(Document document, IList<TodoComment> comments, CancellationToken cancellationToken)
        {
            var items = ImmutableArray.CreateBuilder<TodoItem>();
            if (comments != null)
            {
                var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
                var syntaxTree = document.SupportsSyntaxTree ? await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false) : null;

                foreach (var comment in comments)
                {
                    items.Add(CreateItem(document, text, syntaxTree, comment));
                }
            }

            return items.ToImmutable();
        }

        private TodoItem CreateItem(Document document, SourceText text, SyntaxTree tree, TodoComment comment)
        {
            // make sure given position is within valid text range.
            var textSpan = new TextSpan(Math.Min(text.Length, Math.Max(0, comment.Position)), 0);

            var location = tree == null ? Location.Create(document.FilePath, textSpan, text.Lines.GetLinePositionSpan(textSpan)) : tree.GetLocation(textSpan);
            var originalLineInfo = location.GetLineSpan();
            var mappedLineInfo = location.GetMappedLineSpan();

            return new TodoItem(
                comment.Descriptor.Priority,
                comment.Message,
                document.Project.Solution.Workspace,
                document.Id,
                mappedLine: mappedLineInfo.StartLinePosition.Line,
                originalLine: originalLineInfo.StartLinePosition.Line,
                mappedColumn: mappedLineInfo.StartLinePosition.Character,
                originalColumn: originalLineInfo.StartLinePosition.Character,
                mappedFilePath: mappedLineInfo.GetMappedFilePathIfExist(),
                originalFilePath: document.FilePath);
        }

        public ImmutableArray<TodoItem> GetTodoItems(Workspace workspace, DocumentId id, CancellationToken cancellationToken)
        {
            var document = workspace.CurrentSolution.GetDocument(id);
            if (document == null)
            {
                return ImmutableArray<TodoItem>.Empty;
            }

            // TODO let's think about what to do here. for now, let call it synchronously. also, there is no actual asynch-ness for the
            // TryGetExistingDataAsync, API just happen to be async since our persistent API is async API. but both caller and implementor are
            // actually not async.
            var existingData = _state.TryGetExistingDataAsync(document, cancellationToken).WaitAndGetResult(cancellationToken);
            if (existingData == null)
            {
                return ImmutableArray<TodoItem>.Empty;
            }

            return existingData.Items;
        }

        private static bool CheckVersions(Document document, VersionStamp textVersion, VersionStamp syntaxVersion, Data existingData)
        {
            // first check full version to see whether we can reuse data in same session, if we can't, check timestamp only version to see whether
            // we can use it cross-session.
            return document.CanReusePersistedTextVersion(textVersion, existingData.TextVersion) &&
                   document.CanReusePersistedSyntaxTreeVersion(syntaxVersion, existingData.SyntaxVersion);
        }

        internal ImmutableArray<TodoItem> GetItems_TestingOnly(DocumentId documentId)
        {
            return _state.GetItems_TestingOnly(documentId);
        }

        private void RaiseTaskListUpdated(Workspace workspace, Solution solution, DocumentId documentId, ImmutableArray<TodoItem> items)
        {
            if (_owner != null)
            {
                _owner.RaiseTaskListUpdated(documentId, workspace, solution, documentId.ProjectId, documentId, items);
            }
        }

        public void RemoveDocument(DocumentId documentId)
        {
            _state.Remove(documentId);

            RaiseTaskListUpdated(_workspace, null, documentId, ImmutableArray<TodoItem>.Empty);
        }

        public bool NeedsReanalysisOnOptionChanged(object sender, OptionChangedEventArgs e)
        {
            return e.Option == TodoCommentOptions.TokenList;
        }

        private class Data
        {
            public readonly VersionStamp TextVersion;
            public readonly VersionStamp SyntaxVersion;
            public readonly ImmutableArray<TodoItem> Items;

            public Data(VersionStamp textVersion, VersionStamp syntaxVersion, ImmutableArray<TodoItem> items)
            {
                this.TextVersion = textVersion;
                this.SyntaxVersion = syntaxVersion;
                this.Items = items;
            }
        }

        #region not used
        public Task NewSolutionSnapshotAsync(Solution solution, CancellationToken cancellationToken)
        {
            return SpecializedTasks.EmptyTask;
        }

        public Task DocumentOpenAsync(Document document, CancellationToken cancellationToken)
        {
            return SpecializedTasks.EmptyTask;
        }

        public Task DocumentCloseAsync(Document document, CancellationToken cancellationToken)
        {
            return SpecializedTasks.EmptyTask;
        }

        public Task AnalyzeDocumentAsync(Document document, SyntaxNode bodyOpt, CancellationToken cancellationToken)
        {
            return SpecializedTasks.EmptyTask;
        }

        public Task AnalyzeProjectAsync(Project project, bool semanticsChanged, CancellationToken cancellationToken)
        {
            return SpecializedTasks.EmptyTask;
        }

        public void RemoveProject(ProjectId projectId)
        {
        }
        #endregion
    }
}
