using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using static JavascriptEmulator.JavascriptLexer;

namespace JavascriptEmulator
{
    public class JavascriptLexer
    {
        private string text { get; }
        private string fileName { get; }
        private int position { get; set; }
        private int column { get; set; }
        private int row { get; set; }
        public List<Token> Tokens { get; internal set; }
        private int index = 0;

        public JavascriptLexer(string text, string fileName)
        {
            this.text = text;
            this.fileName = fileName;
            Tokens = new List<Token>();
        }

        public IEnumerable<Token> Lex()
        {
            Tokens.Clear();

            Token[]? tokens;
            while ((tokens = GetNextToken()) != null && tokens.Length > 0)
            {
                foreach(var token in tokens)
                {
                    Tokens.Add(token);
                }                
            }

            Lex2();

            return Tokens;
        }

        private void Lex2()
        {
            var _tokens = new List<Token>();

            for (index = 0; index < Tokens.Count;)
            {                
                var token = GetNext();
                if (token == null) break;

                var newToken = new Token { BaseType = token.BaseType, Value = token.Value, Location = token.Location };
                var value = token.Value;

                if(token.BaseType == TokenBaseType.Symbol)
                {
                    if (value == "{")
                        newToken.Type = TokenType.lblock;
                    else if (value == "}")
                        newToken.Type = TokenType.rblock;
                    else if (value == "[")
                        newToken.Type = TokenType.lbracket;
                    else if (value == "]")
                        newToken.Type = TokenType.rbracket;
                    else if (value == "(")
                        newToken.Type = TokenType.lparenthese;
                    else if (value == ")")
                        newToken.Type = TokenType.rparenthese;
                    else if (value == ",")
                        token.Type = TokenType.comma;
                    else if (value == ";")
                        newToken.Type = TokenType.semicolon;
                    else if (value == ":")
                        newToken.Type = TokenType.colon;
                    else if (value == "<")
                    {
                        if (PeekNext()?.Value == "=")
                        {
                            newToken.Type = TokenType.lessequal;
                            index++;
                        }
                        else
                            newToken.Type = TokenType.less;
                    }
                    else if (value == ">")
                    {
                        if (PeekNext()?.Value == "=")
                        {
                            newToken.Type = TokenType.moreequal;
                            index++;
                        }
                        else
                            newToken.Type = TokenType.more;
                    }
                    else if (value == "+")
                    {
                        if (PeekNext()?.Value == "+")
                        {
                            newToken.Type = TokenType.increment;
                            index++;
                        }
                        else
                            newToken.Type = TokenType.plus;
                    }
                    else if (value == "-")
                    {
                        if (PeekNext()?.Value == "-")
                        {
                            newToken.Type = TokenType.decrement;
                            index++;
                        }
                        else
                            newToken.Type = TokenType.minus;
                    }
                    else if (value == "*")
                        newToken.Type = TokenType.mul;
                    else if (value == "/")
                        newToken.Type = TokenType.div;
                    else if (value == ".")
                        newToken.Type = TokenType.dot;
                    else if (value == "=")
                    {
                        if (PeekNext()?.Value == "=")
                        {
                            newToken.Type = TokenType.equal;
                            index++;
                        }
                        else
                            newToken.Type = TokenType.asign;
                    }
                    else if (value == "!")
                    {
                        if (PeekNext()?.Value == "=")
                        {
                            newToken.Type = TokenType.notequal;
                            index++;
                        }
                        else
                            newToken.Type = TokenType.exclamation;
                    }
                    else if (value == "?")
                        newToken.Type = TokenType.questionmark;
                    else if (value == "&")
                    {
                        if (PeekNext()?.Value == "&")
                        {
                            newToken.Type = TokenType.and;
                            index++;
                        }
                        else
                            newToken.Type = TokenType.band;
                    }
                    else if (value == "|")
                    {
                        if (PeekNext()?.Value == "|")
                        {
                            newToken.Type = TokenType.or;
                            index++;
                        }
                        else
                            newToken.Type = TokenType.bor;
                    }
                    else if (value == "$")
                        newToken.Type = TokenType.dolar;
                    else if (value == "%")
                        newToken.Type = TokenType.mod;
                    else
                        throw new Exception($"Unknown symbol '{value}' at {token.Location}");
                }
                else if(token.BaseType == TokenBaseType.Text)
                {
                    if (value == "for")
                        newToken.Type = TokenType.keyword_for;
                    else if (value == "if")
                        newToken.Type = TokenType.keyword_if;
                    else if (value == "else")
                        newToken.Type = TokenType.keyword_else;
                    else if (value == "while")
                        newToken.Type = TokenType.keyword_while;
                    else if (value == "true")
                        newToken.Type = TokenType.keyword_true;
                    else if (value == "false")
                        newToken.Type = TokenType.keyword_false;
                    else if (value == "new")
                        newToken.Type = TokenType.keyword_new;
                    else if (value == "var")
                        newToken.Type = TokenType.keyword_var;
                    else
                        newToken.Type = TokenType.ident;
                }
                else if(token.BaseType == TokenBaseType.Number)
                {
                    var _value = value;
                    var next = PeekNext();
                    if (next != null)
                    {
                        if (next.BaseType == TokenBaseType.Symbol && next.Value == ".")
                        {
                            GetNext();
                            next = GetNext();
                            if(next == null)
                            {
                                throw new Exception("Unexpected end");
                            }

                            _value += "." + next.Value;

                            newToken.Value = _value;
                            newToken.ValueFloat = float.Parse(_value, CultureInfo.InvariantCulture);
                            newToken.Type = TokenType.number;
                            newToken.Flags |= TokenFlags.NumberFloat;
                        }
                    }

                    if(newToken.Type == TokenType.unknown)
                    {
                        newToken.ValueInt = int.Parse(_value);
                        newToken.Type = TokenType.number;
                        newToken.Flags |= TokenFlags.NumberInterger;
                    }
                }

                _tokens.Add(newToken);
            }

            var prevTokensCount = Tokens.Count;
            var newToknesCount = _tokens.Count;

            Tokens = _tokens;
        }

        private Token? GetNext()
        {
            if (index < Tokens.Count) return Tokens[index++];
            return null;
        }

        private Token? PeekNext()
        {
            if (index < Tokens.Count) return Tokens[index];
            return null;
        }

        public Token[]? GetNextToken()
        {
            var buffer = new StringBuilder();
            var type = TokenBaseType.Unknown;
            ContentLocation? saved_location = null;

            while (position < text.Length)
            {               
                if(type == TokenBaseType.Unknown) MaybeSkipWhiteSpace();

                var location = MakeLocation();
                var ch = text[position];
                var ch_type = DetermineTokenType(ch);

                if (saved_location == null) saved_location = location;

                column++;
                position++;

                if (ch_type == TokenBaseType.Skip)
                {
                    continue;
                }
                
                if(ch_type == TokenBaseType.NewLine)
                {
                    column = 0;
                    row++;
                    continue;
                }

                if(ch_type == TokenBaseType.Symbol)
                {
                    var tokens = new List<Token>();

                    if (new char[] { '"', '\'', '`' }.Contains(ch))
                    {
                        var openCharacter = ch;
                        var open = 0;
                        tokens.Add(new Token { BaseType = TokenBaseType.Symbol, Value = ch.ToString(), Location = location });

                        location = MakeLocation();

                        while (position < text.Length)
                        {
                            var ch2 = text[position];

                            column++;
                            position++;

                            if (ch2 == openCharacter)
                            {
                                if(open == 0)
                                {
                                    tokens.Add(new Token { BaseType = TokenBaseType.Text, Value = buffer.ToString(), Location = location });

                                    location = MakeLocation();
                                    tokens.Add(new Token { BaseType = TokenBaseType.Symbol, Value = ch2.ToString(), Location = location });
                                    break;
                                }
                            }
                            if (ch2 == '\r') continue;
                            if (ch2 == '\n')
                            {
                                if(openCharacter == '`')
                                {
                                    buffer.Append("\n");
                                    column = 0;
                                    row++;
                                    continue;
                                }
                                else
                                {
                                    tokens.Add(new Token { BaseType = TokenBaseType.Text, Value = buffer.ToString(), Location = location });
                                    break;
                                }                                
                            }

                            buffer.Append(ch2);
                        }

                        return tokens.ToArray();
                    }
                    else if(type == TokenBaseType.Number)
                    {
                        tokens.Add(new Token { BaseType = type, Value = buffer.ToString(), Location = saved_location });
                        tokens.Add(new Token { BaseType = TokenBaseType.Symbol, Value = ch.ToString(), Location = location });

                        return tokens.ToArray();
                    }

                    if (type != TokenBaseType.Unknown)
                    {
                        tokens.Add(new Token { BaseType = type, Value = buffer.ToString(), Location = saved_location });
                    }
                    
                    tokens.Add(new Token { BaseType = TokenBaseType.Symbol, Value = ch.ToString(), Location = location });

                    return tokens.ToArray();
                }
               
                if (type == TokenBaseType.Unknown) type = ch_type;

                if (ch_type != type)
                {
                    MaybeSkipWhiteSpace();
                    return new []{ new Token { BaseType = type, Value = buffer.ToString(), Location = saved_location } };
                }

                buffer.Append(ch);
            }

            if(type != TokenBaseType.Unknown)
            {
                if (saved_location == null) throw new Exception("Missing saved location!");

                return new[] { new Token { BaseType = type, Value = buffer.ToString(), Location = saved_location } };
            }

            return null;
        }        

        private void MaybeSkipWhiteSpace()
        {
            var ch = text[position];
            var type = DetermineTokenType(ch);

            if (type != TokenBaseType.Whitespace) return;

            while (position < text.Length && DetermineTokenType(text[position]) == TokenBaseType.Whitespace)
            {
                column++;
                position++;
            }
        }

        private TokenBaseType DetermineTokenType(char token)
        {
            if (token == '\r') return TokenBaseType.Skip;
            if (token == '\n') return TokenBaseType.NewLine;
            if (char.IsDigit(token)) return TokenBaseType.Number;      // Pro jakékoliv číslo
            if (char.IsLetter(token)) return TokenBaseType.Text;       // Pro jakékoliv písmeno (včetně diakritiky a jiných abeced)
            if (char.IsWhiteSpace(token)) return TokenBaseType.Whitespace;  // Zvláštní zachycení bílých znaků
            if (char.IsSymbol(token) || char.IsPunctuation(token)) return TokenBaseType.Symbol;  // Symboly a interpunkce
            return TokenBaseType.Unknown;  // Pokud není nic z výše uvedeného
        }        

        public ContentLocation MakeLocation()
        {
            return new ContentLocation { FileName = fileName, Row = row + 1, Column = column + 1 };
        }                            
    }

    [Flags]
    public enum TokenFlags
    {
        None = 0,
        NumberInterger,
        NumberFloat,
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public TokenBaseType BaseType { get; set; }
        public TokenFlags Flags { get; set; } = TokenFlags.None;

        public string Value { get; set; }
        public int ValueInt { get; set; }
        public float ValueFloat { get; set; }

        public ContentLocation Location { get; set; }
    }

    public enum TokenBaseType
    {
        Unknown = 0,
        Text = 1,
        Number = 2,
        Symbol = 3,
        Skip = 97,
        NewLine = 98,
        Whitespace = 99
    }

    public enum TokenType
    {
        unknown = 0,

        keyword_iden,
        keyword_true,
        keyword_false,
        keyword_if,
        keyword_else,
        keyword_for,
        //keyword_string,
        keyword_new,
        //keyword_float,
        //keyword_char,
        //keyword_long,
        keyword_retur,
        keyword_enum,
        keyword_struct,
        //keyword_defer,
        keyword_constructor,
        keyword_descrutor,
        keyword_static,
        keyword_while,
        //keyword_bool,
        //keyword_int,
        keyword_typeof,
        keyword_sizeof,
        //keyword_union,
        keyword_var,

        plus = '+',
        minus = '-',
        mul = '*',
        div = '/',
        lparenthese = '(',
        rparenthese = ')',
        lbracket = '[',
        rbracket = ']',
        lblock = '{',
        rblock = '}',
        more = '>',
        less = '<',
        colon = ':',
        dot = '.',
        comma = ',',
        semicolon = ';',
        asign = '=',
        exclamation = '!',
        questionmark = '?',
        band = '&',
        bor = '|',
        dolar = '$',
        mod = '%',

        eof = -1,
        nop = -2,

        div_asing = 703, ///=
        mul_asing = 702, //*=
        increment = 100, //++
        increment_asign = 700, //+=
        decrement = 101, //--
        decrement_asign = 701, //-=
        returntype = 102, //->
        equal = 104, //==
        notequal = 105, //!=
        and = 106, //&&
        or = 107, //||
        range = 108, //..
        moreequal = 109, //>=
        lessequal = 110, //<=
        empty_index = 111, //[]

        number = 200,
        keyword = 300,
        ident = 400,
        @string = 500
    }
}
