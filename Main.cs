
using System;
using System.IO;
using System.Collections.Generic;
using GardensPoint;
using System.Threading;
using SymTab;
using QUT.Gppg;

public enum Type
{
    String, Int, Double, Bool, Plus, Minus, Times, Divide, Declaration, BitOr, BitAnd, UnMinus, BitNeg, Neg, IntConv, DoubleConv, Or, And, Equal,
    NotEqual, Greater, Smaller, SmallerOrEq, GreaterOrEq, Main, Error, Return
}


public class Compiler
{

    public static int errors = 0;
    public static int line = 1;
    public static int etNumber = 1;
    public static List<string> source;
    public static StructTree tree;
    public static Dictionary<string, string> variables = new Dictionary<string, string>();
    public static Stack<StructTree> stackTree = new Stack<StructTree>();
    // arg[0] określa plik źródłowy
    // pozostałe argumenty są ignorowane
    public static int Main(string[] args)
    {

        string file = "";
        FileStream source;
        Console.WriteLine("\nSingle-Pass CIL Code Generator for Multiline Calculator - Gardens Point");
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
    \ Declaration, BitOr, BitAnd,Or, And, Equal,
    NotEqual, Greater, Smaller, SmallerOrEq, Main, Error, Return

    public Type GetValType(string s)
    {
        switch (s)
        {
            case "string":
                return Type.String;
            case "int":
                return Type.Int;
            case "double":
                return Type.Double;
            case "bool":
                return Type.Bool;
        }

        return Type.Error;
    }

    public Type GetMulType(string s)
    {
        switch (s)
        {
            case "*":
                return Type.Times;
            case "/":
                return Type.Divide;
        }

        return Type.Error;
    }

    public Type GetAddType(string s)
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

    public Type GetUnaryType(string s)
    {
        switch (s)
        {
            case "-":
                return Type.UnMinus;
            case "~":
                return Type.BitNeg;
            case "!":
                return Type.Neg;
            case "(int)":
                return Type.IntConv;
            case "(double)":
                return Type.DoubleConv;
        }
        return Type.Error;
    }
    public Type GetBitType(string s)
    {
        switch (s)
        {
            case "|":
                return Type.BitAnd;
            case "&":
                return Type.BitOr;
        }
        return Type.Error;
    }
    public Type GetLogicType(string s)
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
    public Type GetRelationType(string s)
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
                return Type.SmallerOrEq;
            case ">=":
                return Type.GreaterOrEq;
        }
        return Type.Error;
    }
}

public abstract class StructTree
{
    public string type;
    public int line = -1;
    public abstract string CheckType();
    public abstract void GenCode();
    public StructTree left = null;
    public StructTree right = null;

}

public class MainNode : StructTree
{
    public override string CheckType()
    {
        string type1 = "", type2 = "";
        if (left != null) type1 = left.CheckType();
        if (right != null) type2 = right.CheckType();
        if (type1 == "error" || type2 == "error") Console.WriteLine($" Error in line {line}");
        return "";
    }

    public override void GenCode()
    {
        if (left != null) left.GenCode();
        if (right != null) right.GenCode();

    }
}

public class DeclarationNode : StructTree
{
    public override string CheckType() { return ""; }
    public string varType;
    public string ident;
    public bool isValid() { return Compiler.variables.ContainsKey(ident); }
    public override void GenCode()
    {
        string s = ".locals init ";
        if (varType == "int")
            s = s + "( int32 ";
        if (varType == "double")
            s = s + "( float64 ";
        if (varType == "bool")
            s = s + "( bool ";

        s = s + $"_{ident} )";

        Compiler.EmitCode(s);

        s = $"ldc.";
        if (varType == "int")
            s = s + $"i4 0";
        if (varType == "double")
            s = s + $"r8 0.0";
        if (varType == "bool")
            s = s + "i4.0";

        Compiler.EmitCode(s);
        s = $"stloc _{ident}";
        Compiler.EmitCode(s);


    }
}

public class AssignNode : StructTree
{
    public override string CheckType()
    {
        string typeL = left.CheckType();
        if (typeL == "error") return "error";

        if (!Compiler.variables.ContainsKey(ident))
        {
            Console.Write($"Semantic error. Variable {ident} undeclared.");
            Compiler.errors++;
            return "error";
        }
        if (Compiler.variables[ident] == "bool" && typeL != "bool")
        {
            Console.Write($"Semantic error. Value {typeL} cannot be assigned to variable of type {Compiler.variables[ident] }");
            Compiler.errors++;
            return "error";

        }

        if (Compiler.variables[ident] == "int" && typeL != "int")
        {
            Console.Write($"Semantic error. Value {typeL} cannot be assigned to variable of type {Compiler.variables[ident] }");
            Compiler.errors++;
            return "error";

        }


        if (Compiler.variables[ident] == "double" && typeL == "bool")
        {
            Console.Write($"Semantic error. Value {typeL} cannot be assigned to variable of type {Compiler.variables[ident] }");
            Compiler.errors++;
            return "error";

        }
        return Compiler.variables[ident];
    }
    public string ident;
    public override void GenCode()
    {
        if (left != null) left.GenCode();
        if (right != null) right.GenCode();
        if (left.CheckType() == "double") Compiler.EmitCode("conv.i4");
        string s = $"stloc _{ident}";
        Compiler.EmitCode(s);
    }
}

public class LogicNode : StructTree
{
    public override string CheckType()
    {
        string type1 = left.CheckType();
        string type2 = right.CheckType();
        if (type1 == "error" || type2 == "error") return "error";

        if (type1 != "bool")
        {
            Console.Write($"Semantic error. Logic operation {type} cannot be applied to {type1}");
            Compiler.errors++;
            return "error";

        }
        if (type2 != "bool")
        {
            Console.Write($"Semantic error. Logic operation {type} cannot be applied to {type2}");
            Compiler.errors++;
            return "error";

        }
        return "bool";
    }
    public override void GenCode()
    {
        int et = Compiler.etNumber; Compiler.etNumber++;
        if (left != null) left.GenCode();
        Compiler.EmitCode("dup");
        if (type == "&&")
            Compiler.EmitCode($"brfalse et{et}");
        if (type == "||")
            Compiler.EmitCode($"brtrue et{et}");   

        if (right != null) right.GenCode();

        if (type == "&&")
            Compiler.EmitCode("and");
        if (type == "||")
            Compiler.EmitCode("or");

        Compiler.EmitCode($"et{et}: nop");
    }
}

public class RelationNode : StructTree
{
    public override string CheckType()
    {
        string type1 = left.CheckType();
        string type2 = right.CheckType();
        if (type1 == "error" || type2 == "error") return "error";
        if (type == ">" || type == "<" || type == "<=" || type == ">=")
        {
            if (type1 == "bool")
            {
                Console.Write($"Semantic error. Logic operation {type} cannot be applied to {type1}");
                Compiler.errors++;
                return "error";

            }
            if (type2 == "bool")
            {
                Console.Write($"Semantic error. Logic operation {type} cannot be applied to {type2}");
                Compiler.errors++;
                return "error";
            }
        }
        if (type == "==" || type == "!=")
        {
            if ((type1 == "bool" && type2 != "bool") || (type2 == "bool" && type1 != "bool"))
            {
                Console.Write($"Semantic error. Logic operation {type} cannot be applied to {type1} and {type2}");
                Compiler.errors++;
                return "error";

            }
        }

        return "bool";
    }
    public override void GenCode()
    {
        string rightT = right.CheckType();
        string leftT = left.CheckType();

        right.GenCode();
        if (leftT == "double" && rightT != "double")
            Compiler.EmitCode("conv.r8");
        left.GenCode();
        if (leftT != "double" && rightT == "double")
            Compiler.EmitCode("conv.r8");

        if (type == "==")
            Compiler.EmitCode("ceq");
        if (type == "!=")
        {
            Compiler.EmitCode("ceq");
            Compiler.EmitCode("not");
        }
        if (type == ">")
            Compiler.EmitCode("cgt");
        if (type == "<=")
        {
            Compiler.EmitCode("cgt");
            Compiler.EmitCode("not");
        }
        if (type == "<")
            Compiler.EmitCode("clt");
        if (type == ">=")
        {
            Compiler.EmitCode("clt");
            Compiler.EmitCode("not");
        }
    }
}

public class AddNode : StructTree
{
    public override string CheckType()
    {
        string type1 = left.CheckType();
        string type2 = right.CheckType();
        if (type1 == "error" || type2 == "error") return "error";

        if (type1 == "bool")
        {
            Console.Write($"Semantic error. Add operation {type} cannot be applied to {type1}");
            Compiler.errors++;
            return "error";

        }
        if (type2 == "bool")
        {
            Console.Write($"Semantic error. Add operation {type} cannot be applied to {type2}");
            Compiler.errors++;
            return "error";

        }
        if (type1 == "int" && type1 == type2)
            return "int";

        return "double";
    }
    public override void GenCode()
    {
        string rightT = right.CheckType();
        string leftT = left.CheckType();

        right.GenCode();
        if (leftT == "double" && rightT != "double")
            Compiler.EmitCode("conv.r8");
        left.GenCode();
        if (leftT != "double" && rightT == "double")
            Compiler.EmitCode("conv.r8");

        string s;
        if (type == "+")
            s = "add";
        else
            s = "sub";
        Compiler.EmitCode(s);
    }
}

public class MulNode : StructTree
{
    public override string CheckType()
    {
        string type1 = left.CheckType();
        string type2 = right.CheckType();
        if (type1 == "error" || type2 == "error") return "error";

        if (type1 == "bool")
        {
            Console.Write($"Semantic error. Multiply operation {type} cannot be applied to {type1}");
            Compiler.errors++;
            return "error";

        }
        if (type2 == "bool")
        {
            Console.Write($"Semantic error. Multiply operation {type} cannot be applied to {type2}");
            Compiler.errors++;
            return "error";

        }
        if (type1 == "int" && type1 == type2)
            return "int";

        return "double";
    }
    public override void GenCode()
    {
        string rightT = right.CheckType();
        string leftT = left.CheckType();

        right.GenCode();
        if (leftT == "double" && rightT != "double")
            Compiler.EmitCode("conv.r8");
        left.GenCode();
        if (leftT != "double" && rightT == "double")
            Compiler.EmitCode("conv.r8");
        string s;
        if (type == "*")
            s = "mul";
        else
            s = "div";
        Compiler.EmitCode(s);
    }
}

public class BitNode : StructTree
{
    public override string CheckType()
    {
        string type1 = left.CheckType();
        string type2 = right.CheckType();
        if (type1 == "error" || type2 == "error") return "error";

        if (type1 != "int")
        {
            Console.Write($"Semantic error. Bit operation {type} cannot be applied to {type1}");
            Compiler.errors++;
            return "error";

        }
        if (type2 != "int")
        {
            Console.Write($"Semantic error. Bit operation {type} cannot be applied to {type2}");
            Compiler.errors++;
            return "error";

        }
        return "int";
    }
    public override void GenCode()
    {
        if (left != null) left.GenCode();

        if (type == "&")
            Compiler.EmitCode("and");
        if (type == "|")
            Compiler.EmitCode("or");
    }
}

public class UnaryNode : StructTree
{
    public override string CheckType()
    {
        string str = left.CheckType();
        if (str == "error") return "error";

        if (type == "-" && str != "double" && str != "int")
        {
            Compiler.errors++;
            Console.Write("Semantic error. Unary operator - can't be applied to bool value.");
            return "error";

        }
        if (type == "~" && str != "int")
        {
            Compiler.errors++;
            Console.Write($"Semantic error. Unary operator ~ can't be applied to {str} value.");
            return "error";

        }
        if (type == "!" && str != "bool")
        {
            Compiler.errors++;
            Console.Write($"Semontic error. Unary operator ! can't be applied to {str} value.");
            return "error";

        }
        if (type == "(int)")
            str = "int";
        if (type == "(double)")
            str = "double";
        return str;
    }
    public override void GenCode()
    {
        left.GenCode();
        string s = "";
        if (type == "-")
        {
            s = "neg";
            Compiler.EmitCode(s);

        }
        if (type == "!" | type == "~")
        {
            s = "not";
            Compiler.EmitCode(s);

        }
        if (type == "(int)")
        {
            s = "conv.i4";
            Compiler.EmitCode(s);

        }
        if (type == "(double)")
        {
            s = "conv.r8";
            Compiler.EmitCode(s);

        }
    }
}
public class IdentNode : StructTree
{
    public override string CheckType()
    {
        if (Compiler.variables.ContainsKey(ident))
            return Compiler.variables[ident];

        Console.Write($"Semantic error. Variable {ident} wasn't declared.");
        Compiler.errors++;
        return "error";

    }
    public string ident;
    public override void GenCode()
    {
        Compiler.EmitCode($"ldloc _{ident}");
    }
}

public class IntNode : StructTree
{
    public override string CheckType() { return "int"; }
    public int value;
    public override void GenCode()
    {
        string s = string.Format("ldc.i4 {0}", value);
        Compiler.EmitCode(s);
    }
}


public class DoubleNode : StructTree
{
    public override string CheckType() { return "double"; }
    public double value;
    public override void GenCode()
    {
        string s = string.Format(System.Globalization.CultureInfo.InvariantCulture, "ldc.r8 {0}", value);
        Compiler.EmitCode(s);
    }
}

public class BoolNode : StructTree
{
    public override string CheckType() { return "bool"; }
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
    public override string CheckType() { return "string"; }
    public string value;
    public override void GenCode()
    {
        Compiler.EmitCode($"ldstr {value}");
    }
}

public class WriteNode : StructTree
{
    public override string CheckType()
    {
        if (right != null) right.CheckType();
        if (left != null) left.CheckType();
        return "";
    }
    public override void GenCode()
    {
        string rightT = right.CheckType();
        if (rightT != "string")
        {
            if (rightT == "double")
            {
                Compiler.EmitCode(" call class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()");
                Compiler.EmitCode("ldstr \"{0:0.000000}\"");
            }
            else
            {
                Compiler.EmitCode("ldstr \"{0}\"");
            }
            if (right != null) right.GenCode();
            if (left != null) left.GenCode();
            if (right != null && rightT == "int")
                Compiler.EmitCode("box[mscorlib]System.Int32");
            if (right != null && rightT == "double")
                Compiler.EmitCode("box[mscorlib]System.Double");
            if (right != null && rightT == "bool")
                Compiler.EmitCode("box[mscorlib]System.Boolean");

            if (rightT == "double")
            {
                Compiler.EmitCode("call string [mscorlib]System.String::Format(class [mscorlib]System.IFormatProvider,string,object)");
                Compiler.EmitCode("call void [mscorlib]System.Console::WriteLine(string)");
            }
            else
                Compiler.EmitCode("call void [mscorlib]System.Console::WriteLine(string, object)");
        }
        else
        {
            if (right != null) right.GenCode();
            if (left != null) left.GenCode();
            Compiler.EmitCode("call void [mscorlib]System.Console::WriteLine(string)");
        }
    }
}

public class ReadNode : StructTree
{
    public override string CheckType()
    {
        if (Compiler.variables.ContainsKey(value))
            return Compiler.variables[value];

        Console.Write($"Semantic error. Variable {value} wasn't declared.");
        Compiler.errors++;
        return "error";
    }
    public string value;
    public override void GenCode()
    {

        Compiler.EmitCode("call string [mscorlib]System.Console::ReadLine()");
        if (Compiler.variables[value] == "double")
        {
            Compiler.EmitCode(" call  class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()");
            Compiler.EmitCode(" call  float64 [mscorlib]System.Double::Parse(string,class [mscorlib]System.IFormatProvider)");
        }
        if (Compiler.variables[value] == "int")
            Compiler.EmitCode(" call  int32 [mscorlib]System.Int32::Parse(string)");
        if (Compiler.variables[value] == "bool")
            Compiler.EmitCode(" call bool [mscorlib]System.Boolean" +
                "::Parse(string)");

        Compiler.EmitCode($"stloc _{value}");
    }
}

public class WhileNode : StructTree
{
    public override string CheckType()
    {

        string type1 = left.CheckType();
        if (type1 == "error") return "error";

        if (type1 != "bool")
        {
            Console.Write($"Semantic error. While condition cannot be of type {type1}");
            Compiler.errors++;
            return "error";

        }
        if (right != null) right.CheckType();

        return "";
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
    public override string CheckType()
    {
        string type1 = left.CheckType();
        if (type1 == "error") return "error";

        if (type1 != "bool")
        {
            Console.Write($"If condition cannot be of type {type1}");
            Compiler.errors++;
            return "error";

        }
        if (right != null) right.CheckType();

        return "";
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
    public override string CheckType()
    {
        string type1 = left.CheckType();
        if (type1 == "error") return "error";

        if (type1 != "bool")
        {
            Console.Write($"If condition cannot be of type {type1}");
            Compiler.errors++;
            return "error";

        }
        if (elseNode != null) elseNode.CheckType();
        if (right != null) right.CheckType();
        return "";
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
    public override string CheckType()
    {

        return "";
    }
    public override void GenCode()
    {
        Compiler.EmitCode($"leave EndMain");
    }
}
