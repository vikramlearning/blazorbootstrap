namespace BlazorBootstrap;

/// <summary>
/// Represents a committed RichTextEditor content change.
/// </summary>
public sealed record RichTextEditorChange(string Html, string Text, int CharacterCount, int WordCount);
