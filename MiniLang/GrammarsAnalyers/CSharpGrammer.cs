using MiniLang.Attributes.GrammarAttribute;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.Interop;
using MiniLang.SyntaxObjects.Csharp;
using MiniLang.TokenObjects;

namespace MiniLang.GrammarsAnalyers
{
    [TriggerTokenType(TriggerType.Type)]
    public class CSharpGrammer : IGrammarAnalyser
    {
        public string GrammarName => "cscall";
        public TokenOperation[] TriggerTokensOperator => [TokenOperation.Cscall, TokenOperation.Win];
        public TokenType[] TriggerTokenTypes => [TokenType.CSharp];
        public bool RequiresTermination => true;
        public int CacheCode { get; set; }

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = "Invalid c# call, expected cscall/win <namespace> <function(args)>;";
            if (tokens.Length != 1)
            {
                return true;
            }

            if (tokens[0].Value is not CSharpCallSyntaxObject call)
            {
                errorMessage = "Invalid C# call payload.";
                return true;
            }

            if (!InteropBridgeRegistry.TryResolve(call.NameSpace, call.FunctionCall.FunctionName, out var bridgeMethod, out errorMessage))
            {
                return true;
            }

            if (bridgeMethod.ArgumentTypes.Count != call.FunctionCall.FunctionArgmentsCount)
            {
                errorMessage = $"Invalid interop call '{call.NameSpace}.{call.FunctionCall.FunctionName}': expected {bridgeMethod.ArgumentTypes.Count} arguments, got {call.FunctionCall.FunctionArgmentsCount}.";
                return true;
            }

            errorMessage = null;
            return false;
        }

        public Token BuildNode(
            Token[] tokens,
            ScopeObjectValueManager scopeObjectValueManager,
            ExpressionGrammarAnalyser expressionGrammarAnalyser,
            FunctionDeclarationScopeManager functionDeclarationManager,
            IGrammarInterpreter grammarInterpreter,
            int line,
            Action<Token> pushToken)
        {
            if (tokens[0].Value is not CSharpCallSyntaxObject call)
            {
                throw new InvalidOperationException("Invalid C# call syntax object.");
            }

            if (!InteropBridgeRegistry.TryResolve(call.NameSpace, call.FunctionCall.FunctionName, out var bridgeMethod, out var message))
            {
                throw new InvalidOperationException(message);
            }

            if (bridgeMethod.ArgumentTypes.Count != call.FunctionCall.FunctionArgmentsCount)
            {
                throw new InvalidOperationException($"Invalid interop call '{call.NameSpace}.{call.FunctionCall.FunctionName}': expected {bridgeMethod.ArgumentTypes.Count} arguments, got {call.FunctionCall.FunctionArgmentsCount}.");
            }

            return tokens[0];
        }
    }
}
