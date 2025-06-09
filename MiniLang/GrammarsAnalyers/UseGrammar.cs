using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.SyntaxObjects;
using MiniLang.Tokenilzer;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MiniLang.GrammarsAnalyers
{
    /// <summary>
    /// Provides functionality for analyzing and processing the `use` keyword in a custom grammar.
    /// </summary>
    /// <remarks>The <see cref="UseGrammar"/> class is responsible for handling the `use` keyword, which is
    /// used to include external files. It validates the syntax, ensures the referenced file exists, and processes its
    /// contents to generate tokens for further interpretation.</remarks>
    /// <example>
    /// 
    ///        use "path/to/file.mini"; // This line will be processed by the UseGrammar analyser.
    /// </example>
    public class UseGrammar : IGrammarAnalyser,IDebugger
    {
        public string GrammarName => "use keyword";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.@use];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public TokenType[] TriggerTokenTypes => throw new NotImplementedException();

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens.Length != 2 || tokens[1].TokenType != TokenType.StringLiteralExpression)
            {
                errorMessage = "syntax error: expected `use \"filepath\";`";
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
            var pathToken = tokens[1];
            var path = pathToken.Value?.ToString();
            string resolvedPath = ResolvePathSmartly(path);

            if (!File.Exists(path))
                throw new FileNotFoundException($"use error: file not found → \"{path}\"");

            string fileSource = File.ReadAllText(resolvedPath);

            var tokensFromFile = Tokenizer.Tokenize(fileSource);
            var parsedTokens = Parser.Parser.Parse(tokensFromFile);

           var Tokens =  grammarInterpreter.Interpret(parsedTokens,scopeObjectValueManager, FunctionDeclarationManager,expressionGrammarAnalyser); // builds the token from the source file

            return new Token(TokenType.Keyword, TokenOperation.@use,TokenTree.Single, new UseSyntaxObject(path, Tokens));
        }

        public string ViewSelf(Token Token, GrammarValidator grammarValidator = null, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            if(Token.Value is UseSyntaxObject UseSyntaxObject)
            {
                foreach (Token token in UseSyntaxObject.Tokens)
                {
                    if (grammarValidator.ResolveAnalyser(token) is IGrammarAnalyser gm && gm is IDebugger db)
                    {
                        sb.AppendLine(db.ViewSelf(token, grammarValidator, 0));
                    }
                }
            }
            return sb.ToString();
        }
        private string ResolvePathSmartly(string path)
        {
            if (File.Exists(path))
                return Path.GetFullPath(path);

            string currentDirPath = Path.Combine(Environment.CurrentDirectory, path);
            if (File.Exists(currentDirPath))
                return Path.GetFullPath(currentDirPath);

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string assemblyDirPath = Path.Combine(baseDir, path);
            if (File.Exists(assemblyDirPath))
                return Path.GetFullPath(assemblyDirPath);

            string includesDirPath = Path.Combine(baseDir, "includes", path);
            if (File.Exists(includesDirPath))
                return Path.GetFullPath(includesDirPath);

            return path;
        }

    }
}

