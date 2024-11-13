using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JavascriptEmulator.JavascriptLexer;

namespace JavascriptEmulator
{
    public class JavascriptEmulator
    {
        private JavascriptLexer lexer {  get; set; }
        private JavascriptBaseParser parser { get; set; }
        public IncludeFileDelegate OnFileInclude { get; set; }
        public JavascriptErrorOccuredDelegate OnError { get; set; }

        public delegate void JavascriptErrorOccuredDelegate(string error, ContentLocation location);
        public delegate IncludeFile IncludeFileDelegate(string fileName);

        public JavascriptEmulator()
        {            
        }

        public void Execute(string content, string fileName = "<anonymous>")
        {
            lexer = new JavascriptLexer(content, fileName);
            lexer.Lex();

            Console.WriteLine(content);
            Console.WriteLine();
            foreach(var token in lexer.Tokens)
            {
                Console.WriteLine($"{token.Location.FileName}:{token.Location.Row}:{token.Location.Column} - {token.BaseType}/{token.Type} - {token.Value}");
            }

            parser = new JavascriptBaseParser(lexer);
            var block = parser.ParseBlock();

            var x = 4;
        }

        public class IncludeFile
        {
            public string Name { get; set; }
            public string Content { get; set; }
            public Uri SourceUrl { get; set; }
        }
    }
}
