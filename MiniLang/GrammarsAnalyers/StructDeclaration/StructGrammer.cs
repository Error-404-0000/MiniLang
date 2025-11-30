using MiniLang.Attributes.GrammarAttribute;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.Runtime.StackObjects.StackFunctionFrame;
using MiniLang.StructCreation;
using MiniLang.SyntaxObjects.FunctionBuilder;
using MiniLang.SyntaxObjects.Structure;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers.StructDeclaration
{
    [TriggerTokenType(TriggerType.Type)]
    [RequiresBody]
    public class StructGrammer : IGrammarAnalyser
    {
        public string GrammarName => "struct";

        public TokenOperation[] TriggerTokensOperator => [];

        public TokenType[] TriggerTokenTypes => [TokenType.Struct];

        public bool RequiresTermination => false;

        public int CacheCode { get; set; }

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;
            if (tokens == null || tokens.Length == 0)
            {
                errorMessage = "Error creating class. no token found";
                return true;
            }
            if (tokens.Length is < 3 or > 3)
            {
                errorMessage = "unexpected class creation. class creation needs class <className> {<body>}";
                return true;
            }
            if (tokens[1].TokenType is not TokenType.Identifier)
            {
                errorMessage = $"unexpected class name '{tokens[1].Value}', a {tokens[1].TokenType} was found instead.";
                return true;
            }
            if (tokens[2].TokenType != TokenType.Scope)
            {
                errorMessage = $"unexpected class body '{tokens[2].Value}', a {tokens[2].TokenType} was found instead..";
                return true;
            }
            return false;
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager,
            ExpressionGrammarAnalyser expressionGrammarAnalyser,
            FunctionDeclarationScopeManager FunctionDeclarationManager,
            IGrammarInterpreter grammarInterpreter, int Line)
        {
            var structName = tokens[1].Value.ToString() ?? throw new InvalidOperationException($"Struct name missing at line {Line}.");
            var scope = tokens[2].Value;

            if (scope is not IEnumerable<Token> scopeTokens)
            {
                throw new InvalidOperationException($"struct missing body at like {Line}.");
            }

            //creation for interpter check--dummy scope
            scopeObjectValueManager.Add(new GrammarInterpreter.GrammerdummyScopes.ScopeObjectValue() { IsAssigned = true, Identifier = structName, TokenType = TokenType.Struct });
            
            
            scopeObjectValueManager.IsInStruct();
            FunctionDeclarationManager.StructIn();


            var tokenResult = grammarInterpreter.Interpret(scopeTokens.ToList(),
                              scopeObjectValueManager, FunctionDeclarationManager, expressionGrammarAnalyser);

            if (tokenResult.Any(x => x.Value is not FieldItem && x.TokenType is not TokenType.NewFunction))
            {
                throw new InvalidOperationException($"struct can only have field and functions, error at line {Line}.");
            }

            scopeObjectValueManager.StructOut();


            var ToStringFunction = (FunctionDeclarationSyntaxObject?)tokenResult.FirstOrDefault(x => x.Value is FunctionDeclarationSyntaxObject func && func.FunctionName == structName + "ToString")?.Value;
            FunctionDeclarationManager.StructOut();//clears the struct methods
            if (ToStringFunction != null)
            {
                FunctionDeclarationManager.Add(ToStringFunction);
            }
            //FunctionDeclarationSyntaxObject ToStringStructMethods = new(null, 0, TokenOperation.ReturnsObject, [], []);
            //foreach(FieldItem field in from field in tokenResult where field.Value is FieldItem fielditem &&
            //                                   fielditem.IsStruct select field.Value as FieldItem)
            //{
            //    FunctionDeclarationManager.Add(ToStringStructMethods with { FunctionName = field.FieldName });
            //}
            
        
            return new Token(TokenType.Struct, TokenOperation.None, TokenTree.Single, new StructSyntaxObject()
            {

                StructHandler = new StructFieldHandler(tokenResult.Where(x => x.Value is FieldItem).Select(x => (FieldItem)x.Value))
                {
                    Functions = tokenResult.Where(x => x.Value is FunctionDeclarationSyntaxObject).
                    Select(x => new Runtime.StackObjects.StackFunctionFrame.RuntimeFunction(((FunctionDeclarationSyntaxObject)x.Value).FunctionName,
                                                                                             ((FunctionDeclarationSyntaxObject)x.Value).FunctionArgmentsCount,
                                                                                              (FunctionDeclarationSyntaxObject)x.Value)),
                    onFunctionOpen = fields => context => LoadStructFields(context,fields),
                    ToStringFunctionName = ToStringFunction?.FunctionName
                },
                StructName = structName,

            });
        }
        public static void LoadStructFields(RuntimeContext context, IEnumerable<FieldItem> fields)
        {
            foreach (FieldItem field in fields)
            {
                context.RuntimeScopeFrame.Declare(new RuntimeVariable(field.FieldName, field.FieldType,
                                               new RuntimeValue(TokenType.Object, TokenOperation.None, field.Value), field.IsStruct));
                if (field.IsStruct)
                {
                    var functionName = $"{field.FieldName}";
                    context.FunctionTable.Declare(new RuntimeFunction(functionName, 0,
                        new FunctionDeclarationSyntaxObject(functionName, 0, TokenOperation.ReturnsNothing, [], [])));
                }
            }
        }

    }
}
