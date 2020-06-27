
using System;
using System.IO;
using System.Collections.Generic;
using GardensPoint;
using System.Threading;
using QUT.Gppg;

public enum Type
{
    Default, String, Int, Double, Bool, Plus, Minus, Multiply, Divide, Declaration, BitOr, BitAnd, UnaryMinus, BitNegation, Negation, IntConversion, DoubleConversion, Or, And, Equal,
    NotEqual, Greater, Smaller, SmallerOrEqual, GreaterOrEqual, Main, Error, Return, If, Else, While, Read, Write, Assign, Ident, AssignMid, StandaloneExp
}


public class Compiler
{

    public static int errors = 0;
    public static int line = 1;
    public static int etNumber = 1;
    public static List<string> source;
    public static StructTree tree;
    public static Dictionary<string, Type> variables = new Dictionary<string, Type>();
    public static Stack<StructTree> stackTree = new Stack<StructTree>();
    // arg[0] określa plik źródłowy
    // pozostałe argumenty są ignorowane
    public static int Main(string[] args)
    {

        string file = "";
        FileStream source;
        Console.WriteLine("\n Mini Language compilator");
        if (args.Length >= 1)
            file = args[0];
        else
        {
            Console.WriteLine("No file passed.");
            return 2;
        }
        try
        {
            var sr = new StreamReader(file);
            string str = sr.ReadToEnd();
            sr.Close();
            Compiler.source = new System.Collections.Generic.List<string>(str.Split(new string[] { "\r\n" }, System.StringSplitOptions.None));
            source = new FileStream(file, FileMode.Open);
        }
        catch (Exception e)
        {
            Console.WriteLine("\n" + e.Message);
            return 1;
        }
        Scanner scanner = new Scanner(source);
        Parser parser = new Parser(scanner);
        Console.WriteLine();

        sw = new StreamWriter(file + ".il");
        GenProlog();
        parser.Parse();

        StructTree currentnode = tree;
        if (errors == 0)
        {
            CheckTypeTree(currentnode);
        }
        if (errors == 0)
        {
            GenCode(tree);
        }
        GenEpilog();
        sw.Close();
        source.Close();
        if (errors == 0)
            Console.WriteLine("  compilation successful\n");
        else
        {
            Console.WriteLine($"\n  {errors} errors detected\n");
            File.Delete(file + ".il");
        }
        Thread.Sleep(4000);
        return errors == 0 ? 0 : 2;
    }

    public static void CheckTypeTree(StructTree node)
    {
        node.CheckType();
    }

    public static void GenCode(StructTree node)
    {
        node.GenCode();

    }
    public static void EmitCode(string instr = null)
    {
        sw.WriteLine(instr);
    }

    public static void EmitCode(string instr, params object[] args)
    {
        sw.WriteLine(instr, args);
    }

    private static StreamWriter sw;

    private static void GenProlog()
    {
        EmitCode(".assembly extern mscorlib { }");
        EmitCode(".assembly Compilator { }");
        EmitCode(".method static void main()");
        EmitCode("{");
        EmitCode(".entrypoint");
        EmitCode(".try");
        EmitCode("{");
        EmitCode(" .maxstack 8");
        EmitCode();

        EmitCode();
    }

    private static void GenEpilog()
    {
        EmitCode("leave EndMain");
        EmitCode("}");
        EmitCode("catch [mscorlib]System.Exception");
        EmitCode("{");
        EmitCode("callvirt instance string [mscorlib]System.Exception::get_Message()");
        EmitCode("call void [mscorlib]System.Console::WriteLine(string)");
        EmitCode("leave EndMain");
        EmitCode("}");
        EmitCode("EndMain: ret");
        EmitCode("}");
    }

    public static Type GetValType(string s)
    {
        switch (s)
        {
            case "int":
                return Type.Int;
            case "double":
                return Type.Double;
            case "bool":
                return Type.Bool;
        }

        return Type.Error;
    }

    public static Type GetMulType(string s)
    {
        switch (s)
        {
            case "*":
                return Type.Multiply;
            case "/":
                return Type.Divide;
        }

        return Type.Error;
    }

    public static Type GetAddType(string s)
    {
        switch (s)
        {
            case "+":
                return Type.Plus;
            case "-":
                return Type.Minus;
        }

        return Type.Error;
    }

    public static Type GetUnaryType(string s)
    {
        switch (s)
        {
            case "-":
                return Type.UnaryMinus;
            case "~":
                return Type.BitNegation;
            case "!":
                return Type.Negation;
            case "(int)":
                return Type.IntConversion;
            case "(double)":
                return Type.DoubleConversion;
        }
        return Type.Error;
    }
    public static Type GetBitType(string s)
    {
        switch (s)
        {
            case "|":
                return Type.BitOr;
            case "&":
                return Type.BitAnd;
        }
        return Type.Error;
    }
    public static Type GetLogicType(string s)
    {
        switch (s)
        {
            case "||":
                return Type.Or;
            case "&&":
                return Type.And;
        }
        return Type.Error;
    }
    public static Type GetRelationType(string s)
    {
        switch (s)
        {
            case "==":
                return Type.Equal;
            case "!=":
                return Type.NotEqual;
            case ">":
                return Type.Greater;
            case "<":
                return Type.Smaller;
            case "<=":
                return Type.SmallerOrEqual;
            case ">=":
                return Type.GreaterOrEqual;
        }
        return Type.Error;
    }
}

public abstract class StructTree
{
    public Type type;
    public int line = -1;
    public abstract Type CheckType();
    public abstract void GenCode();
    public StructTree left = null;
    public StructTree right = null;

}

public class MainNode : StructTree
{
    public override Type CheckType()
    {
        Type type1 = Type.Default, type2 =Type.Default;
        if (left != null) type1 = left.CheckType();
        if (type1 == Type.Error) Console.WriteLine($" Error in line {line} \n");
        if (right != null) type2 = right.CheckType();
        if (type2 == Type.Error) Console.WriteLine($" Error in line {line} \n");
        return type;
    }

    public override void GenCode()
    {
        if (left != null) left.GenCode();
        if (right != null) right.GenCode();

    }
}

public class DeclarationNode : StructTree
{
    public override Type CheckType() { return type; }
    public Type varType;
    public string ident;
    public bool isValid() { return Compiler.variables.ContainsKey(ident); }
    public override void GenCode()
    {
        string s = ".locals init ";
        if (varType == Type.Int)
            s = s + "( int32 ";
        if (varType == Type.Double)
            s = s + "( float64 ";
        if (varType == Type.Bool)
            s = s + "( bool ";

        s = s + $"_{ident} )";

        Compiler.EmitCode(s);

        s = $"ldc.";
        if (varType == Type.Int)
            s = s + $"i4 0";
        if (varType == Type.Double)
            s = s + $"r8 0.0";
        if (varType == Type.Bool)
            s = s + "i4.0";

        Compiler.EmitCode(s);
        s = $"stloc _{ident}";
        Compiler.EmitCode(s);


    }
}

public class AssignNode : StructTree
{
    public override Type CheckType()
    {
        Type typeL = left.CheckType();
        if (typeL == Type.Error) return Type.Error;

        if (!Compiler.variables.ContainsKey(ident))
        {
            Console.Write($"Semantic error. Variable {ident} undeclared.");
            Compiler.errors++;
            return Type.Error;
        }
        if (Compiler.variables[ident] == Type.Bool && typeL !=Type.Bool)
        {
            Console.Write($"Semantic error. Value {typeL.ToString()} cannot be assigned to variable of type {Compiler.variables[ident] }");
            Compiler.errors++;
            return Type.Error;

        }

        if (Compiler.variables[ident] == Type.Int && typeL != Type.Int)
        {
            Console.Write($"Semantic error. Value {typeL.ToString()} cannot be assigned to variable of type {Compiler.variables[ident] }");
            Compiler.errors++;
            return Type.Error;

        }


        if (Compiler.variables[ident] == Type.Double && typeL == Type.Bool)
        {
            Console.Write($"Semantic error. Value {typeL.ToString()} cannot be assigned to variable of type {Compiler.variables[ident] }");
            Compiler.errors++;
            return Type.Error;

        }
        return Compiler.variables[ident];
    }
    public string ident;
    public override void GenCode()
    {
        if (left != null) left.GenCode();
        if (right != null) right.GenCode();
        if (left.CheckType() == Type.Int && Compiler.variables[ident] == Type.Double) Compiler.EmitCode("conv.r8");
        string s = $"stloc _{ident}";
        Compiler.EmitCode(s);
    }
}

public class LogicNode : StructTree
{
    public override Type CheckType()
    {
        Type type1 = left.CheckType();
        Type type2 = right.CheckType();
        if (type1 == Type.Error || type2 == Type.Error) return Type.Error;

        if (type1 != Type.Bool)
        {
            Console.Write($"Semantic error. Logic operation {type.ToString()} cannot be applied to {type1.ToString()}");
            Compiler.errors++;
            return Type.Error;

        }
        if (type2 != Type.Bool)
        {
            Console.Write($"Semantic error. Logic operation {type.ToString()} cannot be applied to {type2.ToString()}");
            Compiler.errors++;
            return Type.Error;

        }
        return Type.Bool;
    }
    public override void GenCode()
    {
        int et = Compiler.etNumber; Compiler.etNumber++;
        if (left != null) left.GenCode();
        Compiler.EmitCode("dup");
        if (type == Type.And)
            Compiler.EmitCode($"brfalse et{et}");
        if (type == Type.Or)
            Compiler.EmitCode($"brtrue et{et}");
        if (right != null) right.GenCode();
        if (type == Type.And)
            Compiler.EmitCode("and");
        if (type == Type.Or)
            Compiler.EmitCode("or");

        Compiler.EmitCode($"et{et}: nop");
    }
}

public class RelationNode : StructTree
{
    public override Type CheckType()
    {
        Type type1 = left.CheckType();
        Type type2 = right.CheckType();
        if (type1 == Type.Error || type2 == Type.Error) return Type.Error;
        if (type == Type.Greater || type == Type.Smaller || type == Type.SmallerOrEqual || type == Type.GreaterOrEqual)
        {
            if (type1 == Type.Bool)
            {
                Console.Write($"Semantic error. Logic operation {type.ToString()} cannot be applied to {type1.ToString()}");
                Compiler.errors++;
                return Type.Error;

            }
            if (type2 == Type.Bool)
            {
                Console.Write($"Semantic error. Logic operation {type} cannot be applied to {type2}");
                Compiler.errors++;
                return Type.Error;
            }
        }
        if (type == Type .Equal|| type == Type.NotEqual)
        {
            if ((type1 == Type.Bool && type2 != Type.Bool) || (type2 == Type.Bool && type1 != Type.Bool))
            {
                Console.Write($"Semantic error. Logic operation {type} cannot be applied to {type1} and {type2}");
                Compiler.errors++;
                return Type.Error;

            }
        }

        return Type.Bool;
    }
    public override void GenCode()
    {
        Type rightT = right.CheckType();
        Type leftT = left.CheckType();

        if (left != null) left.GenCode();
        if (leftT == Type.Double && rightT != Type.Double)
            Compiler.EmitCode("conv.r8");
        if (right != null) right.GenCode();
        if (leftT != Type.Double && rightT == Type.Double)
            Compiler.EmitCode("conv.r8");

        if (type == Type.Equal)
            Compiler.EmitCode("ceq");
        if (type == Type.NotEqual)
        {
            Compiler.EmitCode("ceq");
            Compiler.EmitCode("not");
        }
        if (type == Type.Greater)
            Compiler.EmitCode("cgt");
        if (type == Type.SmallerOrEqual)
        {
            Compiler.EmitCode("cgt");
            Compiler.EmitCode("not");
        }
        if (type == Type.Smaller)
            Compiler.EmitCode("clt");
        if (type == Type.GreaterOrEqual)
        {
            Compiler.EmitCode("clt");
            Compiler.EmitCode("not");
        }
    }
}

public class AddNode : StructTree
{
    public override Type CheckType()
    {
        Type type1 = left.CheckType();
        Type type2 = right.CheckType();
        if (type1 == Type.Error || type2 == Type.Error) return Type.Error;

        if (type1 == Type.Bool)
        {
            Console.Write($"Semantic error. Add operation {type} cannot be applied to {type1}");
            Compiler.errors++;
            return Type.Error;

        }
        if (type2 == Type.Bool)
        {
            Console.Write($"Semantic error. Add operation {type} cannot be applied to {type2}");
            Compiler.errors++;
            return Type.Error;

        }
        if (type1 == Type.Int && type1 == type2)
            return Type.Int;

        return Type.Double;
    }
    public override void GenCode()
    {
        Type rightT = right.CheckType();
        Type leftT = left.CheckType();

        left.GenCode();
        if (leftT != Type.Double && rightT == Type.Double)
            Compiler.EmitCode("conv.r8");
        right.GenCode();
        if (leftT == Type.Double && rightT != Type.Double)
            Compiler.EmitCode("conv.r8");


        string s;
        if (type == Type.Plus)
            s = "add";
        else
            s = "sub";
        Compiler.EmitCode(s);
    }
}

public class MulNode : StructTree
{
    public override Type CheckType()
    {
        Type type1 = left.CheckType();
        Type type2 = right.CheckType();
        if (type1 == Type.Error || type2 == Type.Error) return Type.Error;

        if (type1 == Type.Bool)
        {
            Console.Write($"Semantic error. Multiply operation {type} cannot be applied to {type1}");
            Compiler.errors++;
            return Type.Error;

        }
        if (type2 == Type.Bool)
        {
            Console.Write($"Semantic error. Multiply operation {type} cannot be applied to {type2}");
            Compiler.errors++;
            return Type.Error;

        }
        if (type1 == Type.Int && type1 == type2)
            return Type.Int;

        return Type.Double;
    }
    public override void GenCode()
    {
        Type rightT = right.CheckType();
        Type leftT = left.CheckType();

        left.GenCode();
        if (leftT != Type.Double && rightT == Type.Double)
            Compiler.EmitCode("conv.r8");
        right.GenCode();
        if (leftT == Type.Double && rightT != Type.Double)
            Compiler.EmitCode("conv.r8");

        string s;
        if (type == Type.Multiply)
            s = "mul";
        else
            s = "div";
        Compiler.EmitCode(s);
    }
}

public class BitNode : StructTree
{
    public override Type CheckType()
    {
        Type type1 = left.CheckType();
        Type type2 = right.CheckType();
        if (type1 == Type.Error || type2 == Type.Error) return Type.Error;

        if (type1 != Type.Int)
        {
            Console.Write($"Semantic error. Bit operation {type} cannot be applied to {type1}");
            Compiler.errors++;
            return Type.Error;

        }
        if (type2 != Type.Int)
        {
            Console.Write($"Semantic error. Bit operation {type} cannot be applied to {type2}");
            Compiler.errors++;
            return Type.Error;

        }
        return Type.Int;
    }
    public override void GenCode()
    {
        if (left != null) left.GenCode();
        if (right != null) right.GenCode();
        if (type == Type.BitAnd)
            Compiler.EmitCode("and");
        if (type == Type.BitOr)
            Compiler.EmitCode("or");
    }
}

public class UnaryNode : StructTree
{
    public override Type CheckType()
    {
        Type str = left.CheckType();
        if (str == Type.Error) return Type.Error;

        if (type == Type.UnaryMinus && str != Type.Double && str != Type.Int)
        {
            Compiler.errors++;
            Console.Write("Semantic error. Unary operator - can't be applied to bool value.");
            return Type.Error;

        }
        if (type == Type.BitNegation && str != Type.Int)
        {
            Compiler.errors++;
            Console.Write($"Semantic error. Unary operator ~ can't be applied to {str.ToString()} value.");
            return Type.Error;

        }
        if (type == Type.Negation && str != Type.Bool)
        {
            Compiler.errors++;
            Console.Write($"Semontic error. Unary operator ! can't be applied to {str.ToString()} value.");
            return Type.Error;

        }
        if (type == Type.IntConversion)
            str = Type.Int;
        if (type == Type.DoubleConversion)
            str = Type.Double;
        return str;
    }
    public override void GenCode()
    {
        left.GenCode();
        string s = "";
        if (type == Type.UnaryMinus)
        {
            s = "neg";
            Compiler.EmitCode(s);

        }
        if (type == Type.Negation | type == Type.BitNegation)
        {
            s = "not";
            Compiler.EmitCode(s);

        }
        if (type == Type.IntConversion)
        {
            s = "conv.i4";
            Compiler.EmitCode(s);

        }
        if (type == Type.DoubleConversion)
        {
            s = "conv.r8";
            Compiler.EmitCode(s);

        }
    }
}
public class IdentNode : StructTree
{
    public override Type CheckType()
    {
        if (Compiler.variables.ContainsKey(ident))
            return Compiler.variables[ident];

        Console.Write($"Semantic error. Variable {ident} wasn't declared.");
        Compiler.errors++;
        return Type.Error;

    }
    public string ident;
    public override void GenCode()
    {
        Compiler.EmitCode($"ldloc _{ident}");
    }
}

public class IntNode : StructTree
{
    public override Type CheckType() { return Type.Int; }
    public int value;
    public override void GenCode()
    {
        string s = string.Format("ldc.i4 {0}", value);
        Compiler.EmitCode(s);
    }
}


public class DoubleNode : StructTree
{
    public override Type CheckType() { return Type.Double; }
    public double value;
    public override void GenCode()
    {
        string s = string.Format(System.Globalization.CultureInfo.InvariantCulture, "ldc.r8 {0}", value);
        Compiler.EmitCode(s);
    }
}

public class BoolNode : StructTree
{
    public override Type CheckType() { return Type.Bool; }
    public bool value;
    public override void GenCode()
    {
        if (value)
            Compiler.EmitCode("ldc.i4 1");
        else
            Compiler.EmitCode("ldc.i4 0");
    }
}


public class StringNode : StructTree
{
    public override Type CheckType() { return Type.String; }
    public string value;
    public override void GenCode()
    {
        Compiler.EmitCode($"ldstr {value}");
    }
}

public class WriteNode : StructTree
{
    public override Type CheckType()
    {
        if (right != null) right.CheckType();
        if (left != null) left.CheckType();
        return Type.Write;
    }
    public override void GenCode()
    {
        Type rightT = right.CheckType();
        if (rightT != Type.String)
        {
            if (rightT == Type.Double)
            {
                Compiler.EmitCode("call class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()");
                Compiler.EmitCode("ldstr \"{0:0.000000}\"");
            }
            else
            {
                Compiler.EmitCode("ldstr \"{0}\"");
            }
            if (right != null) right.GenCode();
            if (left != null) left.GenCode();
            if (right != null && rightT == Type.Int)
                Compiler.EmitCode("box[mscorlib]System.Int32");
            if (right != null && rightT == Type.Double)
                Compiler.EmitCode("box[mscorlib]System.Double");
            if (right != null && rightT == Type.Bool)
                Compiler.EmitCode("box[mscorlib]System.Boolean");

            if (rightT == Type.Double)
            {
                Compiler.EmitCode("call string [mscorlib]System.String::Format(class [mscorlib]System.IFormatProvider,string,object)");
                Compiler.EmitCode("call void [mscorlib]System.Console::Write(string)");
            }
            else
                Compiler.EmitCode("call void [mscorlib]System.Console::Write(string, object)");
        }
        else
        {
            if (right != null) right.GenCode();
            if (left != null) left.GenCode();
            Compiler.EmitCode("call void [mscorlib]System.Console::Write(string)");
        }
    }
}

public class ReadNode : StructTree
{
    public override Type CheckType()
    {
        if (Compiler.variables.ContainsKey(value))
            return Compiler.variables[value];

        Console.Write($"Semantic error. Variable {value} wasn't declared.");
        Compiler.errors++;
        return Type.Error;
    }
    public string value;
    public override void GenCode()
    {

        Compiler.EmitCode("call string [mscorlib]System.Console::ReadLine()");
        if (Compiler.variables[value] == Type.Double)
        {
            Compiler.EmitCode("call  class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()");
            Compiler.EmitCode("call  float64 [mscorlib]System.Double::Parse(string,class [mscorlib]System.IFormatProvider)");
        }
        if (Compiler.variables[value] == Type.Int)
            Compiler.EmitCode("call  int32 [mscorlib]System.Int32::Parse(string)");
        if (Compiler.variables[value] == Type.Bool)
            Compiler.EmitCode("call bool [mscorlib]System.Boolean::Parse(string)");

        Compiler.EmitCode($"stloc _{value}");
    }
}

public class WhileNode : StructTree
{
    public override Type CheckType()
    {

        Type type1 = left.CheckType();
        if (type1 == Type.Error) return Type.Error;

        if (type1 != Type.Bool)
        {
            Console.Write($"Semantic error. While condition cannot be of type {type1.ToString()}");
            Compiler.errors++;
            return Type.Error;

        }
        if (right != null) right.CheckType();

        return Type.While;
    }
    public string value;
    public override void GenCode()
    {
        int etykieta1 = Compiler.etNumber;
        Compiler.etNumber++;
        int etykieta2 = Compiler.etNumber;
        Compiler.etNumber++;
        Compiler.EmitCode($"et{etykieta1}: nop");
        if (left != null) left.GenCode();
        Compiler.EmitCode($"brfalse  et{etykieta2}");
        if (right != null) right.GenCode();
        Compiler.EmitCode($"br et{etykieta1}");
        Compiler.EmitCode($"et{etykieta2}: nop");

    }
}


public class IfNode : StructTree
{
    public override Type CheckType()
    {
        Type type1 = left.CheckType();
        if (type1 == Type.Error) return Type.Error;

        if (type1 != Type.Bool)
        {
            Console.Write($"If condition cannot be of type {type1.ToString()}");
            Compiler.errors++;
            return Type.Error;

        }
        if (right != null) right.CheckType();

        return Type.If;
    }
    public override void GenCode()
    {
        int et1=Compiler.etNumber; Compiler.etNumber++;
        if (left != null) left.GenCode();
        Compiler.EmitCode($"brfalse et{et1}");
        if (right != null) right.GenCode();
        Compiler.EmitCode($"et{et1}: nop");
    }
}


public class IfElseNode : StructTree
{
    public override Type CheckType()
    {
        Type type1 = left.CheckType();
        if (type1 == Type.Error) return Type.Error;

        if (type1 != Type.Bool)
        {
            Console.Write($"If condition cannot be of type {type1.ToString()}");
            Compiler.errors++;
            return Type.Error;

        }
        if (elseNode != null) elseNode.CheckType();
        if (right != null) right.CheckType();
        return Type.If;
    }
    public StructTree elseNode;
    public override void GenCode()
    {
        int et1 = Compiler.etNumber; Compiler.etNumber++;
        int et2 = Compiler.etNumber; Compiler.etNumber++;
        if (left != null) left.GenCode();
        Compiler.EmitCode($"brfalse et{et1}");
        if (right != null) right.GenCode();
        Compiler.EmitCode($"br et{et2}");
        Compiler.EmitCode($"et{et1}: nop");
        if (elseNode != null) elseNode.GenCode();
        Compiler.EmitCode($"et{et2}: nop");

    }
}

public class ReturnNode : StructTree
{
    public override Type CheckType()
    {
        return Type.Return;
    }
    public override void GenCode()
    {
        Compiler.EmitCode($"leave EndMain");
    }
}

public class AssignMidNode : AssignNode
{
    public override void GenCode()
    {
        if (left != null) left.GenCode();
        if (right != null) right.GenCode();
        if (left.CheckType() == Type.Int && Compiler.variables[ident]==Type.Double) Compiler.EmitCode("conv.r8");
        string s = $"stloc _{ident}";
        Compiler.EmitCode(s);
        s = $"ldloc _{ident}";
        Compiler.EmitCode(s);
    }


}

public class StandaloneExpNode : StructTree
{
    public override Type CheckType()
    {
        Type type1 = Type.Default, type2 = Type.Default;
        if (left != null) type1 = left.CheckType();
        if (right != null) type2 = right.CheckType();
        if (type1 == Type.Error || type2 == Type.Error) return Type.Error;
        return type;
    }
    public override void GenCode()
    {
        if (left != null) left.GenCode();
        if (right != null) right.GenCode();
        Compiler.EmitCode("pop");
    }


}