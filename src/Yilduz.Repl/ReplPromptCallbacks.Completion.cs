using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PrettyPrompt.Completion;
using PrettyPrompt.Documents;

namespace Yilduz.Repl;

public sealed partial class ReplPromptCallbacks
{
    protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(
        string text,
        int caret,
        TextSpan spanToBeReplaced,
        CancellationToken cancellationToken
    )
    {
        if (text is "#")
        {
            return Task.FromResult<IReadOnlyList<CompletionItem>>(
                [
                    new("c", "Clear the console"),
                    new("r", "Reset the engine"),
                    new("q", "Quit the REPL"),
                ]
            );
        }

        return base.GetCompletionItemsAsync(text, caret, spanToBeReplaced, cancellationToken);
    }
}
