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
using System.IO;
using System.Linq;
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

        public bool Analyze(Token[] tokens, out string errorMessage)
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
            int line, Action<Token> PushToken)
        {
            var pathToken = tokens[1];
            var path = pathToken.Value?.ToString();
            string resolvedPath = ResolvePathSmartly(path);

            if (!File.Exists(resolvedPath))
                throw new FileNotFoundException($"use error: file not found → \"{path}\"");

            string fileSource = File.ReadAllText(resolvedPath);

            using var _ = UsePathContext.Push(resolvedPath);
            var tokensFromFile = Tokenizer.Tokenize(fileSource);
            var parsedTokens = Parser.Parser.Parse(tokensFromFile);

            var Tokens = grammarInterpreter.Interpret(parsedTokens, scopeObjectValueManager, FunctionDeclarationManager, expressionGrammarAnalyser); // builds the token from the source file

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
            if (string.IsNullOrWhiteSpace(path))
                return path;

            var normalizedPath = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            if (File.Exists(path))
                return Path.GetFullPath(path);

            foreach (var importerDirectory in UsePathContext.ImporterDirectories)
            {
                foreach (var candidate in EnumerateCandidatePaths(importerDirectory, normalizedPath, treatRootAsDirectory: true))
                {
                    if (File.Exists(candidate))
                        return Path.GetFullPath(candidate);
                }
            }

            foreach (var root in EnumerateCandidateRoots())
            {
                foreach (var candidate in EnumerateCandidatePaths(root, normalizedPath, treatRootAsDirectory: false))
                {
                    if (File.Exists(candidate))
                        return Path.GetFullPath(candidate);
                }
            }

            return path;
        }

        private static IEnumerable<string> EnumerateCandidateRoots()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var start in new[]
                     {
                         Environment.CurrentDirectory,
                         AppDomain.CurrentDomain.BaseDirectory
                     }.Where(static value => !string.IsNullOrWhiteSpace(value)))
            {
                var current = new DirectoryInfo(start);
                while (current is not null)
                {
                    if (seen.Add(current.FullName))
                        yield return current.FullName;

                    current = current.Parent;
                }
            }
        }

        private static IEnumerable<string> EnumerateCandidatePaths(string root, string relativePath, bool treatRootAsDirectory)
        {
            yield return Path.Combine(root, relativePath);

            if (!treatRootAsDirectory)
            {
                yield return Path.Combine(root, "includes", relativePath);
            }

            if (!relativePath.StartsWith("MiniLangLibraries" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(relativePath, "MiniLangLibraries", StringComparison.OrdinalIgnoreCase))
            {
                yield return Path.Combine(root, "MiniLangLibraries", relativePath);
            }

            if (!relativePath.StartsWith("MiniLangGuide" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(relativePath, "MiniLangGuide", StringComparison.OrdinalIgnoreCase))
            {
                yield return Path.Combine(root, "MiniLangGuide", relativePath);
                yield return Path.Combine(root, "MiniLangGuide", "MiniLang_Syntax_Guide", relativePath);
            }

            if (!relativePath.StartsWith("MiniLangProjects" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(relativePath, "MiniLangProjects", StringComparison.OrdinalIgnoreCase))
            {
                yield return Path.Combine(root, "MiniLangProjects", relativePath);
            }
        }

    }
}

