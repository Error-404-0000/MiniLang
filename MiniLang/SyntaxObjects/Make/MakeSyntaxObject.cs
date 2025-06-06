using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.SyntaxObjects.Make
{
    public class MakeSyntaxObject
    {
        /// <summary>
        /// The name of the variable being declared.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// The value or expression being assigned.
        /// Could be a literal, another identifier, or a complex expression node.
        /// </summary>
        public object AssignedValue { get; }

        /// <summary>
        /// Optional: Source line number or debug info.
        /// </summary>
        public int? SourceLine { get; }

        public MakeSyntaxObject(string identifier, object assignedValue, int? sourceLine = null)
        {
            Identifier = identifier;
            AssignedValue = assignedValue;
            SourceLine = sourceLine;
        }
        public string ToTreeString(string indent = "", bool isLast = true)
        {
            var builder = new System.Text.StringBuilder();
            string currentPrefix = indent + (isLast ? "└── " : "├── ");
            string childIndent = indent + (isLast ? "    " : "│   ");

            builder.AppendLine($"{currentPrefix}[MakeStatement]");
            builder.AppendLine($"{childIndent}├── [Identifier -> {Identifier}]");

            if (AssignedValue is MakeSyntaxObject nestedMake)
            {
                builder.AppendLine($"{childIndent}└── [Assigned ->]");
                builder.Append(nestedMake.ToTreeString(childIndent + "    ", true));
            }
            else if (AssignedValue is IEnumerable<Token> tokenList)
            {
                var tokens = tokenList.ToList();
                for (int i = 0; i < tokens.Count; i++)
                {
                    var token = tokens[i];
                    bool isLastToken = i == tokens.Count - 1;

                    if (token.Value is IEnumerable<Token> innerTokens)
                    {
                        builder.AppendLine($"{childIndent}{(isLastToken ? "└──" : "├──")} [Group]");
                        foreach (var t in innerTokens)
                        {
                            builder.AppendLine($"{childIndent}    └── [Token::{t.TokenType}<{t.TokenOperation}> -> {t.Value}]");
                        }
                    }
                    else
                    {
                        builder.AppendLine($"{childIndent}{(isLastToken ? "└──" : "├──")} [Token::{token.TokenType}<{token.TokenOperation}> -> {token.Value}]");
                    }
                }
            }
            else
            {
                builder.AppendLine($"{childIndent}└── [AssignedValue -> {AssignedValue}]");
            }

            return builder.ToString();
        }

    }


}
