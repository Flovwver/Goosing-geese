using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class FormatOwnedCode
{
    public static string Main(bool checkOnly = true)
    {
        string root = Path.GetFullPath(".");
        string[] folders =
        {
            "Assets/Goosing geese/Scripts",
            "Assets/Goosing geese/Editor",
            "Tests/Architecture",
            "Tools/CodeStyle"
        };
        var changed = new List<string>();

        foreach (string folder in folders)
        {
            foreach (string file in Directory.GetFiles(Path.Combine(root, folder), "*.cs", SearchOption.AllDirectories))
            {
                string normalizedPath = file.Replace('\\', '/');

                if (normalizedPath.Contains("/obj/") || normalizedPath.Contains("/bin/"))
                    continue;

                string source = File.ReadAllText(file).Replace("\r\n", "\n");
                var tree = CSharpSyntaxTree.ParseText(source);

                if (tree.GetDiagnostics().Any(item => item.Severity == DiagnosticSeverity.Error))
                    throw new InvalidOperationException("Cannot format invalid C#: " + file);

                var rewritten = new Rules().Visit(tree.GetRoot());
                string formatted = WrapLines(SeparateFlow(SeparateMembers(rewritten.NormalizeWhitespace("    ", "\n").ToFullString()))) + "\n";
                formatted = string.Join("\n", formatted.Split('\n').Select(line => line.TrimEnd()));

                if (source == formatted)
                    continue;

                changed.Add(Path.GetRelativePath(root, file));

                if (checkOnly == false)
                    File.WriteAllText(file, formatted, new UTF8Encoding(false));
            }
        }

        if (checkOnly && changed.Count > 0)
            throw new InvalidOperationException("Style violations:\n" + string.Join("\n", changed));

        return checkOnly ? "PASS: owned C# member order, explicit false comparisons and control-flow spacing." : "Formatted " + changed.Count + " files.";
    }

    private static string SeparateFlow(string text)
    {
        var tree = CSharpSyntaxTree.ParseText(text);
        var lines = text.Split('\n').ToList();
        var boundaries = new HashSet<int>();

        foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
        {
            SyntaxList<StatementSyntax> statements;

            if (node is BlockSyntax block)
                statements = block.Statements;
            else if (node is SwitchSectionSyntax section)
                statements = section.Statements;
            else
                continue;

            for (int index = 0; index < statements.Count; index++)
            {
                StatementSyntax statement = statements[index];
                bool control = statement is IfStatementSyntax
                    || statement is ForStatementSyntax
                    || statement is ForEachStatementSyntax
                    || statement is WhileStatementSyntax
                    || statement is DoStatementSyntax
                    || statement is SwitchStatementSyntax
                    || statement is TryStatementSyntax
                    || statement is UsingStatementSyntax
                    || statement is LockStatementSyntax;

                if (control == false)
                    continue;

                var span = tree.GetLineSpan(statement.Span);

                if (index > 0)
                    boundaries.Add(span.StartLinePosition.Line);

                if (index + 1 < statements.Count)
                    boundaries.Add(span.EndLinePosition.Line + 1);
            }
        }

        foreach (int line in boundaries.OrderByDescending(value => value))
        {
            if (line > 0 && line < lines.Count && string.IsNullOrWhiteSpace(lines[line - 1]) == false && string.IsNullOrWhiteSpace(lines[line]) == false)
                lines.Insert(line, "");
        }

        return string.Join("\n", lines).TrimEnd();
    }

    private static string SeparateMembers(string text)
    {
        var tree = CSharpSyntaxTree.ParseText(text);
        var lines = text.Split('\n').ToList();
        var boundaries = new HashSet<int>();

        foreach (var type in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            for (int index = 1; index < type.Members.Count; index++)
            {
                var previous = type.Members[index - 1];
                var current = type.Members[index];
                bool compact = previous.Kind() == current.Kind()
                    && (current is FieldDeclarationSyntax
                    || current is EventFieldDeclarationSyntax
                    || current is PropertyDeclarationSyntax);

                if (compact == false)
                    boundaries.Add(tree.GetLineSpan(current.Span).StartLinePosition.Line);
            }
        }

        foreach (int line in boundaries.OrderByDescending(value => value))
        {
            if (line > 0 && string.IsNullOrWhiteSpace(lines[line - 1]) == false)
                lines.Insert(line, "");
        }

        return string.Join("\n", lines);
    }

    private static string WrapLines(string text)
    {
        var tree = CSharpSyntaxTree.ParseText(text);
        var source = tree.GetText();
        var insertions = new SortedDictionary<int, string>();

        foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
        {
            var span = tree.GetLineSpan(node.Span);

            if (span.StartLinePosition.Line != span.EndLinePosition.Line)
                continue;

            var line = source.Lines[span.StartLinePosition.Line];

            if (line.Span.Length <= 160)
                continue;

            int indentation = line.ToString().TakeWhile(char.IsWhiteSpace).Count() + 4;
            string nextLine = "\n" + new string (' ', indentation);

            if (node is BinaryExpressionSyntax binary
                && (binary.IsKind(SyntaxKind.LogicalAndExpression)
                || binary.IsKind(SyntaxKind.LogicalOrExpression)
                || (binary.IsKind(SyntaxKind.AddExpression)
                && binary.Ancestors().OfType<BracketedArgumentListSyntax>().Any() == false)))
                insertions[binary.OperatorToken.SpanStart] = nextLine;

            if (node is ArgumentListSyntax arguments && arguments.Arguments.Count > 1)
            {
                foreach (SyntaxToken separator in arguments.Arguments.GetSeparators())
                    insertions[separator.Span.End] = nextLine;
            }

            if (node is ParameterListSyntax parameters && parameters.Parameters.Count > 2)
            {
                foreach (SyntaxToken separator in parameters.Parameters.GetSeparators())
                    insertions[separator.Span.End] = nextLine;
            }
        }

        var output = new StringBuilder(text);

        foreach (var insertion in insertions.Reverse())
        {
            int position = insertion.Key;
            int whitespace = 0;

            while (position + whitespace < text.Length && text[position + whitespace] == ' ')
                whitespace++;

            output.Remove(position, whitespace);
            output.Insert(position, insertion.Value);
        }

        return output.ToString();
    }

    private sealed class Rules : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var visited = (CompilationUnitSyntax)base.VisitCompilationUnit(node);
            return visited.WithUsings(SyntaxFactory.List(visited.Usings.OrderBy(item => item.Name.ToString().StartsWith("System",
                StringComparison.Ordinal) ? 0 : 1).ThenBy(item => item.Name.ToString(),
                StringComparer.Ordinal)));
        }

        public override SyntaxNode VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var visited = (PrefixUnaryExpressionSyntax)base.VisitPrefixUnaryExpression(node);

            if (visited.IsKind(SyntaxKind.LogicalNotExpression) == false)
                return visited;

            return SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression,
                visited.Operand.WithoutTrivia(),
                SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))).WithTriviaFrom(visited);
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            var visited = (IfStatementSyntax)base.VisitIfStatement(node);

            if (visited.Condition is ParenthesizedExpressionSyntax expression)
                return visited.WithCondition(expression.Expression);

            return visited;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var visited = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
            var ordered = visited.Members.Select((member, index) => new { Member = member, Index = index }).OrderBy(item => Kind(item.Member)).ThenBy(item => Callback(item.Member)).ThenBy(item => Access(item.Member)).ThenBy(item => IsStatic(item.Member) ? 0 : 1).ThenBy(item => IsReadonly(item.Member) ? 0 : 1).ThenBy(item => item.Index).Select(item => item.Member);
            return visited.WithMembers(SyntaxFactory.List(ordered));
        }

        private static int Kind(MemberDeclarationSyntax member)
        {
            if (member is FieldDeclarationSyntax field)
                return field.Modifiers.Any(SyntaxKind.ConstKeyword) ? 0 : 1;

            if (member is ConstructorDeclarationSyntax)
                return 2;

            if (member is DestructorDeclarationSyntax)
                return 3;

            if (member is DelegateDeclarationSyntax)
                return 4;

            if (member is EventDeclarationSyntax || member is EventFieldDeclarationSyntax)
                return 5;

            if (member is EnumDeclarationSyntax)
                return 6;

            if (member is InterfaceDeclarationSyntax)
                return 7;

            if (member is PropertyDeclarationSyntax)
                return 8;

            if (member is IndexerDeclarationSyntax)
                return 9;

            if (member is MethodDeclarationSyntax)
                return 10;

            if (member is StructDeclarationSyntax)
                return 11;

            return 12;
        }

        private static int Callback(MemberDeclarationSyntax member)
        {
            if (member is MethodDeclarationSyntax method == false)
                return 100;

            string name = ((MethodDeclarationSyntax)member).Identifier.Text;
            string[] phases =
            {
                "Reset",
                "OnValidate",
                "Awake",
                "OnEnable",
                "Start",
                "FixedUpdate",
                "OnCollisionEnter",
                "OnCollisionStay",
                "OnCollisionExit",
                "OnTriggerEnter",
                "OnTriggerStay",
                "OnTriggerExit",
                "Update",
                "LateUpdate",
                "OnPopulateMesh",
                "OnGUI",
                "OnDrawGizmos",
                "OnDrawGizmosSelected",
                "OnApplicationFocus",
                "OnApplicationPause",
                "OnApplicationQuit",
                "OnDisable",
                "OnDestroy"
            };
            int index = Array.IndexOf(phases, name);
            return index < 0 ? 100 : index;
        }

        private static int Access(MemberDeclarationSyntax member)
        {
            var modifiers = member.Modifiers;

            if (modifiers.Any(SyntaxKind.PublicKeyword))
                return 0;

            if (modifiers.Any(SyntaxKind.ProtectedKeyword) && modifiers.Any(SyntaxKind.InternalKeyword))
                return 2;

            if (modifiers.Any(SyntaxKind.InternalKeyword))
                return 1;

            if (modifiers.Any(SyntaxKind.ProtectedKeyword) && modifiers.Any(SyntaxKind.PrivateKeyword))
                return 4;

            if (modifiers.Any(SyntaxKind.ProtectedKeyword))
                return 3;

            return 5;
        }

        private static bool IsStatic(MemberDeclarationSyntax member)
        {
            return member.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        private static bool IsReadonly(MemberDeclarationSyntax member)
        {
            return member is FieldDeclarationSyntax && member.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
        }
    }
}
