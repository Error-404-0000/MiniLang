using MiniLang.Attributes.GrammarAttribute;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{

    /// <summary>
    /// Represents the grammar for analyzing and interpreting "while" loop constructs in a custom language.
    /// </summary>
    /// <remarks>This class provides functionality to analyze tokens representing a "while" loop and validate
    /// their structure. It ensures that the loop contains a valid condition and an optional body. Additionally, it 
    /// 
    ///            while(<!-- condition -->) { <!-- body --> }-->
    ///            while(<!-- condition -->);
    ///            while(<!-- condition -->) : <!--body--> done
    /// 
    /// supports building the corresponding syntax tree node for the loop.</remarks>
    [RequiresBody]
    public class WhileGrammar : IGrammarAnalyser
    {
        public string GrammarName => "While loop";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.While];

        public TokenType[] TriggerTokenTypes => [];

        public bool RequiresTermination => false;

        public int CacheCode { get ; set; }

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;
            if (tokens.Length < 2)
            {
                errorMessage = "While loop requires at least a condition and a body(optional).";
                return true;
            }
            if(tokens.Length==2 && tokens[1].TokenTree != TokenTree.Group)
            {
                errorMessage = "While loop requires an expression scope.";
                return true;
            }
            if (tokens.Length == 2)
            {
                return false;
            }
            if(tokens.Length==3 && tokens[2].TokenType != TokenType.Scope && tokens[2].TokenType != TokenType.Scope)
            {
                errorMessage = "While loop requires a body scope or a semicolon.";
                return true;
            }
            if (tokens.Length > 3)
            {
                errorMessage = "While loop can only have a condition and an optional body.";
                return true;
            }
            return false;
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser, FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int Line)
        {
            if(tokens.Length is < 2 or > 3)
            {
                throw new ArgumentException("While loop must have a condition and an optional body.");
            }

            var condition = tokens[1].Value as IEnumerable<Token>;
            var body = tokens[2].Value as IEnumerable<Token>;
            expressionGrammarAnalyser.UpdateScope(scopeObjectValueManager);
            if (!expressionGrammarAnalyser.IsValidExpression(condition?.ToArray() ?? [],out string error))
            {
                throw new Exception($"Invalid condition in while loop: {error}");
            }
            var Body = grammarInterpreter.Interpret(body?.ToList() ??[], scopeObjectValueManager, FunctionDeclarationManager, expressionGrammarAnalyser);
            return new Token(TokenType.Conditions, TokenOperation.While, TokenTree.Single, new WhileSyntaxObject(condition ?? [], Body ?? [], body is not null));
        }
    }

    public record WhileSyntaxObject(IEnumerable<Token> Expression,IEnumerable<Token> Scope,bool hasBody);
}
