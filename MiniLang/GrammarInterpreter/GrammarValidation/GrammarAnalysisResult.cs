using MiniLang.TokenObjects;

namespace MiniLang.GrammarInterpreter
{
    public record GrammarAnalysisResult
    {
        public bool HasError { get; init; }
        public string? ErrorMessage { get; init; }
        public Token? Node { get; init; }

        public static GrammarAnalysisResult Success(Token node) =>
            new GrammarAnalysisResult { HasError = false, Node = node };

        public static GrammarAnalysisResult Error(string message) =>
            new GrammarAnalysisResult { HasError = true, ErrorMessage = message };
    }
}
