﻿%namespace GardensPoint

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

start : Program OpenBracket maincandeclare CloseBracket Eof { Compiler.tree = Compiler.stackTree.Pop();}
	error Eof;

if: If  OpenPar exp ClosePar ifbody { var nodeI = new IfNode();
					if(Compiler.stackTree.Count>0) nodeI.right = Compiler.stackTree.Pop();
				    if(Compiler.stackTree.Count>0) nodeI.left = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeI);}
	| If  OpenPar exp ClosePar ifbody Else ifbody { var nodeI =new IfElseNode();
					if(Compiler.stackTree.Count>0) nodeI.elseNode = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) nodeI.right = Compiler.stackTree.Pop();
				    if(Compiler.stackTree.Count>0) nodeI.left = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeI);};

ifbody: OpenBracket main CloseBracket 
	| expression SemiCol 
	| while 
	| if;

read: Read Ident  { var nodeR = new WriteNode();
					if(Compiler.stackTree.Count>0) nodeR.right = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeR);};

write: Write exp{ var nodeW = new WriteNode();
					if(Compiler.stackTree.Count>0) nodeW.right = Compiler.stackTree.Pop();
					    Compiler.stackTree.Push(nodeW);}
	| Write OpenPar exp ClosePar { Console.WriteLine("write");var nodeW = new WriteNode();
					if(Compiler.stackTree.Count>0) nodeW.right = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeW);}
	| Write String {
					var nodeS = new StringNode();
					nodeS.value = $2;
					var nodeW = new WriteNode();
					nodeW.right=nodeS;
				    Compiler.stackTree.Push(nodeW);};

maincandeclare: declare maincandeclare
				 {  
				    var nodeM = new MainNode();
					if(Compiler.stackTree.Count>0) nodeM.right = Compiler.stackTree.Pop();
				    if(Compiler.stackTree.Count>0) nodeM.left = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeM);
				}
	| main {};

	
expression: read 
	| write
	| assign;

main:	expression expression {Console.WriteLine("Syntax error");  ++Compiler.errors;
               yyerrok();
                } SemiCol  main 
	| expression SemiCol main
				{  
				    var nodeM = new MainNode();
					if(Compiler.stackTree.Count>0) nodeM.right = Compiler.stackTree.Pop();
				    if(Compiler.stackTree.Count>0) nodeM.left = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeM);
				}
	| exp SemiCol main {  
				    var nodeM = new MainNode();
					if(Compiler.stackTree.Count>0) nodeM.right = Compiler.stackTree.Pop();
				    if(Compiler.stackTree.Count>0) nodeM.left = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeM);
				}
	| while main	{  
				    var nodeM = new MainNode();
					if(Compiler.stackTree.Count>0) nodeM.right = Compiler.stackTree.Pop();
				    if(Compiler.stackTree.Count>0) nodeM.left = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeM);
				}
	| if main 	{  
				    var nodeM = new MainNode();
					if(Compiler.stackTree.Count>0) nodeM.right = Compiler.stackTree.Pop();
				    if(Compiler.stackTree.Count>0) nodeM.left = Compiler.stackTree.Pop();
				    Compiler.stackTree.Push(nodeM);
				}
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

while: While OpenPar exp ClosePar OpenBracket main CloseBracket {var nodeW = new WhileNode();
					if(Compiler.stackTree.Count>0) nodeW.right = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) nodeW.left = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(nodeW);
					}
	| While OpenPar exp ClosePar whilebody  {var nodeW = new WhileNode();
					if(Compiler.stackTree.Count>0) nodeW.right = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) nodeW.left = Compiler.stackTree.Pop();
					Compiler.stackTree.Push(nodeW);
					};

whilebody: expression SemiCol 	
	| while 
	| if  ;

declare : vtype Ident SemiCol {var node = new DeclarationNode(); 
						  node.varType=$1; node.ident=$2;
						  if(Compiler.variables.ContainsKey($2)) 
						  {	
							string s=$2;
							Console.WriteLine("Semantic error. Variable {0} already declared.",s);
							++Compiler.errors;
						  }
						  else
							 Compiler.variables.Add($2,$1);
						  Compiler.stackTree.Push(node);};

vtype: Int {$$="int";}
	| Double {$$="double";}
	| Bool {$$ = "bool";};

assign: Ident Assign exp  {var node = new AssignNode(); 
						   node.ident=$1;
						   	if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
						  Compiler.stackTree.Push(node);};


exp: exp2 logicop exp  {var node = new LogicNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $2;
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar  logicop exp  {var node = new LogicNode(); 
					  if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $4;
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar  logicop OpenPar exp ClosePar 
					{var node = new LogicNode(); 
					if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $4;
					Compiler.stackTree.Push(node);}
	| exp2  logicop OpenPar exp ClosePar 
					{var node = new LogicNode(); 
					  if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $2;
					Compiler.stackTree.Push(node);}
	| exp2;

exp2: exp3 relatiop exp2
		{
		 var node = new RelationNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $2;
					Compiler.stackTree.Push(node);}
	|  OpenPar exp ClosePar relatiop exp2  
		 {var node = new RelationNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $4;
					Compiler.stackTree.Push(node);}
	|  OpenPar exp ClosePar relatiop OpenPar exp ClosePar 
		 {var node = new RelationNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $4;
					Compiler.stackTree.Push(node);}
	| exp3 relatiop OpenPar exp ClosePar 
		 {var node = new RelationNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $2;
					Compiler.stackTree.Push(node);}
	| exp3;

exp3: exp4 addop exp3 {var node = new AddNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $2;
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar addop exp3 {var node = new AddNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $4;
					Compiler.stackTree.Push(node);}
	| exp4 addop OpenPar exp ClosePar {var node = new AddNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $2;
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar addop OpenPar exp ClosePar
					{var node = new AddNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $4;
					Compiler.stackTree.Push(node);}
	| exp4;

exp4: exp5 mulop exp4  {var node = new MulNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $2;
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar mulop OpenPar exp ClosePar  {var node = new MulNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $4;
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar mulop exp4  {var node = new MulNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $4;
					Compiler.stackTree.Push(node);}
	| exp5 mulop OpenPar exp ClosePar  {var node = new MulNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $2;
					Compiler.stackTree.Push(node);}
	| exp5;

exp5: exp6 bitop exp5  {var node = new BitNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
					node.type = $2;
					Compiler.stackTree.Push(node);}
	|  OpenPar exp ClosePar bitop exp5  {var node = new BitNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
						node.type = $4;
					Compiler.stackTree.Push(node);}
	|  exp6 bitop  OpenPar exp ClosePar  {var node = new BitNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
						node.type = $2;
					Compiler.stackTree.Push(node);}
	|   OpenPar exp ClosePar bitop  OpenPar exp ClosePar {var node = new BitNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					if(Compiler.stackTree.Count>0) node.right = Compiler.stackTree.Pop();
						node.type = $4;
					Compiler.stackTree.Push(node);}
	| exp6;

exp6: unary exp6   {var node = new UnaryNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					node.type=$1;
					Compiler.stackTree.Push(node);}
	| unary OpenPar exp ClosePar {var node = new UnaryNode(); 
				    if(Compiler.stackTree.Count>0) node.left = Compiler.stackTree.Pop();
					node.type=$1;
					Compiler.stackTree.Push(node);}
	| term ;

term: Ident {var node = new IdentNode();
			node.ident = $1;
			Compiler.stackTree.Push(node);}
	| RealNumber {var node = new DoubleNode();
					node.value=Double.Parse($1);
						  Compiler.stackTree.Push(node);}
	| IntNumber {var node = new IntNode();
					node.value=Int32.Parse($1);
						  Compiler.stackTree.Push(node);}
	| True {var node = new BoolNode();
					node.value=true;
						  Compiler.stackTree.Push(node);}
	| False {var node = new BoolNode();
					node.value=false;
						  Compiler.stackTree.Push(node);};

addop: Plus  {$$="+";}
	| Minus  {$$="-";};

mulop: Multiply  {$$="*";}
	
	| Divide {$$="/";};
bitop: BitOr {$$="||";}
	| BitAnd   {$$="&&";} ;

unary: Minus  {$$="-";}
	| BitNegation  {$$="~";}
	| Negation  {$$="!";}
	| IntConv  {$$="(int)";}
	| DoubleConv  {$$="(double)";};

logicop: Or
	| And;

relatiop: Equal  {$$="==";}
	| NotEqual  {$$="!=";}
	| Greater {$$=">";}
	| Smaller  {$$="<";}
	| GreaterOrEqual {$$=">=";}
	| SmallerOrEqual  {$$="<=";}
	;

%%

public Parser(Scanner scanner) : base(scanner) { }