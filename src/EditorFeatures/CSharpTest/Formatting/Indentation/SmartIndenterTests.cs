﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editor.UnitTests;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Formatting.Rules;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Formatting.Indentation
{
    public partial class SmartIndenterTests : FormatterTestsBase
    {
        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void EmptyFile()
        {
            AssertSmartIndent(
                code: string.Empty,
                indentationLine: 0,
                expectedIndentation: 0);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NoPreviousLine()
        {
            var code = @"#region Test

#warning 0
#undef SYMBOL
#define SYMBOL
#if false
#elif true
#else
#endif
#pragma warning disable 99999
#foo

#endregion

";
            AssertSmartIndent(
                code,
                indentationLine: 13,
                expectedIndentation: 0);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void EndOfFileInactive()
        {
            var code = @"
    // Line 1
#if false
#endif

";
            AssertSmartIndent(
                code,
                indentationLine: 4,
                expectedIndentation: 4);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void EndOfFileInactive2()
        {
            var code = @"
    // Line 1
#if false
#endif
// Line 2

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 0);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Comments()
        {
            var code = @"using System;

class Class
{
    // Comments
    /// Xml Comments

";
            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 4);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void UsingDirective()
        {
            var code = @"using System;

";
            AssertSmartIndent(
                code,
                indentationLine: 1,
                expectedIndentation: 0);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void DottedName()
        {
            var code = @"using System.

";
            AssertSmartIndent(
                code,
                indentationLine: 1,
                expectedIndentation: 4);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Namespace()
        {
            var code = @"using System;

namespace NS

";
            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 4);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NamespaceDottedName()
        {
            var code = @"using System;

namespace NS.

";
            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 4);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NamespaceBody()
        {
            var code = @"using System;

namespace NS
{

";
            AssertSmartIndent(
                code,
                indentationLine: 4,
                expectedIndentation: 4);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Class()
        {
            var code = @"using System;

namespace NS
{
    class Class

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void ClassBody()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {

";
            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Method()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        void Method()

";

            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void MethodBody()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        void Method()
        {

";

            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Property()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        public static string Name

";

            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void PropertyGetBody()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        private string name;
        public string Names
        {
            get

";
            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void PropertySetBody()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        private static string name;
        public static string Names
        {
            set

";

            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Statement()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        void Method()
        {
            int i = 10;

";

            AssertSmartIndent(
                code,
                indentationLine: 9,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void FieldInitializer()
        {
            var code = @"class C
{
    int i = 2;
";

            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 4);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void FieldInitializerWithNamespace()
        {
            var code = @"namespace NS
{
    class C
    {
        C c = new C();

";

            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void MethodCall()
        {
            var code = @"class c
{
    void Method()
    {
        M(
            a: 1, 
            b: 1);
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 12);
            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: null);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Switch()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        void Method()
        {
            switch (10)

";

            AssertSmartIndent(
                code,
                indentationLine: 9,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void SwitchBody()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        void Method()
        {
            switch (10)
            {

";

            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void SwitchCase()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        void Method()
        {
            switch (10)
            {
                case 10 :

";

            AssertSmartIndent(
                code,
                indentationLine: 11,
                expectedIndentation: 20);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Block()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        void Method()
        {
            switch (10)
            {
                case 10 :
                    {

";

            AssertSmartIndent(
                code,
                indentationLine: 12,
                expectedIndentation: 24);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void MultilineStatement1()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        void Method()
        {
            int i = 10 +

";

            AssertSmartIndent(
                code,
                indentationLine: 9,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void MultilineStatement2()
        {
            var code = @"using System;

namespace NS
{
    class Class
    {
        void Method()
        {
            int i = 10 +
                    20 +

";

            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 20);
        }

        // Bug number 902477
        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Comments2()
        {
            var code = @"class Class
{
    void Method()
    {
        if (true) // Test

    }
}
";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void AfterCompletedBlock()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        foreach(var a in x) {}

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void AfterCompletedBlockNestedInOtherBlock()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        foreach(var a in x) {{}

        }
    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void AfterTopLevelAttribute()
        {
            var code = @"class Program
{
    [Attr]

}

";
            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 4);
        }

        [WorkItem(537802)]
        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void EmbeddedStatement()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        if (true)
            Console.WriteLine(1);

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 8);
        }

        [WorkItem(537883)]
        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void EnterAfterComment()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        int a; // enter

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 8);
        }

        [WorkItem(538121)]
        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NestedBlock1()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        {

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NestedEmbeddedStatement1()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        if (true)
            if (true)
                if (true)
                    args = null;

    }
}
";
            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NestedEmbeddedStatement2()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        if (true)
            if (true)
                if (true)
                    { }

    }
}
";

            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NestedEmbeddedStatement3()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        if (true)
            if (true)
                if (true)
                    { return; }

    }
}
";

            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NestedEmbeddedStatement4()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        if (true)
            if (true)
                if (true)
                    args = null;

";

            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NestedEmbeddedStatement5()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        if (true)
            if (true)
                if (true)
                    { }

";

            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NestedEmbeddedStatement6()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        if (true)
            if (true)
                if (true)
                    { return; }

";

            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NestedEmbeddedStatement7()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        if (true)
            if (true)
                if (true)
                    return;
                else
                    return;

";

            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void NestedEmbeddedStatement8()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        if (true)
            if (true)
                if (true)
                    return;
                else
                    return;

    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Label1()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
    Label:
        Console.WriteLine(1);

    }
}

";

            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Label2()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
    Label: Console.WriteLine(1);

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Label3()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        switch(args.GetType())
        {
            case 1:
                Console.WriteLine(1);

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Label4()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        switch(args.GetType())
        {
            case 1: Console.WriteLine(1);

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Label5()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        switch(args.GetType())
        {
            case 1:

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Label6()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
    Label:

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression1()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var a = from c in

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 20);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression2()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var a = from c in b

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression3()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var a = from c in b.

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression4()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var a = from c in b where c > 10

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression5()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var a = from c in
                    from b in G

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 20);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression6()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var a = from c in
                    from b in G
                    select b

    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: 20);
        }

        [Fact]
        [WorkItem(538779)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression7()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var q = from string s in args

                where s == null
                select s;
    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 16);
        }

        [Fact]
        [WorkItem(538779)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression8()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var q = from string s in args.
                              b.c.

                where s == null
                select s;
    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 30);
        }

        [Fact]
        [WorkItem(538780)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression9()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var q = from string s in args
                where s == null

                select s;
    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 16);
        }

        [Fact]
        [WorkItem(538780)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void QueryExpression10()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var q = from string s in args
                where s == null
                        == 1

                select s;
    }
}

";
            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: 24);
        }

        [WorkItem(538333)]
        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Statement1()
        {
            var code = @"class Program
{
    void Test() { }

}";
            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 4);
        }

        [WorkItem(538933)]
        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void EndOfFile1()
        {
            var code = @"class Program
{
    void Test() 
    {
        int i;


";
            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 8);
        }

        [WorkItem(539059)]
        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void VerbatimString()
        {
            var code = @"class Program
{
    void Test() 
    {
        var foo = @""Foo

";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 0);
        }

        [Fact]
        [WorkItem(539892)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Bug5994()
        {
            var code = @"using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var studentQuery =
            from student in students
               group student by (avg == 0 ? 0 : avg / 10) into g

            ;
    }
}
";

            AssertSmartIndent(
                code,
                indentationLine: 11,
                expectedIndentation: 15);
        }

        [Fact]
        [WorkItem(539990)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Bug6124()
        {
            var code = @"class Program
{
    void Main()
    {
        var commandLine = string.Format(
            "",
            0,
            42,
            string.Format("",
                0,
                0),

            0);
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 11,
                expectedIndentation: 12);
        }

        [Fact]
        [WorkItem(539990)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void Bug6124_1()
        {
            var code = @"class Program
{
    void Main()
    {
        var commandLine = string.Format(
            "",
            0,
            42,
            string.Format("",
                0,
                0

),
            0);
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 11,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void AfterIfWithSingleStatementInTopLevelMethod_Bug7291_1()
        {
            var code = @"int fact(int x)
{
    if (x < 1)
        return 1;

";
            AssertSmartIndent(
                code,
                indentationLine: 4,
                expectedIndentation: 4,
                options: Options.Script);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void AfterIfWithSingleStatementInTopLevelMethod_Bug7291_2()
        {
            var code = @"int fact(int x)
{
    if (x < 1)
        return 1;

}
";
            AssertSmartIndent(
                code,
                indentationLine: 4,
                expectedIndentation: 4,
                options: Options.Script);
        }

        [WorkItem(540634)]
        [WorkItem(544268)]
        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void FirstArgumentInArgumentList()
        {
            var code = @"class Program
{
    public Program(
        string a,
        int b,
        bool c)
        : this(
            a,
            new Program(

                "",
                3,
                true),
            b,
            c)
    {
    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 9,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void ForLoop()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        for (;      
        ;) { }
    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void CallBaseCtor()
        {
            var code = @"class Program
{
    public Program() :           
    base() { }
}";

            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 8);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void MultipleDeclarations()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        int i,
        j = 42;
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void CloseBracket()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var i = new int[1]
        ;
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void SwitchLabel()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        switch (args[0])
        {
            case ""foo"":

            case ""bar"":
                break;
        }
    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: 16);
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void TypeParameters()
        {
            var code = @"class Program
{
    static void Foo<T1,                 
T2>() { }
}";
            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 8);
        }

        [Fact]
        [WorkItem(542428)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void TypeArguments()
        {
            var code = @"class Program
{
        static void Foo<T1, T2>(T1 t1, T2 t2) { }

        static void Main(string[] args)
        {
            Foo<int, 
            int>(4, 2);
        }
}";
            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: 16);
        }

        [Fact]
        [WorkItem(542983)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void ConstructorInitializer1()
        {
            var code = @"public class Asset
{
    public Asset() : this(

";
            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 8);
        }

        [Fact]
        [WorkItem(542983)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void ConstructorInitializer2()
        {
            var code = @"public class Asset
{
    public Asset()
        : this(

";
            AssertSmartIndent(
                code,
                indentationLine: 4,
                expectedIndentation: 14);
        }

        [Fact]
        [WorkItem(542983)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void ConstructorInitializer3()
        {
            var code = @"public class Asset
{
    public Asset() :
        this(

";
            AssertSmartIndent(
                code,
                indentationLine: 4,
                expectedIndentation: 12);
        }

        [Fact]
        [WorkItem(543131)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void LockStatement1()
        {
            var code = @"using System;
class Program
{
    static object lockObj = new object();
    static int Main()
    {
        int sum = 0;
        lock (lockObj)
            try
            { sum = 0; }

        return sum;
    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 12);
        }

        [Fact]
        [WorkItem(543533)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void ConstructorInitializer()
        {
            var code = @"public class Asset
{
    public Asset() :

";
            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 8);
        }

        [Fact]
        [WorkItem(952803)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void ArrayInitializer()
        {
            var code = @"using System.Collections.ObjectModel;

class Program
{
    static void Main(string[] args)
    {
        new ReadOnlyCollection<int>(new int[]
        {

        });
    }
}
";
            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 12);
        }

        [Fact]
        [WorkItem(543563)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void LambdaEmbededInExpression()
        {
            var code = @"using System;
using System.Collections.Generic;
using System.Linq;
 
class Program
{
    static void Main(string[] args)
    {
        using (var var = new FooClass(() =>
        {

        }))
        {
            var var2 = var;
        }
    }
}
 
class FooClass : IDisposable
{
    public FooClass(Action a)
    {
    }
 
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 12);
        }

        [Fact]
        [WorkItem(543563)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void LambdaEmbededInExpression_1()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        using (var var = new FooClass(() =>

    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 16);
        }

        [Fact]
        [WorkItem(543563)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void LambdaEmbededInExpression_3()
        {
            var code = @"using System;

class Program
{
    static void Main(string[] args)
    {
        using (var var = new FooClass(() =>
        {

        }))
        {
            var var2 = var;
        }
    }
}

class FooClass : IDisposable
{
    public FooClass(Action a)
    {
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 12);
        }

        [Fact]
        [WorkItem(543563)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void LambdaEmbededInExpression_2()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        using (var var = new FooClass(
            () =>

    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 16);
        }

        [Fact]
        [WorkItem(543563)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void LambdaEmbededInExpression_4()
        {
            var code = @"using System;
class Class
{
    public void Method()
    {
        OtherMethod(() =>
        {
            var aaa = new object(); if (aaa != null)
            {
                var bbb = new object();

            }
        });
    }
    private void OtherMethod(Action action) { }
}";
            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 16);
        }

        [Fact]
        [WorkItem(530074)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void EnterInArgumentList1()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        Main(args,

    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
        }

        [Fact]
        [WorkItem(530074)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void EnterInArgumentList2()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        Main(args,
)
    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
        }

        [Fact]
        [WorkItem(806266)]
        [Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void EnterInArgumentList3()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var s = string.Format(1,

    }
}";
            AssertSmartIndent(
                code,
                indentationLine: 5,
                expectedIndentation: 12);
        }

        [WorkItem(9216, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void FollowPreviousLineInMultilineStatements()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var accessibleConstructors = normalType.InstanceConstructors
                                       .Where(c => c.IsAccessibleWithin(within))
                                       .Where(s => s.IsEditorBrowsable(document.ShouldHideAdvancedMembers(), semanticModel.Compilation))
.Sort(symbolDisplayService, invocationExpression.GetLocation(), semanticModel);
    }
}";
            AssertSmartIndent(code, indentationLine: 7, expectedIndentation: 39);
        }

        [WorkItem(648068)]
        [WorkItem(674611)]
        [Fact(Skip = "674611"), Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void AtBeginningOfSpanInNugget()
        {
            var markup = @"class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|
$$Console.WriteLine();|]|}
#line default
#line hidden
    }
}";
            AssertSmartIndentInProjection(
                markup, BaseIndentationOfNugget + 4);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void AtEndOfSpanInNugget()
        {
            var markup = @"class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|Console.WriteLine();
$$|]|}
#line default
#line hidden
    }
}";
            AssertSmartIndentInProjection(
                markup, BaseIndentationOfNugget + 4);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void InMiddleOfSpanAtStartOfNugget()
        {
            var markup = @"class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|Console.Wri
$$teLine();|]|}
#line default
#line hidden
    }
}";

            // Again, it doesn't matter where Console _is_ in this case -we format based on
            // where we think it _should_ be.  So the position is one indent level past the base
            // for the nugget (where we think the statement should be), plus one more since it is
            // a continuation
            AssertSmartIndentInProjection(
                markup, BaseIndentationOfNugget + 8);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void InMiddleOfSpanInsideOfNugget()
        {
            var markup = @"class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|
              Console.Wri
$$teLine();|]|}
#line default
#line hidden
    }
}";

            // Again, it doesn't matter where Console _is_ in this case -we format based on
            // where we think it _should_ be.  So the position is one indent level past the base
            // for the nugget (where we think the statement should be), plus one more since it is
            // a continuation
            AssertSmartIndentInProjection(
                markup, BaseIndentationOfNugget + 8);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void AfterStatementInNugget()
        {
            var markup = @"class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|
              Console.WriteLine();
$$
            |]|}
#line default
#line hidden
    }
}";
            AssertSmartIndentInProjection(
                markup, BaseIndentationOfNugget + 4);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void AfterStatementOnFirstLineOfNugget()
        {
            var markup = @"class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|Console.WriteLine();
$$
|]|}
#line default
#line hidden
    }
}";

            // TODO: Fix this to indent relative to the previous statement,
            // instead of relative to the containing scope.  I.e. Format like:
            //     <%Console.WriteLine();
            //       Console.WriteLine(); %>
            // instead of
            //     <%Console.WriteLine();
            //         Console.WriteLine(); %>
            // C# had the desired behavior in Dev12, where VB had the same behavior
            // as Roslyn has.  The Roslyn formatting engine currently always formats
            // each statement independently, so let's not change that just for Venus
            AssertSmartIndentInProjection(
                markup, BaseIndentationOfNugget + 4);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void InQueryOnFistLineOfNugget()
        {
            var markup = @"class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|var q = from
$$
|]|}
#line default
#line hidden
    }
}";
            AssertSmartIndentInProjection(
                markup, BaseIndentationOfNugget + 8);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void InQueryInNugget()
        {
            var markup = @"class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|
              var q = from
$$
|]|}
#line default
#line hidden
    }
}";
            AssertSmartIndentInProjection(
                markup, BaseIndentationOfNugget + 8);
        }

        [WorkItem(9216, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void InsideBracesInNugget()
        {
            var markup = @"class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
                    {|S1:[|if (true)
        {
$$
        }|]|}
#line default
#line hidden
    }
}";
            AssertSmartIndentInProjection(markup, BaseIndentationOfNugget + 8);
        }

        [WorkItem(9216, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void AfterEmbeddedStatementOnFirstLineOfNugget()
        {
            var markup = @"class Program
        {
            static void Main(string[] args)
            {
        #line ""Foo.aspx"", 27
                            {|S1:[|if (true)
                {
                }
                $$
|]|}
        #line default
        #line hidden
            }
        }";

            // In this case, we align the next statement with the "if" (though we _don't_
            // align the braces with it :S)
            AssertSmartIndentInProjection(markup,
                expectedIndentation: BaseIndentationOfNugget + 2);
        }

        [WorkItem(9216, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void AfterEmbeddedStatementInNugget()
        {
            var markup = @"class Program
        {
            static void Main(string[] args)
            {
        #line ""Foo.aspx"", 27
                            {|S1:[|
            if (true)
            {
            }
$$
|]|}
        #line default
        #line hidden
            }
        }";

            // In this case we alsign with the "if", - the base indentation we pass in doesn't matter.
            AssertSmartIndentInProjection(markup,
                expectedIndentation: BaseIndentationOfNugget + 4);
        }

        // this is the special case where the smart indenter 
        // aligns with the base or base + 4th position.
        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void AfterSwitchStatementAtEndOfNugget()
        {
            var markup = @"
class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|switch (10)
            {
                case 10:
$$
            }|]|}
#line default
#line hidden
    }
}";

            // It's yuck that I saw differences depending on where the end of the nugget is
            // but I did, so lets add a test.
            AssertSmartIndentInProjection(markup,
                expectedIndentation: BaseIndentationOfNugget + 12);
        }

        // this is the special case where the smart indenter 
        // aligns with the base or base + 4th position.
        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void AfterSwitchStatementInNugget()
        {
            var markup = @"
class Program
{
    static void Main(string[] args)
    {
#line ""Foo.aspx"", 27
            {|S1:[|switch (10)
            {
                case 10:
$$
            }
|]|}
#line default
#line hidden
    }
}";

            AssertSmartIndentInProjection(markup,
                expectedIndentation: BaseIndentationOfNugget + 12);
        }

        [Fact, WorkItem(529876), Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void InEmptyNugget()
        {
            var markup = @"class Program
        {
            static void Main(string[] args)
            {
        #line ""Foo.aspx"", 27
            {|S1:[|
$$|]|}
        #line default
        #line hidden
            }
        }";

            AssertSmartIndentInProjection(markup,
                expectedIndentation: BaseIndentationOfNugget + 4);
        }

        [WorkItem(1190278)]
        [Fact, Trait(Traits.Feature, Traits.Features.SmartIndent), Trait(Traits.Feature, Traits.Features.Venus)]
        public void GetNextTokenForFormattingSpanCalculationIncludesZeroWidthToken_CS()
        {
            var markup = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP {
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Security;
using System.Web.UI;
using System.Web.WebPages;
using System.Web.WebPages.Html;
using WebMatrix.Data;
using WebMatrix.WebData;
using Microsoft.Web.WebPages.OAuth;
using DotNetOpenAuth.AspNet;

public class _Page_Default_cshtml : System.Web.WebPages.WebPage {
#line hidden
public _Page_Default_cshtml() {
}
protected System.Web.HttpApplication ApplicationInstance {
get {
return ((System.Web.HttpApplication)(Context.ApplicationInstance));
}
}
public override void Execute() {

#line 1 ""C:\Users\basoundr\Documents\Visual Studio 2015\WebSites\WebSite6\Default.cshtml""

    {|S1:[|public class LanguagePreference
        {

        }

if (!File.Exists(physicalPath))
{
    Context.Response.SetStatus(HttpStatusCode.NotFound);
    return;
}
$$
    string[] languages = Context.Request.UserLanguages;

if(languages == null || languages.Length == 0)
{

    Response.Redirect()
    }

|]|}
#line default
#line hidden
}
}
}";

            AssertSmartIndentInProjection(markup,
                expectedIndentation: 16);
        }

        [Fact, WorkItem(530948), Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void CommaSeparatedListEnumMembers()
        {
            var code = @"enum MyEnum
{
    e1,

}";

            AssertSmartIndent(
                code,
                indentationLine: 3,
                expectedIndentation: 4);
        }

        [Fact, WorkItem(530796), Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void RelativeIndentationForBracesInExpression()
        {
            var code = @"class C
{
    void M(C c)
    {
        M(new C()
        {

        });
    }
}
";

            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 12);
        }

        [Fact, WorkItem(584599), Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void SwitchSection()
        {
            var code = @"class C
{
    void Method()
    {
        switch (i)
        {

            case 1:

            case 2:

                int i2 = 10;

            case 4:

        }
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 6,
                expectedIndentation: 12);

            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 16);

            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 16);

            AssertSmartIndent(
                code,
                indentationLine: 12,
                expectedIndentation: 16);

            AssertSmartIndent(
                code,
                indentationLine: 14,
                expectedIndentation: 16);
        }

        [Fact, WorkItem(584599), Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void SwitchSection2()
        {
            var code = @"class C
{
    void Method()
    {
        switch (i)
        {
            // test

            case 1:
                // test

            case 2:
                // test

                int i2 = 10;
            // test

            case 4:
            // test

        }
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 7,
                expectedIndentation: 12);

            AssertSmartIndent(
                code,
                indentationLine: 10,
                expectedIndentation: 16);

            AssertSmartIndent(
                code,
                indentationLine: 13,
                expectedIndentation: 16);

            AssertSmartIndent(
                code,
                indentationLine: 16,
                expectedIndentation: 12);

            AssertSmartIndent(
                code,
                indentationLine: 19,
                expectedIndentation: 12);
        }

        [Fact, WorkItem(584599), Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void CommentAtTheEndOfLine()
        {
            var code = @"using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(); /* this is a comment */
                             // that I would like to keep


        // properly indented
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 29);

            AssertSmartIndent(
                code,
                indentationLine: 9,
                expectedIndentation: 8);
        }

        [Fact, WorkItem(912735), Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void CommentAtTheEndOfLineWithExecutableAfterCaret()
        {
            var code = @"using System;

class Program
{
    static void Main(string[] args)
    {
        // A
        // B


        return;
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 8,
                expectedIndentation: 8);

            AssertSmartIndent(
                code,
                indentationLine: 9,
                expectedIndentation: 8);
        }

        [Fact, WorkItem(912735), Trait(Traits.Feature, Traits.Features.SmartIndent)]
        public void CommentAtTheEndOfLineInsideInitializer()
        {
            var code = @"using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        var s = new List<string>
                        {
                            """",
                                    """",/*sdfsdfsdfsdf*/
                                       // dfsdfsdfsdfsdf


                        };
    }
}";

            AssertSmartIndent(
                code,
                indentationLine: 12,
                expectedIndentation: 39);

            AssertSmartIndent(
                code,
                indentationLine: 13,
                expectedIndentation: 36);
        }

        private static void AssertSmartIndentInProjection(string markup, int expectedIndentation, CSharpParseOptions options = null)
        {
            var optionsSet = options != null
                    ? new[] { options }
                    : new[] { Options.Regular, Options.Script };

            foreach (var option in optionsSet)
            {
                using (var workspace = CSharpWorkspaceFactory.CreateWorkspaceFromLines(new string[] { markup }, parseOptions: option))
                {
                    var subjectDocument = workspace.Documents.Single();

                    var projectedDocument =
                        workspace.CreateProjectionBufferDocument(HtmlMarkup, workspace.Documents, LanguageNames.CSharp);

                    var provider = workspace.Services.GetService<IHostDependentFormattingRuleFactoryService>()
                                        as TestFormattingRuleFactoryServiceFactory.Factory;
                    if (provider != null)
                    {
                        provider.BaseIndentation = BaseIndentationOfNugget;
                        provider.TextSpan = subjectDocument.SelectedSpans.Single();
                    }

                    var indentationLine = projectedDocument.TextBuffer.CurrentSnapshot.GetLineFromPosition(projectedDocument.CursorPosition.Value);
                    var point = projectedDocument.GetTextView().BufferGraph.MapDownToBuffer(indentationLine.Start, PointTrackingMode.Negative, subjectDocument.TextBuffer, PositionAffinity.Predecessor);

                    TestIndentation(point.Value, expectedIndentation, projectedDocument.GetTextView(), subjectDocument);
                }
            }
        }

        private static void AssertSmartIndent(
            string code,
            int indentationLine,
            int? expectedIndentation,
            CSharpParseOptions options = null)
        {
            var optionsSet = options != null
                ? new[] { options }
                : new[] { Options.Regular, Options.Script };

            foreach (var option in optionsSet)
            {
                using (var workspace = CSharpWorkspaceFactory.CreateWorkspaceFromFile(code, parseOptions: option))
                {
                    TestIndentation(indentationLine, expectedIndentation, workspace);
                }
            }
        }

        private static void AssertSmartIndent(
            string code,
            int? expectedIndentation,
            CSharpParseOptions options = null)
        {
            var optionsSet = options != null
                ? new[] { options }
                : new[] { Options.Regular, Options.Script };

            foreach (var option in optionsSet)
            {
                using (var workspace = CSharpWorkspaceFactory.CreateWorkspaceFromFile(code, parseOptions: option))
                {
                    var wpfTextView = workspace.Documents.First().GetTextView();
                    var line = wpfTextView.TextBuffer.CurrentSnapshot.GetLineFromPosition(wpfTextView.Caret.Position.BufferPosition).LineNumber;
                    TestIndentation(line, expectedIndentation, workspace);
                }
            }
        }
    }
}
