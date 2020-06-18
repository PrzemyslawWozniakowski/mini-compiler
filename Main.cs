
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
    public static Dictionary<string,Variable> variables;
    public static Stack<StructTree> stackTree = new Stack<StructTree>();
    // arg[0] określa plik źródłowy
    // pozostałe argumenty są ignorowane
    public static int Main(string[] args)
        {
        string file="";
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
            var sr=new StreamReader(file);
            string str=sr.ReadToEnd();
            sr.Close();
            Compiler.source=new System.Collections.Generic.List<string>(str.Split(new string[]{"\r\n"},System.StringSplitOptions.None));
            source = new FileStream(file,FileMode.Open);
            }
        catch ( Exception e)
            {
            Console.WriteLine("\n"+e.Message);
            return 1;
            }
        Scanner scanner = new Scanner(source);
        Parser parser = new Parser(scanner);
        Console.WriteLine();

        sw = new StreamWriter(file+".il");
        GenProlog();
        parser.Parse();
        StructTree currentnode = tree;

        CheckTypeTree(currentnode);
        GenEpilog();
        sw.Close();
        source.Close();
        if ( errors==0 )
            Console.WriteLine("  compilation successful\n");
        else
            {
            Console.WriteLine($"\n  {errors} errors detected\n");
            File.Delete(file+".il");
            }
        Thread.Sleep(4000);
        return errors==0 ? 0 : 2 ;
        }

    public static void CheckTypeTree(StructTree node)
    {

        if (node.left != null) CheckTypeTree(node.left);
         node.CheckType();
        if (node.right != null) CheckTypeTree(node.right);
    }
    public static void EmitCode(string instr=null)
        {
        sw.WriteLine(instr);
        }

    public static void EmitCode(string instr, params object[] args)
        {
        sw.WriteLine(instr,args);
        }

    private static StreamWriter sw;

    private static void GenProlog()
        {
        EmitCode(".assembly extern mscorlib { }");
        EmitCode(".assembly calculator { }");
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
    public abstract string GenCode();
    public StructTree left=null;
    public StructTree right=null;
   
}

public class MainNode: StructTree
{
    public override string CheckType() { return ""; }

    public override string GenCode()
    {

        return "";
    }
}

public class DeclarationNode: StructTree
{
    public override string CheckType() { Console.WriteLine($"Declaration of {ident}"); return ""; }
    public string varType;
    public string ident;
    public bool isValid() { return Compiler.variables.ContainsKey(ident); }
    public override string GenCode()
    {
        return "";
    }
}

public class AssignNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Assign for {ident}"); return ""; }
    public string ident;
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class LogicNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Logicop"); return ""; }
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class RelationNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Relation op"); return ""; }
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class AddNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Add"); return ""; }
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class MulNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Mul"); return ""; }
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class BitNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Bit"); return ""; }
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class UnaryNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Unary"); return ""; }
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class IdentNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Value"); return ""; }
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class StringNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"String"); return ""; }
    public string value;
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class WriteNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Write"); return ""; }
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}

public class ReadNode : StructTree
{
    public override string CheckType() { Console.WriteLine($"Read"); return ""; }
    public string value;
    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}