
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

    public static List<string> source;
    public static StructTree tree;
    public Dictionary<Variable, string> variables;  

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
        Thread.Sleep(2000);
        return errors==0 ? 0 : 2 ;
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
    public StructTree left;
    public StructTree right;
}

public class Assign : StructTree
{
    public override string CheckType() { return ""; }

    public override string GenCode()
    {
        throw new NotImplementedException();
    }
}
