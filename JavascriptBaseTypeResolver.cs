using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavascriptEmulator
{
    public class JavascriptBaseTypeResolver
    {
        private JavascriptContext? Context;
        private JavascriptContext GlobalContext;

        public JavascriptBaseTypeResolver()
        {
            GlobalContext = new JavascriptContext { IsGlobal = true };
        }

        public List<JavascriptAst> GetConstants()
        {
            return GlobalContext.Constants;
        }

        public JavascriptContext GetGlobalContext()
        {
            return GlobalContext;
        }

        private JavascriptContext PushContext()
        {
            if (Context == null)
            {
                Context = GlobalContext;
                return Context;
            }
            else
            {
                var current = Context;
                Context = new JavascriptContext { IsGlobal = false, ParentContext = current };
                return Context;
            }
        }

        private JavascriptContext PopContext()
        {
            var current = Context;
            Context = current?.ParentContext ?? GlobalContext;
            return current;
        }

        public void Resolve(JavascriptAst ast)
        {
            ast.Context = Context;

            switch (ast.NodeType)
            {
                case JavascriptAstType.Block:
                    ResolveBlock((JavascriptAstBlock)ast);
                    break;
                case JavascriptAstType.Var:
                    ResolveVariable((JavascriptAstVar)ast);
                    break;
                case JavascriptAstType.BinOp:
                    ResolveBinOp((JavascriptAstBinaryOperation)ast);
                    break;
                case JavascriptAstType.Ident:
                    ResolveIdent((JavascriptAstIdent)ast);
                    break;
                case JavascriptAstType.Number:
                    ResolverNumber((JavascriptAstNumber)ast);
                    break;
            }
        }     

        public void ResolveBlock(JavascriptAstBlock ast)
        {
            var context = PushContext();
            context.Owner = ast;
            ast.Context = context;

            foreach (var element in ast.Elements)
            {
                Resolve(element);
            }

            PopContext();
        }

        public void ResolveVariable(JavascriptAstVar ast)
        {
            Context.StoreVariable(ast);
            Resolve(ast.Value);
        }

        public void ResolveBinOp(JavascriptAstBinaryOperation ast)
        {
            Resolve(ast.Left);
            Resolve(ast.Right);
        }

        public void ResolveIdent(JavascriptAstIdent ast)
        {
            //TODO: musí se zjistit kde se proměnná nachází uložit Context ze kterého se to bere

            var index = ast.Context.VariableIndexes[ast.Name];
            ast.Index = index;
            ast.Flag |= JavascriptAstIdentFlag.StoredInGlobalContext;
        }

        public void ResolverNumber(JavascriptAstNumber ast)
        {
            if (ast.Type == JavascriptAstNumberType.Integer)
            {

            }
            else
            {                
                GlobalContext.StoreConstant(ast);
            }
        }
    }
}
