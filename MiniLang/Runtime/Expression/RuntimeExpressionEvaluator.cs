using MiniLang.Functions;
using MiniLang.Runtime.Expression;
using MiniLang.Runtime.RuntimeExecutors.Builtins;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniLang.Runtime.Expression
{
    public class RuntimeExpressionEvaluator
    {
        private readonly RuntimeContext _context;
        private List<Token> _tokens;
        private int _pos;

        public RuntimeExpressionEvaluator(RuntimeContext context)
        {
            _context = context;
        }

        public RuntimeValue Evaluate(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
            return ParseExpression();
        }

        private RuntimeValue ParseExpression(int precedence = 0)
        {
            var left = ParsePrimary();

            while (_pos < _tokens.Count && IsOperator(_tokens[_pos], out int opPrecedence) && opPrecedence >= precedence)
            {
                var opToken = _tokens[_pos++];
                var right = ParseExpression(opPrecedence + 1);
                left = ApplyOperator(opToken, left, right);
            }

            return left;
        }

        private RuntimeValue ParsePrimary()
        {
            if (_pos >= _tokens.Count)
                throw new Exception("Unexpected end of expression.");

            var token = _tokens[_pos++];

            return token.TokenType switch
            {
                TokenType.Number => new RuntimeValue(TokenType.Number,TokenOperation.None, double.Parse(token.Value.ToString()!)),
                TokenType.StringLiteralExpression => new RuntimeValue(TokenType.StringLiteralExpression,TokenOperation.None, new InterpolatedStringEvaluator(_context.RuntimeExpressionEvaluator)
                .Evaluate(token, _context)),
                TokenType.Identifier => ResolveIdentifier(token),
                TokenType.FunctionCall => EvaluateFunctionCall(token),
                TokenType.Group => Evaluate(((List<Token>)token.Value!).ToList()),
                _ => throw new Exception($"Unexpected token in expression: {token.TokenType}")
            };
        }

        private RuntimeValue EvaluateFunctionCall(Token token)
        {
            if (token.Value is not FunctionTokenObject func)
                throw new Exception("Malformed function call token");

            var evaluatedArgs = func.FunctionArgments
                .Select(arg => Evaluate(arg.Argment.ToList()))
                .ToList();

            var functionToken = _context.RuntimeEngine.Execute([token]);
            return functionToken;
        }

        private RuntimeValue ResolveIdentifier(Token token)
        {
            string name = token.Value!.ToString()!;
            if (!_context.RuntimeScopeFrame.Exists(name))
                throw new Exception($"Undeclared identifier: {name}");

            return _context.RuntimeScopeFrame.Get(name);
        }

        private RuntimeValue ApplyOperator(Token op, RuntimeValue left, RuntimeValue right)
        {
            if(left.Type is TokenType.StringLiteralExpression  || right.Type is TokenType.StringLiteralExpression)
            {
                return new RuntimeValue(TokenType.StringLiteralExpression,TokenOperation.None,left.Value.ToString() + right.Value.ToString());
            }
            double l = Convert.ToDouble(left.Value);
            double r = Convert.ToDouble(right.Value);

            return op.TokenOperation switch
            {
                TokenOperation.AddOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, l + r),
                TokenOperation.SubtractOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, l - r),
                TokenOperation.MultiplyOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, l * r),
                TokenOperation.DivideOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, l / r),
                TokenOperation.ModuloOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, l % r),
                TokenOperation.PowerOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, Math.Pow(l, r)),

                TokenOperation.LessThanOrEqual => new RuntimeValue(TokenType.Number, TokenOperation.None, l <= r ? 1 : 0),
                TokenOperation.EqualOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, l == r ? 1 : 0),
                TokenOperation.GreaterThanOrEqual => new RuntimeValue(TokenType.Number, TokenOperation.None, l >= r ? 1 : 0),
                TokenOperation.GreaterThanOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, l > r ? 1 : 0),
                TokenOperation.LessThanOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, l < r ? 1 : 0),
                TokenOperation.Not => new RuntimeValue(TokenType.Number, TokenOperation.None, l != r ? 1 : 0),
                TokenOperation.AndOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, ((l != 0) & (r != 0)) ? 1 : 0),
                TokenOperation.OrOperation => new RuntimeValue(TokenType.Number, TokenOperation.None, ((l != 0) | (r != 0)) ? 1 : 0),

                _ => throw new Exception($"Invalid operator: {op.TokenOperation}")
            };

        }

        private bool IsOperator(Token token, out int precedence)
        {
            precedence = token.TokenOperation switch
            {
                TokenOperation.OrOperation => 1,
                TokenOperation.AndOperation => 2,

                TokenOperation.EqualOperation or TokenOperation.Not or
                TokenOperation.LessThanOperation or TokenOperation.GreaterThanOperation or
                TokenOperation.LessThanOrEqual or TokenOperation.GreaterThanOrEqual => 3,

                TokenOperation.AddOperation or TokenOperation.SubtractOperation => 4,
                TokenOperation.MultiplyOperation or TokenOperation.DivideOperation or TokenOperation.ModuloOperation => 5,
                TokenOperation.PowerOperation => 6,

                _ => -1
            };

            return precedence > 0;
        }

    }
}
