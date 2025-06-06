using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Interfaces;
using MiniLang.Interpreter.GrammarDummyScopes;
using MiniLang.Interpreter.GrammarValidation;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniLang.Interpreter
{

    public class GrammarInterpreter: IGrammarInterpreter
    {
        private GrammarValidator Validator { get; }
        public List<Token> Tokens { get; }

        public GrammarInterpreter(GrammarValidator validator, IEnumerable<Token> tokens)
        {
            Validator = validator;
            Tokens = tokens.ToList();
        }
        public IEnumerable<Token> Interpret()
        {
            ScopeObjectValueManager scopeObjectValueManager = new ScopeObjectValueManager();
            ExpressionGrammarAnalyser expressionGrammarAnalyser = new(scopeObjectValueManager);
            return Interpret(Tokens, scopeObjectValueManager, expressionGrammarAnalyser);
        }

        public IEnumerable<Token> Interpret(List<Token> tokens, ScopeObjectValueManager scopeObjectValueManagerParent, ExpressionGrammarAnalyser expressionGrammarAnalyser)
        {
            tokens = TokenBuilder.BuildStructuredTokens(tokens);
            List<Token> results = new();
            int i = 0;

            while (i < tokens.Count)
            {
                var currentToken = tokens[i];
                var grammarAnalyser = Validator.ResolveAnalyser(currentToken);

                // If a grammar analyser requires a body , look for the next scope
                if (grammarAnalyser != null && grammarAnalyser.GetType().GetCustomAttributes(typeof(RequiresBody), true).FirstOrDefault() is not null)
                {
                    int scopeIndex = -1;
                    for (int j = i + 1; j < tokens.Count; j++)
                    {
                        if (tokens[j].TokenTree == TokenTree.Group && tokens[j].TokenType == TokenType.Scope && tokens[j].Value is List<Token>)
                        {
                            scopeIndex = j;
                            break;
                        }
                    }

                    if (scopeIndex == -1)
                        throw new Exception($"Expected body scope for token '{currentToken.Value}' at index {i}");

                    var headerSegment = tokens.Skip(i).Take(scopeIndex+1 - i).ToArray();
                    var result = Validator.Analyse(headerSegment, scopeObjectValueManagerParent, expressionGrammarAnalyser,this, i);
                    if (result.HasError)
                        throw new Exception(result.ErrorMessage);

                    results.Add(result.Node!);

                    //using (ScopeObjectValueManager newScope = new ScopeObjectValueManager())
                    //{
                    //    newScope.Parent = scopeObjectValueManagerParent;
                    //    ExpressionGrammarAnalyser expressionGrammar = new(newScope);

                    //    var innerGroup = (List<Token>)tokens[scopeIndex].Value!;
                    //    var innerResult = Interpret(innerGroup, newScope, expressionGrammar);
                    //    results.Add(new Token(TokenType.Scope, TokenOperation.None, TokenTree.Group, innerResult.ToList()));
                    //}

                    i = scopeIndex + 1;
                    continue;
                }

                // recursively interpret inner groups
                if (currentToken.TokenTree == TokenTree.Group && currentToken.TokenType == TokenType.Scope && currentToken.Value is List<Token> innerGroupScope)
                {
                    using (ScopeObjectValueManager newScope = new ScopeObjectValueManager())
                    {
                        newScope.Parent = scopeObjectValueManagerParent;
                        ExpressionGrammarAnalyser expressionGrammar = new(newScope);
                        var interpreted = Interpret(innerGroupScope, newScope, expressionGrammar);
                        results.Add(new Token(TokenType.Scope, TokenOperation.None, TokenTree.Group, interpreted.ToList()));
                        i++;
                    }
                    continue;
                }

                int end = IndexOf(tokens, TokenType.Semicolon, i);

                if (end == -1)
                {
                    if (grammarAnalyser != null && grammarAnalyser.RequiresTermination != null)
                    {
                        throw new Exception($"'{currentToken.Value}' expects a semicolon at index {i}.");
                    }
                    else
                    {
                        end = tokens.Count;
                    }
                }

                var segment = tokens.Skip(i).Take(end - i).ToArray();
                var analyseResult = Validator.Analyse(segment, scopeObjectValueManagerParent, expressionGrammarAnalyser,this, i);

                if (analyseResult.HasError)
                    throw new Exception(analyseResult.ErrorMessage);

                results.Add(analyseResult.Node!);
                i = end + 1;
            }

            return results;
        }

        private int IndexOf(List<Token> tokens, TokenType type, int start)
        {
            for (int i = start; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == type)
                    return i;
            }
            return -1;
        }
    }


}
