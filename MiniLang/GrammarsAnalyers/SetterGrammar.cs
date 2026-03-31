using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.SyntaxObjects.Collections;
using MiniLang.TokenObjects;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{
    [TriggerTokenType(TriggerType.Type)]
    public class SetterGrammar : IGrammarAnalyser, IDebugger
    {
        public string GrammarName => "setter operation";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.SETTER, TokenOperation.SETTERAddOperation, TokenOperation.SETTERSubtractOperation];
        public TokenType[] TriggerTokenTypes => [TokenType.Identifier, TokenType.Array];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens.Length < 3)
            {
                errorMessage = "Setter syntax must follow: <target> <operator> <expression>";
                return true;
            }

            if (!IsValidTarget(tokens[0]))
            {
                errorMessage = "Expected identifier or array index as left-hand side of setter.";
                return true;
            }

            if (!TriggerTokensOperator.Contains(tokens[1].TokenOperation))
            {
                errorMessage = $"Invalid setter operator: {tokens[1].Value}";
                return true;
            }

            return false;
        }

        public Token BuildNode(
            Token[] tokens,
            ScopeObjectValueManager scopeObjectValueManager,
            ExpressionGrammarAnalyser expressionGrammarAnalyser,
            FunctionDeclarationScopeManager FunctionDeclarationManager,
            IGrammarInterpreter grammarInterpreter,
            int line, Action<Token> PushToken)
        {
            var target = tokens[0];

            var op = tokens[1].TokenOperation switch
            {
                TokenOperation.SETTER => SetterOperator.SETTER,
                TokenOperation.SETTERAddOperation => SetterOperator.SETTERAddOperation,
                TokenOperation.SETTERSubtractOperation => SetterOperator.SETTERSubtractOperation,
                _ => throw new Exception($"unexpect setter operator '{tokens[1].TokenOperation}'.")
            };

            var expression = tokens[2..];
            if (!expressionGrammarAnalyser.IsValidExpression(expression, out var errorMessage))
            {
                throw new Exception(errorMessage);
            }

            if (target.TokenType == TokenType.Identifier)
            {
                scopeObjectValueManager.MarkAssigned(target.Value?.ToString() ?? string.Empty);
            }

            var setterObj = new SetterSyntaxObject(target, op, expression);
            return new Token(TokenType.SETTERS, tokens[1].TokenOperation, TokenTree.Single, setterObj);
        }

        public string ViewSelf(Token Token, GrammarValidator grammarValidator = null, int indentLevel = 0)
        {
            if (Token.Value is not SetterSyntaxObject setter)
            {
                return string.Empty;
            }

            var indent = string.Join("", Enumerable.Repeat("    ", indentLevel));
            var builder = new StringBuilder();

            builder.AppendLine($"{indent}└── [SetterStatement]");
            builder.AppendLine($"{indent}    ├── [Target -> {setter.Target.Value}]");
            builder.AppendLine($"{indent}    ├── [SetterOperator -> {setter.SetterOperator}]");
            builder.AppendLine($"{indent}    └── [Expression]");
            foreach (var token in setter.Expression)
            {
                builder.AppendLine($"{indent}        └── [Token::{token.TokenType}<{token.TokenOperation}> -> {token.Value}]");
            }

            return builder.ToString();
        }

        private static bool IsValidTarget(Token token) =>
            token.TokenType == TokenType.Identifier ||
            (token.TokenType == TokenType.Array && token.Value is ArrayAccessSyntaxObject);
    }

    public enum SetterOperator
    {
        SETTER,
        SETTERAddOperation,
        SETTERSubtractOperation
    }

    public record SetterSyntaxObject(Token Target, SetterOperator SetterOperator, IEnumerable<Token> Expression);
}
