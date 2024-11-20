using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace JavascriptEmulator
{
    public class JavascriptBaseBytecodeRunner
    {
        private List<JavascriptMemory> GlobalContext { get; set; }
        private List<List<JavascriptMemory>> Context { get; set; }
        private List<JavascriptMemory> Stack { get; set; }
        private List<JavascriptMemory> Constants { get; set; }
        public long[] Bytecodes { get; }

        private int index = 0;
        public JavascriptBaseBytecodeRunner(long[] bytecodes)
        {
            GlobalContext = new List<JavascriptMemory>();
            Stack = new List<JavascriptMemory>();
            Bytecodes = bytecodes;
        }

        public JavascriptMemory[] GetGlobalContext()
        {
            return GlobalContext.ToArray();
        }

        public void BuildConstants(List<JavascriptAst> constants)
        {
            Constants = new List<JavascriptMemory>();
            foreach (var constant in constants)
            {
                if (constant.NodeType == JavascriptAstType.Number)
                {
                    var number = (JavascriptAstNumber)constant;                    
                    if ((number.Flags & JavascriptAstNumberFlags.StoredInConstantTable) != 0)
                    {
                        if (number.Type == JavascriptAstNumberType.Float)
                        {
                            Constants.Add(new JavascriptMemory { Type = JavascriptMemoryType.Float, ValueFloat = number.ValueFloat });
                        }
                    }
                }
            }
        }        

        private long? get()
        {
            if (index >= Bytecodes.Length) return null;
            return Bytecodes[index++];
        }

        public int getInt()
        {
            var l = get();
            if(l == null) throw new Exception("empty");
            return (int)l;
        }

        public void Run()
        {
            long? instruction;
            int index;
            float num1, num2;

            while ((instruction = get()) != null){
                switch (instruction)
                {
                    case (int)JavascriptByteCodes.LdaSmi:
                        PushStack(getInt());
                        break;
                    case (int)JavascriptByteCodes.LdaConstant:
                        index = getInt();
                        PushStack(Constants[index]);
                        break;
                    case (int)JavascriptByteCodes.LdaGlobal:
                        index = getInt();
                        PushStack(GetGlobalContext(index));
                        break;
                    case (int)JavascriptByteCodes.LdaContextSlot:
                        index = getInt();
                        PushStack(GetContext(index));
                        break;
                    case (int)JavascriptByteCodes.StaGlobal:
                        index = getInt();
                        PushGlobalContext(index, PopStack());
                        break;
                    case (int)JavascriptByteCodes.StaCurrentContextSlot:
                        index = getInt();
                        PushContext(index, PopStack());
                        break;
                    /*case (int)JavascriptByteCodes.PushContext:
                        
                        break;
                    case (int)JavascriptByteCodes.PopContext:
                        Stack.RemoveAt(Stack.Count - 1);
                        break;*/
                    case (int)JavascriptByteCodes.Mul:
                        num1 = PopStack().GetNumericValue();
                        num2 = PopStack().GetNumericValue();
                        PushStack(num1 * num2);
                        break;
                    case (int)JavascriptByteCodes.Add:
                        num1 = PopStack().GetNumericValue();
                        num2 = PopStack().GetNumericValue();
                        PushStack(num1 + num2);
                        break;
                    case (int)JavascriptByteCodes.Sub:
                        num1 = PopStack().GetNumericValue();
                        num2 = PopStack().GetNumericValue();
                        PushStack(num1 - num2);
                        break;
                    case (int)JavascriptByteCodes.Div:
                        num1 = PopStack().GetNumericValue();
                        num2 = PopStack().GetNumericValue();
                        PushStack(num1 / num2);
                        break;
                    default:
                        throw new Exception($"Unknown bytecode: {instruction}");
                }
            }
            
        }

        private void PushStack(int number)
        {
            Stack.Add(new JavascriptMemory { Type = JavascriptMemoryType.Number, ValueNumber = number });
        }

        private void PushStack(float number)
        {
            Stack.Add(new JavascriptMemory { Type = JavascriptMemoryType.Float, ValueFloat = number });
        }

        private void PushStack(JavascriptMemory memory)
        {
            Stack.Add(memory);
        }

        private JavascriptMemory PopStack()
        {
            var current = Stack.Last();
            Stack.RemoveAt(Stack.Count - 1);
            return current;
        }

        public void PushContext(int index, JavascriptMemory memory)
        {
            while (Context[Context.Count - 1].Count <= index)
            {
                Context[Context.Count - 1].Add(null);
            }
            Context[Context.Count - 1][index] = memory;
        }

        public JavascriptMemory GetContext(int index)
        {
            if (index >= Context[Context.Count - 1].Count) return new JavascriptMemory { Type = JavascriptMemoryType.Hole };
            return Context[Context.Count - 1][index];
        }

        public void PushGlobalContext(int index, JavascriptMemory memory)
        {
            while (GlobalContext.Count <= index)
            {
                GlobalContext.Add(null);
            }
            GlobalContext[index] = memory;
        }

        public JavascriptMemory GetGlobalContext(int index)
        {
            if (index >= GlobalContext.Count) return new JavascriptMemory { Type = JavascriptMemoryType.Hole };
            return GlobalContext[index];
        }
    }

    public class JavascriptMemory
    {
        public JavascriptMemoryType Type { get; set; }
        public string ValueString { get; set; }
        public long ValueNumber { get; set; }
        public float ValueFloat { get; set; }

        public float GetNumericValue()
        {
            return Type switch
            {
                JavascriptMemoryType.Number => ValueNumber,
                JavascriptMemoryType.Float => ValueFloat,
                _ => throw new InvalidOperationException("Invalid type for numeric value")
            };
        }
    }

    public enum JavascriptMemoryType
    {
        Hole,
        String,
        Number,
        Float
    }
}
