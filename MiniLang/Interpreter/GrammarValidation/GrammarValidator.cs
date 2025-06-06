using CacheLily;
using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Interfaces;
using MiniLang.Interpreter.GrammarDummyScopes;
using MiniLang.Interpreter.GrammarValidation;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiniLang.Interpreter
{
    public class GrammarValidator
    {
        private readonly Cache<IGrammarAnalyser> _cache;
        private readonly IReadOnlyList<IGrammarAnalyser> _grammarAnalysers;

        public GrammarValidator(IEnumerable<IGrammarAnalyser> grammarAnalysers)
        {
            _grammarAnalysers = grammarAnalysers?.ToList()
                                ?? throw new ArgumentNullException(nameof(grammarAnalysers));

            _cache = new Cache<IGrammarAnalyser>(
                capacity: 100,
                200,
                false
            );
        }

        /// <summary>
        /// Analyzes the given tokens using the appropriate grammar handler.
        /// </summary>
        public GrammarAnalysisResult Analyse(Token[] tokens,ScopeObjectValueManager scopeStack, ExpressionGrammarAnalyser expressionGrammar, IGrammarInterpreter IGrammarInterpreter, int line)
        {
            if (tokens == null || tokens.Length == 0)
            {
                return GrammarAnalysisResult.Error("[Grammar Error] Empty or null token array.");
            }
            if (tokens[0].TokenOperation == TokenOperation.@else)
            {
                return GrammarAnalysisResult.Error("'else' cannot be used standalone — it must follow an if block.");
            }
            var firstToken = tokens[0];

            var analyser = _cache.Invoke(GetType(), ResolveAnalyser, firstToken);

            if (analyser == null)
            {
                return GrammarAnalysisResult.Error(
                    $"[Grammar Error] No grammar handler found for token: {firstToken.TokenOperation} (Type: {firstToken.TokenType}) on line {line}.");
            }

            if (analyser.Analyse(tokens, out string? message))
            {
                return GrammarAnalysisResult.Error(message ?? "[Grammar Error] Unknown grammar issue.");
            }

            var node = analyser.BuildNode(tokens, scopeStack,expressionGrammar, IGrammarInterpreter, line);
            return GrammarAnalysisResult.Success(node);
        }

        public IGrammarAnalyser? ResolveAnalyser(Token firstToken)
        {
            IGrammarAnalyser grammarAnalyser = null;
             _grammarAnalysers.ToList().ForEach(analyser =>
            {
                if(analyser.GetType().GetCustomAttribute<TriggerTokenType>(false) is TriggerTokenType trigger)
                {
                    if(trigger.TriggerType == TiggerType.Type)
                    {
                       if (analyser.TriggerTokenTypes.Contains(firstToken.TokenType))
                            grammarAnalyser = analyser;
                    }
                    else
                    {
                        if (analyser.TriggerTokensOperator.Contains(firstToken.TokenOperation))
                            grammarAnalyser = analyser;
                    }
                }
                else
                {
                    if (analyser.TriggerTokensOperator.Contains(firstToken.TokenOperation))
                        grammarAnalyser = analyser;
                }
            });
            return grammarAnalyser;
        }
    }
}
