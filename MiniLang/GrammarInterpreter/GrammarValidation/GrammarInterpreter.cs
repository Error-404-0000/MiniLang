using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using MiniLang.SyntaxObjects;

namespace MiniLang.GrammarInterpreter
{

    public class GrammarInterpreter : IGrammarInterpreter
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
            FunctionDeclarationScopeManager FunctiondeclarationManager = new FunctionDeclarationScopeManager();
            ExpressionGrammarAnalyser expressionGrammarAnalyser = new(ref scopeObjectValueManager, FunctiondeclarationManager);
            return Interpret(Tokens, scopeObjectValueManager, FunctiondeclarationManager, expressionGrammarAnalyser);
        }

        public IEnumerable<Token> Interpret(List<Token> tokens, ScopeObjectValueManager scopeObjectValueManagerParent, FunctionDeclarationScopeManager FunctiondeclarationManager, ExpressionGrammarAnalyser expressionGrammarAnalyser)
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

                    var headerSegment = tokens.Skip(i).Take(scopeIndex + 1 - i).ToArray();
                    var result = Validator.Analyse(headerSegment, scopeObjectValueManagerParent, expressionGrammarAnalyser, FunctiondeclarationManager, this, i);
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
                        using (FunctionDeclarationScopeManager newFunctionScope = new FunctionDeclarationScopeManager())
                        {
                            newFunctionScope.ParentScope = FunctiondeclarationManager;

                        newScope.Parent = scopeObjectValueManagerParent;
                            
                        ExpressionGrammarAnalyser expressionGrammar = new(newScope, FunctiondeclarationManager);
                        var interpreted = Interpret(innerGroupScope, newScope, newFunctionScope, expressionGrammar);
                        results.Add(new Token(TokenType.Scope, TokenOperation.None, TokenTree.Group, interpreted.ToList()));
                        }
                            

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
                var analyseResult = Validator.Analyse(segment, scopeObjectValueManagerParent, expressionGrammarAnalyser, FunctiondeclarationManager, this, i);

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
        /// <summary>
        /// Processes a sequence of tokens, expanding tokens with a <see cref="TokenOperation.use"/> operation into
        /// their inner tokens while preserving other tokens unchanged.
        /// </summary>
        /// <remarks>This method iterates through the provided sequence of tokens and checks each token's
        /// operation.  If the operation is <see cref="TokenOperation.use"/> and the token's value is a <see
        /// cref="UseSyntaxObject"/>,  the inner tokens of the <see cref="UseSyntaxObject"/> are yielded. Otherwise, the
        /// original token is yielded.</remarks>
        /// <param name="tokens">The sequence of tokens to process. Cannot be null.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the processed tokens. Tokens with a  <see
        /// cref="TokenOperation.use"/> operation are replaced by their inner tokens, while other tokens  are returned
        /// as-is.</returns>
        public IEnumerable<Token> InjectUse(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                if (token.TokenOperation == TokenOperation.use  && token.Value is UseSyntaxObject use)
                {
                    foreach (var innerToken in use.Tokens)
                    {
                        yield return innerToken;
                    }
                }
                else
                {
                    yield return token;
                }
            }
        }
    }


}
