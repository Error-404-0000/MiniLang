public static class MiniLangPreprocessor
{
    /// <summary>
    /// Removes all lines that start with "@@" (MiniLang comment syntax).
    /// </summary>
    public static string RemoveCommentLines(string source)
    {
        var lines = source.Split('\n');
        var cleaned = lines
            .Where(line => !line.TrimStart().StartsWith("@@"))
            .ToArray();
        return string.Join('\n', cleaned);
    }
}
