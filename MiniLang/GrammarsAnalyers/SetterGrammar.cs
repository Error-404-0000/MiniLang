using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Interfaces;
using MiniLang.Interpreter;
using MiniLang.Interpreter.GrammarDummyScopes;
using MiniLang.Interpreter.GrammarValidation;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{
    [TriggerTokenType(TiggerType.Type)]
    public class SetterGrammar : IGrammarAnalyser,IDebugger
    {
        public string GrammarName => "setter operation";

        public TokenOperation[] TriggerTokensOperator => [   TokenOperation.SETTER,
            TokenOperation.SETTERAddOperation,
            TokenOperation.SETTERSubtractOperation]; 
        public TokenType[] TriggerTokenTypes => [TokenType.Identifier,TokenType.SETTERS];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens.Length < 3)
            {
                errorMessage = "Setter syntax must follow: <identifier> <operator> <expression>";
                return true;
            }

            if (tokens[0].TokenType != TokenType.Identifier)
            {
                errorMessage = "Expected identifier as left-hand side of setter.";
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
            IGrammarInterpreter grammarInterpreter,
            int line)
        {
            string identifier = tokens[0].Value?.ToString();

            SetterOperator op = tokens[1].TokenOperation switch
            {
                TokenOperation.SETTER => SetterOperator.SETTER,
                TokenOperation.SETTERAddOperation => SetterOperator.SETTERAddOperation,
                TokenOperation.SETTERSubtractOperation => SetterOperator.SETTERSubtractOperation,
                _ => throw new Exception($"unexpect setter operator '{tokens[1].TokenOperation}'.")
            };

            var expression = tokens[2..]; // all tokens after operator
            if (tokens.Length > 3 && !expressionGrammarAnalyser.IsValidExpression(tokens[3..(tokens.Length)], out string errorMessage))
            {
                throw new Exception(errorMessage);
            }
            // Register as assigned in scope
            scopeObjectValueManager.MarkAssigned(identifier);

            var setterObj = new SetterSyntaxObject(identifier, op, expression);

            return new Token(TokenType.SETTERS, tokens[1].TokenOperation, TokenTree.Single, setterObj);
        }

        public string ViewSelf(Token Token, GrammarValidator grammarValidator = null, int indentLevel = 0)
        {
            if (Token.Value is not SetterSyntaxObject setter)
                return string.Empty;

            var indent = string.Join("", Enumerable.Repeat("    ", indentLevel));
            var builder = new StringBuilder();

            builder.AppendLine($"{indent}└── [SetterStatement]");
            builder.AppendLine($"{indent}    ├── [Identifier -> {setter.Identifier}]");
            builder.AppendLine($"{indent}    ├── [SetterOperator -> {setter.SetterOperator}]");

            builder.AppendLine($"{indent}    └── [Expression]");
            foreach (var token in setter.Expression)
            {
                builder.AppendLine($"{indent}        └── [Token::{token.TokenType}<{token.TokenOperation}> -> {token.Value}]");
            }

            return builder.ToString();
        }

    }

    public enum SetterOperator
    {
        SETTER,
        SETTERAddOperation,
        SETTERSubtractOperation
    }

    public record SetterSyntaxObject(string Identifier, SetterOperator SetterOperator, IEnumerable<Token> Expression);
}
