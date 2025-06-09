using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.SyntaxObjects.Condition;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{
    /// <summary>
    /// Represents a grammar analyser for conditional statements, such as "if" and "else".
    /// </summary>
    /// <remarks>This class provides functionality to analyse tokens representing conditional statements,
    /// validate their structure, and build syntax nodes for further interpretation. It supports "if" statements with
    /// optional "else" blocks and ensures proper token ordering and structure.</remarks>
    /// <example>
    /// 
    ///          if(<!--Expression-->) { <!--Body-->  else <!--Body-->}
    ///          if(<!--Expression-->) { <!--Body--> }
    ///           if(<!--Expression-->):   <!--Body-->  done
    ///            if(<!--Expression-->): <!--Body-->  else <!--Body--> done
    /// 
    /// 
    /// </example>
    ///


    [RequiresBody]
    public class ConditionGrammar : IGrammarAnalyser, IDebugger
    {
        public string GrammarName => "Condition Grammar";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.If, TokenOperation.@else];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public TokenType[] TriggerTokenTypes => throw new NotImplementedException();

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (tokens == null || tokens.Length == 0)
            {
                errorMessage = "No tokens provided.";
                return true;
            }

            if (tokens[0].TokenOperation == TokenOperation.@else)
            {
                errorMessage = "'else' cannot be the first token without a preceding 'if'.";
                return true;
            }

            if (tokens[0].TokenOperation != TokenOperation.If)
            {
                errorMessage = "Expected 'if' as the first token.";
                return true;
            }

            if (tokens.Length < 2 || tokens[1].TokenTree != TokenTree.Group)
            {
                errorMessage = "Expected a group token as the condition expression after 'if'.";
                return true;
            }

            if (tokens.Length > 2 && tokens[2].TokenType != TokenType.Scope)
            {
                errorMessage = "Expected a scope block as the body of the 'if' statement.";
                return true;
            }

            return false;
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser,
        FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int line)
        {
            if (tokens.Length < 2)
                throw new ArgumentException("Invalid token length for 'if' grammar.");

            if (tokens[1].TokenTree != TokenTree.Group)
                throw new ArgumentException("Second token must be a group representing the condition.");

            var expression = tokens[1].Value as IEnumerable<Token>;
            if (expression == null)
                throw new ArgumentException("Invalid expression group content.");

            IEnumerable<Token>? body = null;
            IEnumerable<Token>? elseBody = null;
            int elseStartIndex = -1;

            if (tokens.Length > 2 && tokens[2].TokenType == TokenType.Scope)
            {
                var scopeTokens = tokens[2].Value as IEnumerable<Token>;
                elseStartIndex = FindIndexOfElse(scopeTokens);

                if (elseStartIndex != -1)
                {
                    body = grammarInterpreter.Interpret(scopeTokens.Take(elseStartIndex).ToList(), scopeObjectValueManager, FunctionDeclarationManager, expressionGrammarAnalyser);
                    elseBody = grammarInterpreter.Interpret(scopeTokens.Skip(elseStartIndex+1).ToList(), scopeObjectValueManager, FunctionDeclarationManager, expressionGrammarAnalyser);
                }
                else
                {
                    body = grammarInterpreter.Interpret(scopeTokens.ToList(), scopeObjectValueManager, FunctionDeclarationManager, expressionGrammarAnalyser);
                }
            }

            var elseObj = elseBody != null ? new ElseSyntaxObject(elseStartIndex, elseBody) : null;
            return new Token(
                TokenType.Conditions,
                TokenOperation.If,
                TokenTree.Single,
                new ConditionSyntaxObject(expression, line, body, haveElse: elseBody != null, haveBody: body != null, elseObj)
            );
        }

        private static int FindIndexOfElse(IEnumerable<Token> tokens)
        {
            int depth = 0;
            int index = 0;
            foreach (var token in tokens)
            {
                if (token.TokenOperation == TokenOperation.If) depth++;
                else if (token.TokenOperation == TokenOperation.@else)
                {
                    if (depth == 0) return index;
                    depth--;
                }
                index++;
            }
            return -1;
        }

        public string ViewSelf(Token self,GrammarValidator validator, int indentLevel)
        {
            if (self.Value is not ConditionSyntaxObject condition)
                return string.Empty;

            var indent = new string(' ', indentLevel * 2);
            var childIndent = new string(' ', (indentLevel + 1) * 2);
            var sb = new StringBuilder();

            sb.AppendLine($"{indent}Condition");
            sb.AppendLine($"{childIndent}├─ Line: {condition.Line}");
            sb.AppendLine($"{childIndent}├─ HasBody: {condition.HasBody}");
            sb.AppendLine($"{childIndent}├─ HasElse: {condition.HasElse}");

            // Print Condition Expression
            sb.AppendLine($"{childIndent}├─ Condition Expression:");
            foreach (var token in condition.Expression)
            {
                if (validator.ResolveAnalyser(token) is IDebugger dbg)
                    sb.Append(dbg.ViewSelf(token, validator, indentLevel + 2));
                else
                    sb.AppendLine($"  {Debugger.Debugger.WriteTree([token], indent, true)}");
            }

            // Print Body
            if (condition.Scope != null)
            {
                sb.AppendLine($"{childIndent}├─ Body:");
                foreach (var token in condition.Scope)
                {
                    if (validator.ResolveAnalyser(token) is IDebugger dbg)
                        sb.Append(dbg.ViewSelf(token, validator, indentLevel + 2));
                    else
                        sb.AppendLine($"  {Debugger.Debugger.WriteTree([token], indent,true)}");
                }
            }

            // Print Else
            if (condition.HasElse && condition.Else is not null)
            {
                sb.AppendLine($"{childIndent}└─ Else:");
                foreach (var token in condition.Else.Scope)
                {
                    if (validator.ResolveAnalyser(token) is IDebugger dbg)
                        sb.Append(dbg.ViewSelf(token, validator, indentLevel + 2));
                    else
                        sb.AppendLine($"   {Debugger.Debugger.WriteTree([token], indent, true)}");
                }
            }

            return sb.ToString();
        }

    }
}
