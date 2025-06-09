using MiniLang.Runtime.Expression;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interpreter.Expression
{
    public class InterpolatedStringEvaluator
    {
        private readonly RuntimeExpressionEvaluator _evaluator;

        public InterpolatedStringEvaluator(RuntimeExpressionEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public string Evaluate(Token token, RuntimeContext context)
        {
            if (token.TokenType != TokenType.StringLiteralExpression || token.Value is not string raw)
                throw new InvalidOperationException("Invalid string literal for interpolation.");

            var result = new StringBuilder();
            for (int i = 0; i < raw.Length;)
            {
                if (raw[i] == '$' && i + 1 < raw.Length && raw[i + 1] == '(')
                {
                    i += 2;
                    int depth = 1;
                    var exprBuilder = new StringBuilder();

                    while (i < raw.Length && depth > 0)
                    {
                        if (raw[i] == '(') depth++;
                        else if (raw[i] == ')') depth--;

                        if (depth > 0)
                            exprBuilder.Append(raw[i]);

                        i++;
                    }

                    var expr = exprBuilder.ToString();

                    // Tokenize and parse expression
                    var tokens = MiniLang.Tokenilzer.Tokenizer.Tokenize(expr);
                    var parsedTokens = MiniLang.Parser.Parser.Parse(tokens); 

                    var value = _evaluator.Evaluate(parsedTokens);
                    result.Append(value.Value?.ToString());
                }
                else
                {
                    result.Append(raw[i]);
                    i++;
                }
            }

            return result.ToString();
        }
    }
}
