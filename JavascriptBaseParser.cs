using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JavascriptEmulator.JavascriptLexer;

namespace JavascriptEmulator
{
    public class JavascriptBaseParser
    {
        private List<Token> tokens;
        private int index;

        public JavascriptBaseParser(JavascriptLexer lexer)
        {
            index = 0;
            tokens = lexer.Tokens;
        }

        // Mapování operátorů na jejich prioritu
        private static readonly Dictionary<TokenType, int> operatorPrecedence = new Dictionary<TokenType, int>
        {
            { TokenType.mul, 3 }, // Násobení a dělení mají prioritu 3
            { TokenType.div, 3 },

            { TokenType.plus, 2 }, // Sčítání a odčítání mají prioritu 2
            { TokenType.minus, 2 },

            { TokenType.equal, 1 }, // Porovnávání má prioritu 1
            { TokenType.notequal, 1 }
        };

        public JavascriptAst Parse()
        {
            var element = Peek() ?? throw new Exception("No token!");

            if (element.Type == TokenType.keyword_var)
            {
                //var name = [expresion];

                var astVar = AstCreate<JavascriptAstVar>();

                Eat(TokenType.keyword_var);
                
                astVar.Name = Eat(TokenType.ident)!.Value;

                Eat(TokenType.asign);

                astVar.Value = ParseExpression();

                return astVar;
            }
            else if(element.Type == TokenType.number)
            {
                var astNumber = AstCreate<JavascriptAstNumber>();

                Eat(TokenType.number);                

                if((element.Flags & TokenFlags.NumberInterger) != 0)
                {
                    astNumber.Value = element.ValueInt;
                    astNumber.Type = JavascriptAstNumberType.Integer;
                }
                else if((element.Flags & TokenFlags.NumberFloat) != 0)
                {
                    astNumber.ValueFloat = element.ValueFloat;
                    astNumber.Type = JavascriptAstNumberType.Float;
                }
                else
                {
                    throw new JavascriptAstException("Unknown number type");
                }

                return astNumber;
            }
            else if (element.Type == TokenType.ident)
            {               
                var identName = Eat(TokenType.ident)!.Value;

                var next = Peek();
                if (next.Type == TokenType.asign)
                {
                    var astVar = AstCreate<JavascriptAstVar>();
                    astVar.Name = identName;

                    Eat(TokenType.asign);

                    astVar.Value = ParseExpression();

                    return astVar;
                }

                var ident = AstCreate<JavascriptAstIdent>();
                ident.Name = identName;
                return ident;
            }

            throw new JavascriptAstException($"Unexpected token type in basic parse! type = {element.Type}");
        }

        public JavascriptAst? ParseExpression(int precedenceLevel = 0)
        {
            var left = Parse();

            while (true)
            {
                var token = Peek();
                if (token == null || token.Type == TokenType.semicolon) break;

                if (!operatorPrecedence.TryGetValue(token.Type, out int tokenPrecedence) || tokenPrecedence < precedenceLevel)
                {
                    break;
                }

                var opToken = Eat(token.Type);
                var right = ParseExpression(tokenPrecedence + 1);

                left = AstCreate<JavascriptAstBinaryOperation>(left.Location, bin =>
                {
                    bin.Left = left;
                    bin.Operator = TokenToOperator(opToken.Type);
                    bin.Right = right;
                });
            }

            return left;
        }

        private JavascriptAstBinaryOperationType TokenToOperator(TokenType type)
        {
            switch (type)
            {
                case TokenType.plus:
                    return JavascriptAstBinaryOperationType.Plus;
                case TokenType.minus:
                    return JavascriptAstBinaryOperationType.Minus;
                case TokenType.mul:
                    return JavascriptAstBinaryOperationType.Mul;
                case TokenType.div:
                    return JavascriptAstBinaryOperationType.Divide;
                case TokenType.equal:
                    return JavascriptAstBinaryOperationType.Equal;
                case TokenType.notequal:
                    return JavascriptAstBinaryOperationType.NotEqual;
                default:
                    throw new JavascriptAstException($"Unknown token type '{type}'");
            }
        }

        public JavascriptAstBlock ParseBlock(bool eol = false)
        {
            var astBlock = AstCreate<JavascriptAstBlock>();
            var elements = new List<JavascriptAst>();

            Token? element;
            while ((element = Peek()) != null && (eol == false || element?.Type != TokenType.rparenthese))
            {
                if(element.Type == TokenType.semicolon)
                {
                    Eat(TokenType.semicolon);
                    continue;
                }

                var ast = Parse();

                elements.Add(ast);
            }

            astBlock.Elements = elements;
            return astBlock;
        }

        private Token? Peek()
        {
            if (index >= tokens.Count) return null;
            return tokens[index];
        }

        private Token? Eat(TokenType type)
        {
            var token = Peek();
            if (token == null || token.Type != type) throw new JavascriptAstException($"Unexpected token '{token?.Type}' expected '{type}'");

            index++;
            return token;
        }

        private T AstCreate<T>(ContentLocation? location = null, Action<T>? initializer = null) where T : JavascriptAst, new()
        {
            var obj = new T { Location = (location ?? Peek()?.Location) ?? throw new JavascriptAstException("Unknown location!") };
            initializer?.Invoke(obj);
            return obj;
        }
    }

    public class JavascriptAstException: Exception
    {
        public JavascriptAstException(string message): base(message) { }
    }

    public class JavascriptContext
    {
        public bool IsGlobal { get; set; }
        public int CurrentIndex { get; set; } = 0;
        public JavascriptAst? Owner { get; set; }
        public JavascriptContext? ParentContext { get; set; }
        public List<JavascriptAst> Constants { get; set; } = new List<JavascriptAst>();
        public Dictionary<int, JavascriptAst> Variables { get; set; } = new Dictionary<int, JavascriptAst>();
        public Dictionary<string, int> VariableIndexes { get; set; } = new Dictionary<string, int>();

        public int Index()
        {
            return CurrentIndex++;
        }

        public void StoreVariable(JavascriptAstVar ast)
        {
            ast.Context = this;

            if (VariableIndexes.ContainsKey(ast.Name))
            {
                ast.Index = VariableIndexes[ast.Name];
            }
            else
            {
                ast.Index = Index();                
                Variables.Add(ast.Index, ast);
                VariableIndexes.Add(ast.Name, ast.Index);
            }
        }

        public int StoreConstant(JavascriptAstNumber ast)
        {
            if(!IsGlobal)
            {
                return ParentContext.StoreConstant(ast);
            }
            
            var index = Constants.Count;
            Constants.Add(ast);
            ast.Flags |= JavascriptAstNumberFlags.StoredInConstantTable;
            ast.Index = index;
            return index;
        }
    }

    public abstract class JavascriptAst
    {
        public abstract JavascriptAstType NodeType { get; }
        public ContentLocation Location { get; set; }
        public JavascriptContext Context { get; set; }

    }

    public enum JavascriptAstType
    {
        None = 0,
        Block = 1,
        Var = 2,
        BinOp = 3,
        Number = 4,
        Ident = 5
    }

    public class JavascriptAstBlock: JavascriptAst
    {
        public override JavascriptAstType NodeType => JavascriptAstType.Block;

        public IEnumerable<JavascriptAst> Elements { get; set; }
    }

    public class JavascriptAstVar : JavascriptAst
    {
        public override JavascriptAstType NodeType => JavascriptAstType.Var;
        public string Name { get; set; }
        public JavascriptAst? Value { get; set; }
        public int Index { get; set; }
    }

    public class JavascriptAstBinaryOperation : JavascriptAst
    {
        public override JavascriptAstType NodeType => JavascriptAstType.BinOp;
        public JavascriptAst? Left { get; set; }
        public JavascriptAstBinaryOperationType Operator { get; set; }
        public JavascriptAst? Right { get; set; }
    }

    public enum JavascriptAstBinaryOperationType
    {
        Equal,      //==
        NotEqual,   //!=
        Plus,       //+
        Minus,      //-
        Mul,        //*
        Divide      ///
    }

    public class JavascriptAstIdent: JavascriptAst
    {
        public override JavascriptAstType NodeType => JavascriptAstType.Ident;
        public string Name { get; set; }
        public JavascriptAstIdentFlag Flag { get; set; }
        public int Index { get; set; }
    }

    [Flags]
    public enum JavascriptAstIdentFlag
    {
        None = 0,
        StoredInGlobalContext,
        StoredInLocalContext,
        StoredInIndexedContext
    }

    public class JavascriptAstNumber: JavascriptAst
    {
        public override JavascriptAstType NodeType => JavascriptAstType.Number;
        public int Value { get; set; }
        public float ValueFloat { get; set; }
        public JavascriptAstNumberType Type { get; set; }
        public JavascriptAstNumberFlags Flags { get; set; }
        public int Index { get; set; }
    }

    public enum JavascriptAstNumberType
    {
        Integer = 1,
        Float = 2,
    }

    [Flags]
    public enum JavascriptAstNumberFlags
    {
        None = 0,
        StoredInConstantTable
    }
}
