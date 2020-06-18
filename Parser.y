%namespace GardensPoint

%union
{
public string  val;
public int i_val;
public double  d_val;
public string  type;
}

%token Comment DoubleConv IntConv Write Program If Else While Read Int Double Bool True False Return And BitAnd Or BitOr Negation BitNegation  Equal NotEqual GreaterOrEqual SmallerOrEqual Smaller Greater Negation BitNegation  Assign Plus Minus Multiply Divide OpenPar ClosePar SemiCol OpenBracket CloseBracket Endl Eof Error
%token <val> Ident IntNumber RealNumber  String

%type <type> maincandeclare main declare vtype assign exp exp exp2 exp3 exp4 exp5 exp6  logicop addop unary relatiop mulop bitop term read while whilebody if 

%%

start : Program OpenBracket maincandeclare CloseBracket Eof {Console.WriteLine("It's a program PogChamp");
	Console.WriteLine("{0}",Compiler.stackTree.Count); Compiler.tree = Compiler.stackTree.Pop();}
	error Eof;

if: If  OpenPar exp ClosePar ifbody {Console.WriteLine("If");} 
	| If  OpenPar exp ClosePar ifbody Else ifbody {Console.WriteLine("If with else");};

ifbody: OpenBracket main CloseBracket {Console.WriteLine("Long if body - exp");}
	| assign SemiCol {Console.WriteLine("Single operation if body - assign");}
	| exp SemiCol {Console.WriteLine("Single operation if body - exp");}
	| while {Console.WriteLine("Single operation if body - while");}
	| if {Console.WriteLine("Single operation if body - if");}
	| read SemiCol {Console.WriteLine("Single operation if body - read");}
	| write SemiCol {Console.WriteLine("Single operation if body - write");};

read: Read Ident  {Console.WriteLine("read ident");};

write: Write exp{ Console.WriteLine("write");var nodeW = new WriteNode();
					if(Compiler.stackTree.Count>0) nodeW.right = Compiler.stackTree.Pop();
					    Compiler.stackTree.Push(nodeW);}
	| Write OpenPar exp ClosePar { Console.WriteLine("write");var nodeW = new WriteNode();
					if(Compiler.stackTree.Count>0) nodeW.right = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeW);}
	| Write String {Console.WriteLine("write");
					var nodeS = new StringNode();
					nodeS.value = $2;
					var nodeW = new WriteNode();
					nodeW.right=nodeS;
				    Compiler.stackTree.Push(nodeW);};

maincandeclare: declare maincandeclare
				 {  
				    var nodeM = new MainNode();
					Console.WriteLine("{0}",Compiler.stackTree.Count);
					if(Compiler.stackTree.Count>0) nodeM.right = Compiler.stackTree.Pop();
				    if(Compiler.stackTree.Count>0) nodeM.left = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeM);
				}
	| main {};

	
expression: assign
	| read 
	| write;	 

main:	expression expression {Console.WriteLine("Syntax error");  ++Compiler.errors;
               yyerrok();
                } SemiCol  main 
	| expression SemiCol main
				{  
				    var nodeM = new MainNode();
					Console.WriteLine("{0}",Compiler.stackTree.Count);
					if(Compiler.stackTree.Count>0) nodeM.right = Compiler.stackTree.Pop();
				    if(Compiler.stackTree.Count>0) nodeM.left = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeM);
				}
	| exp SemiCol main
	| Comment {Console.WriteLine("It's a comment");} main
	| while main
	| if main
	| error SemiCol  {Console.WriteLine("Error: in line {0}", $1.i_val);   ++Compiler.errors;
               yyerrok();
              } main
	| error {Console.WriteLine("Error: in line {0}", $1.i_val);   ++Compiler.errors;
               yyerrok();
              } main
	| Eof {Console.WriteLine("Error");}
	|  { var nodeM = new MainNode();
		 Compiler.stackTree.Push(nodeM);
				};

while: While OpenPar exp ClosePar OpenBracket main CloseBracket {Console.WriteLine("While long");}
	| While OpenPar exp ClosePar whilebody {Console.WriteLine("While short");};

whilebody: assign SemiCol {Console.WriteLine("Single operation while body - assign");}
	| exp SemiCol {Console.WriteLine("Single operation while body - exp");}
	| while {Console.WriteLine("Single operation while body - while");}
	| if {Console.WriteLine("Single operation while body - if");}
	| read SemiCol {Console.WriteLine("Single operation while body - read");}
	| write SemiCol {Console.WriteLine("Single operation while body - write");};

declare : vtype Ident SemiCol {var node = new DeclarationNode(); 
						  node.varType=$1; node.ident=$2;
						  Compiler.stackTree.Push(node);};

vtype: Int {Console.WriteLine("Int");}
	| Double {Console.WriteLine("Double");}
	| Bool;

assign: Ident Assign exp  {var node = new AssignNode(); 
						   node.ident=$1;
						   	if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
						  Compiler.stackTree.Push(node);};


exp: exp2 logicop exp  {var node = new LogicNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar  logicop exp  {var node = new LogicNode(); 
					  if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
						  Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar  logicop OpenPar exp ClosePar 
					{var node = new LogicNode(); 
				  if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
						  Compiler.stackTree.Push(node);}
	| exp2  logicop OpenPar exp ClosePar 
					{var node = new LogicNode(); 
					  if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
						  Compiler.stackTree.Push(node);}
	| exp2; 

exp2: exp3 relatiop exp2
		 {Console.WriteLine("relatiop");
		 var node = new RelationNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	|  OpenPar exp ClosePar relatiop exp2  
		 {var node = new RelationNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	|  OpenPar exp ClosePar relatiop OpenPar exp ClosePar 
		 {var node = new RelationNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| exp3 relatiop OpenPar exp ClosePar 
		 {var node = new RelationNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| exp3;

exp3: exp4 addop exp3 {var node = new AddNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar addop exp3 {var node = new AddNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| exp4 addop OpenPar exp ClosePar {var node = new AddNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar addop OpenPar exp ClosePar
					{var node = new AddNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| exp4;

exp4: exp5 mulop exp4  {var node = new MulNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar mulop OpenPar exp ClosePar  {var node = new MulNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar mulop exp4  {var node = new MulNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| exp5 mulop OpenPar exp ClosePar  {var node = new MulNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| exp5;

exp5: exp6 bitop exp5  {var node = new BitNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	|  OpenPar exp ClosePar bitop exp5  {var node = new BitNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	|  exp6 bitop  OpenPar exp ClosePar  {var node = new BitNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	|   OpenPar exp ClosePar bitop  OpenPar exp ClosePar {var node = new BitNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| exp6;

exp6: unary exp6   {var node = new UnaryNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| unary OpenPar exp ClosePar {var node = new UnaryNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(node);}
	| term  {//Console.WriteLine("Ident");
	};

term: Ident {var node = new IdentNode();
						  Compiler.stackTree.Push(node);}
	| RealNumber {var node = new IdentNode();
						  Compiler.stackTree.Push(node);}
	| IntNumber {var node = new IdentNode();
						  Compiler.stackTree.Push(node);}
	| True {Console.WriteLine(" true");}
	| False {Console.WriteLine(" false");};

addop: Plus  {//Console.WriteLine("Adding");
	}
	| Minus  {//Console.WriteLine("Minus");
	};

mulop: Multiply  {//Console.WriteLine("Multiply");
	}
	| Divide;

bitop: BitOr {//Console.WriteLine("Bit Or");
	}
	| BitAnd  {//Console.WriteLine("Bit And");
	} ;

unary: Minus  {//Console.WriteLine("Unary minus");
	}
	| BitNegation  {//Console.WriteLine("unary bit negation");
	}
	| Negation  {//Console.WriteLine("unary negation");
	}
	| IntConv  {//Console.WriteLine("conv to int");
	}
	| DoubleConv  {//Console.WriteLine("conv to bool");
	};

logicop: Or {//Console.WriteLine("Or");
	}
	| And {//Console.WriteLine("And");
	};

relatiop: Equal {//Console.WriteLine("Equal");
	}
	| NotEqual {//Console.WriteLine("NotEqual");
	}
	| Greater {//Console.WriteLine("Greater");
	}
	| Smaller {//Console.WriteLine("Smaller");
	}
	| GreaterOrEqual {//Console.WriteLine("GreaterOrEqual");
	}
	| SmallerOrEqual {//Console.WriteLine("SmallerOrEqual");
	}
	;

%%

public Parser(Scanner scanner) : base(scanner) { }