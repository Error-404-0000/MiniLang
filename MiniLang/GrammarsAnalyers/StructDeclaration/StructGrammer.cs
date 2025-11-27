using MiniLang.Attributes.GrammarAttribute;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.StructCreation;
using MiniLang.SyntaxObjects.Structure;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers.StructDeclaration
{
    [TriggerTokenType(TriggerType.Type)]
    [RequiresBody]
    public class StructGrammer : IGrammarAnalyser
    {
        public string GrammarName => "struct";

        public TokenOperation[] TriggerTokensOperator => [];

        public TokenType[] TriggerTokenTypes => [TokenType.Struct];

        public bool RequiresTermination => false;

        public int CacheCode { get; set; }

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;
            if (tokens == null || tokens.Length == 0)
            {
                errorMessage = "Error creating class. no token found";
                return true;
            }
            if (tokens.Length is < 3 or > 3)
            {
                errorMessage = "unexpected class creation. class creation needs class <className> {<body>}";
                return true;
            }
            if (tokens[1].TokenType is not TokenType.Identifier)
            {
                errorMessage = $"unexpected class name '{tokens[1].Value}', a {tokens[1].TokenType} was found instead.";
                return true;
            }
            if (tokens[2].TokenType != TokenType.Scope)
            {
                errorMessage = $"unexpected class body '{tokens[2].Value}', a {tokens[2].TokenType} was found instead..";
                return true;
            }
            return false;
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager,
            ExpressionGrammarAnalyser expressionGrammarAnalyser,
            FunctionDeclarationScopeManager FunctionDeclarationManager,
            IGrammarInterpreter grammarInterpreter, int Line)
        {
            var structName = tokens[1].Value.ToString()??throw new InvalidOperationException($"Struct name missing at line {Line}.");
            var scope = tokens[2].Value;

            if (scope is not IEnumerable<Token> scopeTokens)
            {
                throw new InvalidOperationException($"struct missing body at like {Line}.");
            }

            var tokenResult = grammarInterpreter.Interpret(scopeTokens.ToList(),
                              scopeObjectValueManager, FunctionDeclarationManager, expressionGrammarAnalyser);
           
            if (tokenResult.Any(x => x.Value is not FieldItem))
            {
                throw new InvalidOperationException($"struct can only have field, error at line {Line}.");
            }
            //creation for interpter check--dummy scope
            scopeObjectValueManager.Add(new GrammarInterpreter.GrammerdummyScopes.ScopeObjectValue() { IsAssigned = true, Identifier = structName, TokenType = TokenType.Struct });


            return new Token(TokenType.Struct, TokenOperation.None, TokenTree.Single, new StructSyntaxObject()
            {
                StructHandler = new StructFieldHandler(tokenResult.Select(x => (FieldItem)x.Value)),
                StructName = structName
            });
        }
    }
}
