
using System;
using System.IO;
using System.Collections.Generic;
using GardensPoint;
using System.Threading;
using SymTab;
using QUT.Gppg;

public class Compiler
{

    public static int errors = 0;
    public static int line = 1;
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
        string type1="", type2="";
        if (left != null) type1 = left.CheckType();
        if (right != null) type2 = right.CheckType();
        if (type1 == "error" || type2 == "error") Console.WriteLine($" Error in line {line}");
        return "";
    }

    public override void GenCode()
    {
       if(left!=null) left.GenCode();
       if(right!=null) right.GenCode();

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
        if (varType =="int")
            s = s + "( int32 ";
        if (varType == "double")
            s = s + "( float64 ";
        if (varType == "bool")
            s = s + "( bool ";

        s=s + $"{ident} )";

        Compiler.EmitCode(s);

        s = $"ldc.";
        if (varType == "int")
            s = s + $"i4 0";
        if (varType == "double")
            s = s + $"r8 0.0";
        if (varType == "bool")
            s = s + "i4.0";

        Compiler.EmitCode(s);
         s = $"stloc {ident}";
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
        string s = $"stloc {ident}";
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
        right.GenCode();
        left.GenCode();
        string s = "add";
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
        Compiler.EmitCode($"ldloc {ident}");
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
    }
}


public class StringNode : StructTree
{
    public override string CheckType() { return "string"; }
    public string value;
    public override void GenCode()
    {
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
        string s = "ldstr \" {0} \"";
        Compiler.EmitCode(s);
        if (right != null) right.GenCode();
        if (left != null) left.GenCode();
        if (right != null && right.CheckType() == "double")
            s = "box[mscorlib]System.Double";
        Compiler.EmitCode(s);
        s = "call void [mscorlib]System.Console::WriteLine(string, object)";
        Compiler.EmitCode(s);
        
    }
}

public class ReadNode : StructTree
{
    public override string CheckType()
    {
        if (right != null) right.CheckType();
        if (left != null) left.CheckType();
        return "";
    }
    public string value;
    public override void GenCode()
    {
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
            Console.Write($"While condition cannot be of type {type1}");
            Compiler.errors++;
            return "error";

        }
        if (right != null) right.CheckType();

        return "";
    }
    public string value;
    public override void GenCode()
    {
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
    }
}
