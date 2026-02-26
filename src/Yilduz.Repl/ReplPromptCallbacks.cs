using System;
using Acornima;
using PrettyPrompt;
using PrettyPrompt.Highlighting;

namespace Yilduz.Repl;

public sealed partial class ReplPromptCallbacks : PromptCallbacks
{
    private readonly Parser _parser = new();

    private static readonly ConsoleFormat FormatCommand = new(
        AnsiColor.BrightYellow,
        Underline: true
    );
    private static readonly ConsoleFormat FormatError = new(Background: AnsiColor.BrightRed);
    private static readonly ConsoleFormat FormatKeyword = new(AnsiColor.Rgb(0x56, 0x9C, 0xD6));
    private static readonly ConsoleFormat FormatCall = new(AnsiColor.Rgb(0xDC, 0xDA, 0xAA));
    private static readonly ConsoleFormat FormatIdentifier = new(AnsiColor.Rgb(0x9C, 0xDC, 0xFE));
    private static readonly ConsoleFormat FormatNumber = new(AnsiColor.Rgb(0xB5, 0xCE, 0xA8));
    private static readonly ConsoleFormat FormatRegExp = new(AnsiColor.Rgb(0xD7, 0x5F, 0x5F));
    private static readonly ConsoleFormat FormatRegExpFlags = new(AnsiColor.Rgb(0x56, 0x9C, 0xD6));
    private static readonly ConsoleFormat FormatString = new(AnsiColor.Rgb(0xD6, 0x9D, 0x85));
    private static readonly ConsoleFormat FormatType = new(AnsiColor.Rgb(0x4E, 0xC9, 0xB0));
    private static readonly ConsoleFormat FormatTemplateTag = new(AnsiColor.Rgb(0xFF, 0xD7, 0x00));
    private static readonly ConsoleFormat FormatComment = new(AnsiColor.Rgb(0x57, 0xA6, 0x4A));

    public ReplPromptCallbacks()
    {
        _parser = new(new() { Tolerant = true, AllowReturnOutsideFunction = true });
    }
}
