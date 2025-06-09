using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects.Make;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interpreter.RuntimeExecutors.Builtins
{
    public class MakeExecutable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Keyword ];
        public TokenOperation[] InvokeOperation => [ TokenOperation.make];

        public RuntimeValue? Dispatch(Token token, RuntimeContext context)
        {
            if (token.Value is not MakeSyntaxObject makeSyntax)
                throw new InvalidOperationException("Invalid 'make' token payload.");

            List<Token> expressionTokens = makeSyntax.AssignedValue switch
            {
                IEnumerable<Token> list => list.ToList(),
                Token single => new List<Token> { single },
                _ => throw new InvalidOperationException("Assigned value must be a Token or List<Token>")
            };

            var evaluated = context.RuntimeExpressionEvaluator.Evaluate(expressionTokens);

            // Infer the declared type based on value
            TokenType inferredType = evaluated.Type;

            var variable = new RuntimeVariable(makeSyntax.Identifier, inferredType, evaluated);
            context.RuntimeScopeFrame.Declare(variable);

            return null; // 'make' returns nothing
        }
    }
}
