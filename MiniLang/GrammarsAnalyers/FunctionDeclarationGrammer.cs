using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.SyntaxObjects.FunctionBuilder;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{
    /// <summary>
    /// Represents the grammar analysis for function declarations in a minilang language.
    /// </summary>
    /// <remarks>This class provides functionality to analyze and validate the syntax of function
    /// declarations, ensuring they conform to the expected format. It also supports building syntax tree nodes for
    /// valid function declarations.</remarks>
    /// <example>
    ///     fn number add(number a, number b) {
    ///             give a + b;
    ///     }
    ///     fn string greet(string name) {
    ///         give "Hello, $(name)";
    ///     }
    ///     fn object getObject(obj) {
    ///         give typeof obj;<!-- optional return -->
    ///     }
    ///     fn nothing doNothing(){
    ///         @must return nothing;
    ///     }
    /// 
    /// 
    /// </example>
    ///

    [TriggerTokenType(TriggerType.Type), RequiresBody]
    public class FunctionDeclarationGrammar : IGrammarAnalyser
    {
        public string GrammarName => "Function Declaration";

        public TokenOperation[] TriggerTokensOperator => Array.Empty<TokenOperation>();

        public TokenType[] TriggerTokenTypes => new[] { TokenType.NewFunction };

        public bool RequiresTermination => false;

        public int CacheCode { get; set; }

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens == null || tokens.Length != 4)
            {
                errorMessage = "Syntax error: function declaration must follow the form 'fn <return type> <Function> <Body>'.";
                return true;
            }
            if (tokens[1].TokenType is not TokenType.ReturnType)
            {
                errorMessage = "Syntax error: 'fn' requires a return type.";
                return true;
            }
            if (tokens[2].TokenType != TokenType.FunctionCall)
            {
                errorMessage = "Syntax error: 'fn' must be followed by a function signature.";
                return true;
            }

            if (tokens[3].TokenType != TokenType.Scope)
            {
                errorMessage = "Syntax error: function declaration must include a body block.";
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
            int line)
        {
            if (tokens[2].Value is FunctionTokenObject funcToken)
            {
                // Check that all function arguments are identifiers
                var invalidArg = funcToken.FunctionArgments
                    .FirstOrDefault(arg =>
                        arg.Argment.FirstOrDefault(token => token.TokenType != TokenType.Identifier) != null);

                if (invalidArg != null)
                {
                    throw new InvalidOperationException("Syntax error: function signature requires identifiers as argument names, but an expression was found.");
                }
                FunctionDeclarationScopeManager  FunctionBodyScope = new FunctionDeclarationScopeManager();//creating a new scope
                FunctionBodyScope.ParentScope = FunctionDeclarationManager;
                var func = new FunctionDeclarationSyntaxObject(
                        funcToken.FunctionName,
                        funcToken.FunctionArgmentsCount,
                        tokens[1].TokenOperation,
                        funcToken.FunctionArgments,
                       null
                );
                if(FunctionDeclarationManager.Exists(func.FunctionName, funcToken.FunctionArgmentsCount))
                {
                    throw new InvalidOperationException($"Syntax error: function signature was already declared. {func.FunctionName}");

                }
                ScopeObjectValueManager SubScope = new ScopeObjectValueManager();
                SubScope.Parent = scopeObjectValueManager;
                foreach (var arg in func.FunctionArgments)
                {
                    SubScope.Add(new GrammarInterpreter.GrammerdummyScopes.ScopeObjectValue()
                    {
                        Identifier = arg.Argment.ToArray()[0].Value.ToString()??throw new Exception("Syntax error: function arg was unparseable."),
                        IsAssigned = true,
                        TokenType = TokenType.Identifier,
                    });
                }
                FunctionDeclarationManager.Add(func);
                var Body = grammarInterpreter.Interpret((tokens[3].Value as IEnumerable<Token>).ToList(), SubScope, FunctionBodyScope, expressionGrammarAnalyser);
                FunctionDeclarationManager.Remove(func);
               
                FunctionDeclarationManager.Add(func = new FunctionDeclarationSyntaxObject(
                        funcToken.FunctionName,
                        funcToken.FunctionArgmentsCount,
                        tokens[1].TokenOperation,
                        funcToken.FunctionArgments,
                       Body
                ));

                return new Token(
                    TokenType.NewFunction,
                    TokenOperation.None,
                    TokenTree.Single,
                    func
                );
            }

            throw new InvalidOperationException($"Invalid function token object at line {line}. Expected FunctionTokenObject but got {tokens[1].Value?.GetType().Name ?? "null"}.");
        }
    }
}
