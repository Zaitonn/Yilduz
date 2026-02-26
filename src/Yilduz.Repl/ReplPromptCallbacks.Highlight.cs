using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acornima;
using Acornima.Ast;
using PrettyPrompt.Highlighting;

namespace Yilduz.Repl;

public sealed partial class ReplPromptCallbacks
{
    protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(
        string text,
        CancellationToken cancellationToken
    )
    {
        var spans = new List<FormatSpan>();
        var stringLikeRanges = new List<(int start, int end)>();

        if (text is "#" or "#c" or "#r" or "#q" or "exit")
        {
            spans.Add(new(0, text.Length, FormatCommand));
        }
        else
        {
            try
            {
                var script = _parser.ParseScript(text);

                CollectSpans(spans, script, stringLikeRanges);
                AddCommentSpans(text, spans, stringLikeRanges);
            }
            catch (SyntaxErrorException ex)
            {
                var errorIndex = GetIndexFromLineColumn(text, ex.LineNumber, ex.Column);

                if (errorIndex >= 0 && errorIndex < text.Length)
                {
                    spans.Add(new(errorIndex, 1, FormatError));
                }
            }
        }

        return Task.FromResult<IReadOnlyCollection<FormatSpan>>(spans);
    }

    private static int GetIndexFromLineColumn(string text, int lineNumber, int column)
    {
        if (lineNumber < 1 || column < 0)
        {
            return -1;
        }

        var index = 0;
        var currentLine = 1;

        while (index < text.Length && currentLine < lineNumber)
        {
            if (text[index] == '\n')
            {
                currentLine++;
            }

            index++;
        }

        if (currentLine != lineNumber)
        {
            return -1;
        }

        var target = index + column;
        return target <= text.Length ? target : text.Length;
    }

    private static void CollectSpans(
        List<FormatSpan> spans,
        INode node,
        List<(int start, int end)> stringLikeRanges,
        INode? parent = null
    )
    {
        if (node == null)
        {
            return;
        }

        switch (node)
        {
            case StringLiteral:
                stringLikeRanges.Add((node.Start, node.End));
                spans.Add(GetFormatSpanWithFormat(node, FormatString));
                break;

            case RegExpLiteral regExpLiteral:
                stringLikeRanges.Add((node.Start, node.End));
                spans.Add(
                    GetFormatSpanWithFormat(
                        regExpLiteral.Start,
                        regExpLiteral.End - regExpLiteral.RegExp.Flags.Length,
                        FormatRegExp
                    )
                );

                spans.Add(
                    GetFormatSpanWithFormat(
                        regExpLiteral.End - regExpLiteral.RegExp.Flags.Length,
                        regExpLiteral.End,
                        FormatRegExpFlags
                    )
                );

                break;

            case TemplateLiteral templateLiteral:
                stringLikeRanges.Add((templateLiteral.Start, templateLiteral.Start + 1)); // opening `
                stringLikeRanges.Add((templateLiteral.End - 1, templateLiteral.End)); // closing `

                spans.Add(
                    GetFormatSpanWithFormat(
                        templateLiteral.Start,
                        templateLiteral.Start + 1,
                        FormatString
                    )
                );
                spans.Add(
                    GetFormatSpanWithFormat(
                        templateLiteral.End - 1,
                        templateLiteral.End,
                        FormatString
                    )
                );
                foreach (var quasi in templateLiteral.Quasis)
                {
                    stringLikeRanges.Add((quasi.Start, quasi.End));
                    spans.Add(GetFormatSpanWithFormat(quasi, FormatString));
                }

                foreach (var expression in templateLiteral.Expressions)
                {
                    spans.Add(
                        GetFormatSpanWithFormat(
                            expression.Start - 2,
                            expression.Start,
                            FormatTemplateTag
                        )
                    );

                    spans.Add(
                        GetFormatSpanWithFormat(
                            expression.End,
                            expression.End + 1,
                            FormatTemplateTag
                        )
                    );
                }

                break;

            case CallExpression { Callee: MemberExpression memberExpression }:
                spans.Add(GetFormatSpanWithFormat(memberExpression.Property, FormatCall));
                break;

            case CallExpression { Callee: Identifier identifier }:
                spans.Add(GetFormatSpanWithFormat(identifier, FormatCall));
                break;

            case CallExpression { Callee: Super super }:
                spans.Add(GetFormatSpanWithFormat(super, FormatKeyword));
                break;

            case Identifier:
                spans.Add(GetFormatSpanWithFormat(node, FormatIdentifier));
                break;

            case NumericLiteral:
                spans.Add(GetFormatSpanWithFormat(node, FormatNumber));
                break;

            case MethodDefinition methodDefinition
                when methodDefinition.Kind == PropertyKind.Method:
                spans.Add(GetFormatSpanWithFormat(methodDefinition.Key, FormatCall));

                spans.Add(
                    GetFormatSpanWithFormat(
                        methodDefinition.Start,
                        methodDefinition.Key.Start,
                        FormatKeyword
                    )
                );
                break;

            case MethodDefinition methodDefinition
                when methodDefinition.Kind == PropertyKind.Constructor:
                spans.Add(GetFormatSpanWithFormat(methodDefinition.Key, FormatKeyword));
                break;

            case MethodDefinition methodDefinition
                when methodDefinition.Kind is PropertyKind.Get or PropertyKind.Set:
                spans.Add(
                    GetFormatSpanWithFormat(
                        methodDefinition.Start,
                        methodDefinition.Key.Start,
                        FormatKeyword
                    )
                );
                break;

            case FunctionDeclaration functionDeclaration when functionDeclaration.Id is not null:
                spans.Add(
                    GetFormatSpanWithFormat(
                        functionDeclaration.Start,
                        functionDeclaration.Id.Start,
                        FormatKeyword
                    )
                );
                break;

            case VariableDeclaration variableDeclaration
                when variableDeclaration.Declarations.Count > 0:
                spans.Add(
                    GetFormatSpanWithFormat(
                        variableDeclaration.Start,
                        variableDeclaration.Declarations[0].Start,
                        FormatKeyword
                    )
                );
                break;

            case NewExpression newExpression:
                spans.Add(
                    GetFormatSpanWithFormat(
                        newExpression.Start,
                        newExpression.Callee.Start,
                        FormatKeyword
                    )
                );

                spans.Add(GetFormatSpanWithFormat(newExpression.Callee, FormatType));
                break;

            case ClassDeclaration classDeclaration when classDeclaration.Id is not null:
                spans.Add(
                    GetFormatSpanWithFormat(
                        classDeclaration.Start,
                        classDeclaration.Id.Start,
                        FormatKeyword
                    )
                );

                if (classDeclaration.SuperClass is not null)
                {
                    spans.Add(
                        GetFormatSpanWithFormat(
                            classDeclaration.Id.End,
                            classDeclaration.SuperClass.Start,
                            FormatKeyword
                        )
                    );

                    spans.Add(GetFormatSpanWithFormat(classDeclaration.SuperClass, FormatType));
                }

                spans.Add(GetFormatSpanWithFormat(classDeclaration.Id, FormatType));
                break;

            case ArrowFunctionExpression arrowFunctionExpression:
                spans.Add(
                    GetFormatSpanWithFormat(
                        arrowFunctionExpression.Params.Max(param => param.End),
                        arrowFunctionExpression.Body.Start,
                        FormatKeyword
                    )
                );
                break;
        }

        foreach (var child in node.ChildNodes)
        {
            CollectSpans(spans, child, stringLikeRanges, node);
        }
    }

    private static FormatSpan GetFormatSpanWithFormat(int start, int end, ConsoleFormat format)
    {
        return new(start, end - start, format);
    }

    private static FormatSpan GetFormatSpanWithFormat(INode node, ConsoleFormat format)
    {
        return GetFormatSpanWithFormat(node.Start, node.End, format);
    }

    private static void AddCommentSpans(
        string text,
        List<FormatSpan> spans,
        List<(int start, int end)> stringLikeRanges
    )
    {
        if (text.Length == 0)
        {
            return;
        }

        stringLikeRanges.Sort((a, b) => a.start.CompareTo(b.start));

        var rangeIndex = 0;
        var i = 0;

        while (i < text.Length - 1)
        {
            // Skip over string-like regions to avoid false positives.
            while (rangeIndex < stringLikeRanges.Count && i >= stringLikeRanges[rangeIndex].end)
            {
                rangeIndex++;
            }

            if (
                rangeIndex < stringLikeRanges.Count
                && i >= stringLikeRanges[rangeIndex].start
                && i < stringLikeRanges[rangeIndex].end
            )
            {
                i = stringLikeRanges[rangeIndex].end;
                continue;
            }

            var current = text[i];
            var next = text[i + 1];

            if (current == '/' && next == '/')
            {
                var start = i;
                i += 2;
                while (i < text.Length && text[i] != '\n' && text[i] != '\r')
                {
                    i++;
                }

                spans.Add(new(start, i - start, FormatComment));
                continue;
            }

            if (current == '/' && next == '*')
            {
                var start = i;
                i += 2;
                while (i < text.Length - 1 && !(text[i] == '*' && text[i + 1] == '/'))
                {
                    i++;
                }

                if (i < text.Length - 1)
                {
                    i += 2; // consume closing */
                }
                else
                {
                    i = text.Length;
                }

                spans.Add(new(start, i - start, FormatComment));
                continue;
            }

            i++;
        }
    }
}
