using MiniLang.Attributes.GrammarAttribute;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.StructCreation;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers.StructDeclaration
{
    [TriggerTokenType(TriggerType.Type)]
    public class FieldDeclarationGrammer : IGrammarAnalyser
    {
        public string GrammarName => "field-declartion";

        public TokenOperation[] TriggerTokensOperator => [];

        public TokenType[] TriggerTokenTypes => [TokenType.FieldAccess];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;
            if (tokens.Length is > 4)
            {
                errorMessage = "unexpected field declaration. expected '<private|public> <name> : <type>'.";
                return true;
            }
            if (tokens[0].TokenType is not TokenType.FieldAccess)
            {
                errorMessage = $"unexpected field access type. expected '<private|public>', but got '{tokens[1].Value}' instead.";
                return true;
            }

            if (tokens[1].TokenType is not TokenType.Identifier)
            {
                errorMessage = $"unexpected field access type. expected an identifier, but got '{tokens[2].Value}' of type {tokens[2].TokenType} instead.";
                return true;
            }
            if (tokens[2].TokenType is not TokenType.Director)
            {
                errorMessage = $"unexpected field access. expected '->', but got {tokens[2].Value} instead.";
                return true;
            }                                                             //for other struct linking
            if (!(tokens[3].TokenType is TokenType.ReturnType or TokenType.Identifier))
            {
                errorMessage = $"unexpected field access. expected a return type, but got {tokens[2].Value} instead.";
                return true;
            }
            return false;
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser, FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int Line)
        {
            if (tokens[3].TokenType is TokenType.Identifier)
            {
                if (!expressionGrammarAnalyser.IsValidExpression(tokens[3..], out string message))
                    throw new Exception(message);
            }
            return new Token(TokenType.None, TokenOperation.None, TokenTree.Single, new FieldItem(
                 fieldName: tokens[1].Value.ToString() ?? throw new Exception($"field missing name at line {Line}. "),
                    fieldType: ReturnTypeToNormalType(tokens[3].TokenOperation,
                    () => tokens[3].TokenType is not TokenType.Identifier ? throw new Exception($"Invalid field type was given '{tokens[3].TokenOperation}'.") :
                    tokens[3].TokenType)
                    , value: default(object?)!, IsStruct: tokens[3].TokenType is  TokenType.Identifier,
                    tokens[3].Value.ToString()!
                ));
        }

        private TokenType ReturnTypeToNormalType(TokenOperation operation, Func<TokenType> FallBack) =>
            operation switch
            {
                TokenOperation.None => TokenType.None,
                TokenOperation.ReturnsString => TokenType.StringLiteralExpression,
                TokenOperation.ReturnsNumber => TokenType.Number,
                TokenOperation.ReturnsNothing => throw new InvalidDataException("Invalid data type for field item, you can't use nothing as a type, use Object instead."),
                TokenOperation.ReturnsObject => TokenType.Object,
                _ => FallBack(),
            };
    }
}
