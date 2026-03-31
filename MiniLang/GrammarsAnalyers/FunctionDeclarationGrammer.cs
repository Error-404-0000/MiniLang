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
using System.Linq;
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

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens == null || tokens.Length != 4)
            {
                errorMessage = "Syntax error: function declaration must follow the form 'fn <return type> <Function> <Body>'.";
                return true;
            }
            if (tokens[1].TokenType is not (TokenType.ReturnType or TokenType.Identifier))
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
            int line, Action<Token> PushToken)
        {
            if (tokens[2].Value is FunctionCallTokenObject funcToken)
            {
                // Check that all function arguments are identifiers
                var invalidArg = funcToken.FunctionArgments
                    .FirstOrDefault(arg => !TryGetArgument(arg, out _, out _));

                if (invalidArg != null)
                {
                    throw new InvalidOperationException("Syntax error: function signature requires identifiers as argument names, but an expression was found.");
                }
                FunctionDeclarationScopeManager  FunctionBodyScope = new FunctionDeclarationScopeManager();//creating a new scope
                FunctionBodyScope.ParentScope = FunctionDeclarationManager;
                var declaredTypeName = tokens[1].TokenType == TokenType.Identifier ? tokens[1].Value?.ToString() : null;
                var declaredReturnOperation = tokens[1].TokenOperation;
                var func = new FunctionDeclarationSyntaxObject(
                        funcToken.FunctionName,
                        funcToken.FunctionArgmentsCount,
                        declaredReturnOperation,
                        funcToken.FunctionArgments,
                       null,
                       declaredTypeName
                );
                if(FunctionDeclarationManager.Exists(func.FunctionName, funcToken.FunctionArgmentsCount))
                {
                    throw new InvalidOperationException($"Syntax error: function signature was already declared. {func.FunctionName}");

                }
                if (declaredTypeName is not null && !scopeObjectValueManager.Exists(declaredTypeName))
                {
                    throw new InvalidOperationException($"Syntax error: return type '{declaredTypeName}' is not declared.");
                }
                if (declaredTypeName is not null)
                {
                    declaredReturnOperation = scopeObjectValueManager.GetTypeOf(declaredTypeName) switch
                    {
                        TokenType.Enum => TokenOperation.Enum,
                        TokenType.Struct => TokenOperation.ReturnsObject,
                        _ => throw new InvalidOperationException($"Syntax error: return type '{declaredTypeName}' is not supported.")
                    };
                }
                ScopeObjectValueManager SubScope = new ScopeObjectValueManager();
                SubScope.Parent = scopeObjectValueManager;
                //setting up the args Name(args..<-these)
                foreach (var arg in func.FunctionArgments)
                {
                    if (!TryGetArgument(arg, out var argumentName, out var declaredArgumentType))
                    {
                        throw new InvalidOperationException("Syntax error: function arg was unparseable.");
                    }

                    SubScope.Add(new GrammarInterpreter.GrammerdummyScopes.ScopeObjectValue()
                    {
                        Identifier = argumentName,
                        IsAssigned = true,
                        TokenType = declaredArgumentType ?? TokenType.Identifier,
                    });
                }
                FunctionDeclarationManager.Add(func);
                scopeObjectValueManager = SubScope;
                var exp = new ExpressionGrammarAnalyser(SubScope, FunctionBodyScope);
                var Body = grammarInterpreter.Interpret((tokens[3].Value as IEnumerable<Token>).ToList(), SubScope, FunctionBodyScope, exp);
                FunctionDeclarationManager.Remove(func);
               
                FunctionDeclarationManager.Add(func = new FunctionDeclarationSyntaxObject(
                        funcToken.FunctionName,
                        funcToken.FunctionArgmentsCount,
                        declaredReturnOperation,
                        funcToken.FunctionArgments,
                        Body,
                        declaredTypeName
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

        private static bool TryGetArgument(FunctionArgments argument, out string argumentName, out TokenType? declaredType)
        {
            argumentName = string.Empty;
            declaredType = null;
            var parts = argument.Argment.ToArray();
            if (parts.Length is < 1 or > 2)
            {
                return false;
            }

            if (parts.Length == 1)
            {
                if (parts[0].TokenType != TokenType.Identifier)
                {
                    return false;
                }

                argumentName = parts[0].Value?.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(argumentName);
            }

            if (parts[1].TokenType != TokenType.Identifier)
            {
                return false;
            }

            argumentName = parts[1].Value?.ToString() ?? string.Empty;
            declaredType = ResolveDeclaredType(parts[0]);
            return !string.IsNullOrWhiteSpace(argumentName);
        }

        private static TokenType? ResolveDeclaredType(Token token) =>
            token.TokenType switch
            {
                TokenType.ReturnType => token.TokenOperation switch
                {
                    TokenOperation.ReturnsNumber => TokenType.Number,
                    TokenOperation.ReturnsString => TokenType.StringLiteralExpression,
                    TokenOperation.ReturnsObject => TokenType.Object,
                    TokenOperation.ReturnsArray => TokenType.Array,
                    TokenOperation.ReturnsNothing => null,
                    _ => null
                },
                TokenType.Identifier => TokenType.Identifier,
                _ => null
            };
    }
}
