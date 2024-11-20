using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavascriptEmulator
{
    public class JavascriptBaseBytecodeBuilder
    {
        private List<long> bytecodes = new List<long>();

        public JavascriptBaseBytecodeBuilder() 
        {

        }

        public long[] GetBytecodes()
        {
            return bytecodes.ToArray();
        }

        private int CurrentIndex()
        {
            return bytecodes.Count;
        }

        public void Build(JavascriptAst ast)
        {
            switch(ast.NodeType)
            {
                case JavascriptAstType.Block:
                    BuildBlock((JavascriptAstBlock)ast);
                    break;
                case JavascriptAstType.Var:
                    BuildVariable((JavascriptAstVar)ast);
                    break;
                case JavascriptAstType.BinOp:
                    BuildBinOp((JavascriptAstBinaryOperation)ast);
                    break;
                case JavascriptAstType.Ident:
                    BuildIdent((JavascriptAstIdent)ast);
                    break;
                case JavascriptAstType.Number:
                    BuildNumber((JavascriptAstNumber)ast);
                    break;
            }
        }

        public void BuildIdent(JavascriptAstIdent ast)
        {
            if((ast.Flag & JavascriptAstIdentFlag.StoredInGlobalContext) != 0)
                bytecode(JavascriptByteCodes.LdaGlobal);
            else
                bytecode(JavascriptByteCodes.LdaContextSlot);

            bytecode(ast.Index);
        }

        public void BuildNumber(JavascriptAstNumber ast)
        {
            if(ast.Type == JavascriptAstNumberType.Integer)
            {
                bytecode(JavascriptByteCodes.LdaSmi);
                bytecode(ast.Value);
            }
            else
            {
                bytecode(JavascriptByteCodes.LdaConstant);
                bytecode(ast.Index);
                //DoubleToLong((double)ast.ValueFloat)
            }
        }

        private long DoubleToLong(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        public void BuildBinOp(JavascriptAstBinaryOperation ast)
        {
            Build(ast.Left);
            Build(ast.Right);

            switch (ast.Operator)
            {
                case JavascriptAstBinaryOperationType.Plus:
                    bytecode(JavascriptByteCodes.Add);
                    break;
                case JavascriptAstBinaryOperationType.Minus:
                    bytecode(JavascriptByteCodes.Sub);
                    break;
                case JavascriptAstBinaryOperationType.Mul:
                    bytecode(JavascriptByteCodes.Mul);
                    break;
                case JavascriptAstBinaryOperationType.Divide:
                    bytecode(JavascriptByteCodes.Div);
                    break;
            }
        }

        public void BuildBlock(JavascriptAstBlock ast)
        {
            if(!ast.Context.IsGlobal) 
            {
                bytecode(JavascriptByteCodes.PushContext);
            }

            foreach (var element in ast.Elements)
            {
                Build(element);
            }

            if (!ast.Context.IsGlobal)
            {
                bytecode(JavascriptByteCodes.PopContext);
            }
        }

        public void BuildVariable(JavascriptAstVar ast)
        {            
            Build(ast.Value);
            if (ast.Context.IsGlobal)
            {
                bytecode(JavascriptByteCodes.StaGlobal);
            }
            else
            {
                bytecode(JavascriptByteCodes.StaCurrentContextSlot);
            }
            bytecode(ast.Index);
        }

        private void bytecode(long bytecode)
        {
            bytecodes.Add(bytecode);
        }

        private void bytecode(JavascriptByteCodes bytecode)
        {
            bytecodes.Add((int)bytecode);
        }
    }

    public enum JavascriptByteCodes
    {
        None = 0,
        //Extended width operands
        Wide,
        ExtraWide,
        //Debug Breakpoints
        DebugBreakWide,
        DebugBreakExtraWide,
        DebugBreak0,
        DebugBreak1,
        DebugBreak2,
        DebugBreak3,
        DebugBreak4,
        DebugBreak5,
        DebugBreak6,
        //Side-effect-free bytecodes
        Ldar,
        LdaZero,
        LdaSmi,
        LdaUndefined,
        LdaNull,
        LdaTheHole,
        LdaTrue,
        LdaFalse,
        LdaConstant,
        LdaContextSlot,
        LdaImmutableContextSlot,
        LdaCurrentContextSlot,
        LdaImmutableCurrentContextSlot,
        Star,
        Mov,
        PushContext,
        PopContext,
        TestReferenceEqual,
        TestUndetectable,
        TestNull,
        TestUndefined,
        TestTypeOf,
        //Globals
        LdaGlobal,
        LdaGlobalInsideTypeof,
        StaGlobal,
        //Context operations
        StaContextSlot,
        StaCurrentContextSlot,
        StaScriptContextSlot,
        StaCurrentScriptContextSlot,
        //Load-Store lookup slots
        LdaLookupSlot,
        LdaLookupContextSlot,
        LdaLookupGlobalSlot,
        LdaLookupSlotInsideTypeof,
        LdaLookupContextSlotInsideTypeof,
        LdaLookupGlobalSlotInsideTypeof,
        StaLookupSlot,
        //Property loads (LoadIC) operations
        GetNamedProperty,
        GetNamedPropertyFromSuper,
        GetKeyedProperty,
        GetEnumeratedKeyedProperty,
        //Operations on module variables
        LdaModuleVariable,
        StaModuleVariable,
        //Propery stores (StoreIC) operations
        SetNamedProperty,
        DefineNamedOwnProperty,
        SetKeyedProperty,
        DefineKeyedOwnProperty,
        StaInArrayLiteral,
        DefineKeyedOwnPropertyInLiteral,
        //Binary Operators
        Add,
        Sub,
        Mul,
        Div,
        Mod,
        Exp,
        BitwiseOr,
        BitwiseXor,
        BitwiseAnd,
        ShiftLeft,
        ShiftRight,
        ShiftRightLogical,
        //Binary operators with immediate operands
        AddSmi,
        SubSmi,
        MulSmi,
        DivSmi,
        ModSmi,
        ExpSmi,
        BitwiseOrSmi,
        BitwiseXorSmi,
        BitwiseAndSmi,
        ShiftLeftSmi,
        ShiftRightSmi,
        ShiftRightLogicalSmi,
        //Unary Operators
        Inc,
        Dec,
        Negate,
        BitwiseNot,
        ToBooleanLogicalNot,
        LogicalNot,
        TypeOf,
        DeletePropertyStrict,
        DeletePropertySloppy,
        //GetSuperConstructor operator
        GetSuperConstructor,
        FindNonDefaultConstructorOrConstruct,
        //Call operations
        CallAnyReceiver,
        CallProperty,
        CallProperty0,
        CallProperty1,
        CallProperty2,
        CallUndefinedReceiver,
        CallUndefinedReceiver0,
        CallUndefinedReceiver1,
        CallUndefinedReceiver2,
        CallWithSpread,
        CallRuntime,
        CallRuntimeForPair,
        CallJSRuntime,
        //Intrinsics
        InvokeIntrinsic,
        //Construct operators
        Construct,
        ConstructWithSpread,
        ConstructForwardAllArgs,
        //Effectful Test Operators
        TestEqual,
        TestEqualStrict,
        TestLessThan,
        TestGreaterThan,
        TestLessThanOrEqual,
        TestGreaterThanOrEqual,
        TestInstanceOf,
        TestIn,
        //Cast operators
        ToName,
        ToNumber,
        ToNumeric,
        ToObject,
        ToString,
        ToBoolean,
        //Literals
        CreateRegExpLiteral,
        CreateArrayLiteral,
        CreateArrayFromIterable,
        CreateEmptyArrayLiteral,
        CreateObjectLiteral,
        CreateEmptyObjectLiteral,
        CloneObject,
        //Tagged templates
        GetTemplateObject,
        //Closure allocation
        CreateClosure,
        //Context allocation
        CreateBlockContext,
        CreateCatchContext,
        CreateFunctionContext,
        CreateEvalContext,
        CreateWithContext,
        //Arguments allocation
        CreateMappedArguments,
        CreateUnmappedArguments,
        CreateRestParameter,
        //Control Flow
        JumpLoop,
        Jump,
        JumpConstant,
        JumpIfNullConstant,
        JumpIfNotNullConstant,
        JumpIfUndefinedConstant,
        JumpIfUndefinedOrNullConstant,
        JumpIfTrueConstant,
        JumpIfFalseConstant,
        JumpIfJSReceiverConstant,
        JumpIfForInDoneConstant,
        JumpIfToBooleanTrueConstant,
        JumpIfToBooleanFalseConstant,
        JumpIfToBooleanTrue,
        JumpIfToBooleanFalse,
        JumpIfTrue,
        JumpIfFalse,
        JumpIfNull,
        JumpIfNotNull,
        JumpIfUndefined,
        JumpIfNotUndefined,
        JumpIfUndefinedOrNull,
        JumpIfJSReceiver,
        JumpIfForInDone,
        //Smi-table lookup for switch statements
        SwitchOnSmiNoFeedback,
        //Complex flow control For..in
        ForInEnumerate,
        ForInPrepare,
        ForInNext,
        ForInStep,
        //Update the pending message
        SetPendingMessage,
        //Non-local flow control
        Throw,
        ReThrow,
        Return,
        ThrowReferenceErrorIfHole,
        ThrowSuperNotCalledIfHole,
        ThrowSuperAlreadyCalledIfNotHole,
        ThrowIfNotSuperConstructor,
        //Generators
        SwitchOnGeneratorState,
        SuspendGenerator,
        ResumeGenerator,
        //Iterator protocol operations
        GetIterator,
        //Debugger
        Debugger,
        //Block Coverage
        IncBlockCounter,
        //Execution Abort (internal error)
        Abort
    }
}
