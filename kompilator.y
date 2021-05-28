%namespace GardensPoint

%union
{
public string  val;
public int i_val;
public string  type;
}

%token Comment DoubleConv IntConv Write Program If Else While Read Int Double Bool True False Return And BitAnd Or BitOr Negation BitNegation  Equal NotEqual GreaterOrEqual SmallerOrEqual Smaller Greater Negation BitNegation  Assign Plus Minus Multiply Divide OpenPar ClosePar OpenBracket CloseBracket Endl Eof Error
%token <val> Ident IntNumber RealNumber  String
%token <i_val> SemiCol
%type <type> identPar maincandeclare main declare vtype assign  exp exp2 exp3 exp4 exp5 exp6  logicop addop unary relatiop mulop bitop term read while whilebody if
%type <i_val> semicol
%%

start : Program OpenBracket maincandeclare CloseBracket Eof {if(Compiler.stackTree.Count>0) Compiler.tree = Compiler.stackTree.Pop(); YYACCEPT;}
	| Program Eof  {Console.WriteLine("Unexpected end of file");  ++Compiler.errors; YYABORT;}
	| error{Console.WriteLine("Critical error while parsing");  ++Compiler.errors; YYABORT;} start ;

if: If  OpenPar exp ClosePar ifbody { var nodeI = new IfNode();
				    Compiler.stackTree.Push(nodeI);}
	| If  OpenPar exp ClosePar ifbody Else ifbody { var nodeI =new IfElseNode();
				    Compiler.stackTree.Push(nodeI);};

semicol : SemiCol {Compiler.line=$1;
			$$=$1;};

identPar: Ident {$$=$1;}
	| OpenPar identPar ClosePar {$$ =$2;};

read: Read identPar { 
					var nodeR = new ReadNode($2);
				    Compiler.stackTree.Push(nodeR);};
write: Write exp{ var nodeW = new WriteNode();
				  if(Compiler.stackTree.Count>0) nodeW.right = Compiler.stackTree.Pop();
				  Compiler.stackTree.Push(nodeW);}

		| Write String {
					var nodeS = new StringNode($2);
					var nodeW = new WriteNode();
					nodeW.right=nodeS;
				    Compiler.stackTree.Push(nodeW);};

maincandeclare: declare maincandeclare
				 {  
				    var nodeM = new MainNode();
				    Compiler.stackTree.Push(nodeM);
				}
	| main;

	
expression: read 
	| write;

main:	expression expression {Console.WriteLine("Syntax error");  ++Compiler.errors;
               yyerrok();
                } semicol  main 
	| expression semicol main
				{  
				    var nodeM = new MainNode();
				    Compiler.stackTree.Push(nodeM);
				}
	| assign semicol main {  
				    var nodeM = new MainNode();
				    Compiler.stackTree.Push(nodeM);
				}

	| exp1 semicol main {  
				    var node = new StandaloneExpNode();
				    Compiler.stackTree.Push(node);
				}
	| while main	{  
				    var nodeM = new MainNode();
				    Compiler.stackTree.Push(nodeM);
				}
	| if main 	{  
				    var nodeM = new MainNode();
				    Compiler.stackTree.Push(nodeM);
				}
	| OpenBracket main CloseBracket main 
				{  
				    var nodeM = new MainNode();
				    Compiler.stackTree.Push(nodeM);
				}
	| error semicol  {Console.WriteLine("Error while parsing (syntax or lexical error): around line {0} ",Compiler.line);   ++Compiler.errors;
				yyerrok();
	            } main
	| error {Console.WriteLine("Error while parsing (syntax or lexical error) around line {0}",Compiler.line);   ++Compiler.errors;
				yyerrok();
              } main
	| error Eof {Console.WriteLine("Unexpected end of file");  ++Compiler.errors; YYABORT;}
	| Return semicol main { var nodeR = new ReturnNode();
					Compiler.stackTree.Push(nodeR);
					var nodeM = new MainNode();
				    Compiler.stackTree.Push(nodeM);}
	|  { var nodeM = new MainNode(true);
		 Compiler.stackTree.Push(nodeM);};


while: While OpenPar exp ClosePar OpenBracket main CloseBracket {var nodeW = new WhileNode();
					Compiler.stackTree.Push(nodeW);
					}
	| While OpenPar exp ClosePar whilebody  {var nodeW = new WhileNode();
					Compiler.stackTree.Push(nodeW);
					};

whilebody: expression semicol 	
	| exp1 semicol
	| assign semicol
	| while 
	| if  
	| Return semicol { var nodeR = new ReturnNode();
		 Compiler.stackTree.Push(nodeR);};

ifbody: OpenBracket main CloseBracket 
	| expression semicol
	| exp1 semicol
	| assign semicol
	| while 
	| if
	| Return semicol { var nodeR = new ReturnNode();
		Compiler.stackTree.Push(nodeR);};

declare : vtype Ident semicol {var node = new DeclarationNode(); 
						  node.varType=Compiler.GetValType($1); node.ident=$2;
						  if(Compiler.variables.ContainsKey($2)) 
						  {	
							string s=$2;
							Console.WriteLine("Semantic error. Variable {0} already declared. Error in line {1}",s,Compiler.line);
							++Compiler.errors;
						  }
						  else
							 Compiler.variables.Add($2,Compiler.GetValType($1));
						  Compiler.stackTree.Push(node);};

vtype: Int {$$="int";}
	| Double {$$="double";}
	| Bool {$$ = "bool";};

assign: Ident Assign exp  {var node = new AssignNode($1); 			
						  Compiler.stackTree.Push(node);};

assignMid: Ident Assign exp  {var node = new AssignMidNode($1); 
						  Compiler.stackTree.Push(node);};

exp: exp1
	| assignMid;

exp1: exp1 logicop exp2  {var node = new LogicNode(); 			
					node.type = Compiler.GetLogicType($2);
					Compiler.stackTree.Push(node);}
	| exp1 logicop OpenPar exp ClosePar  {var node = new LogicNode(); 
					node.type = Compiler.GetLogicType($2);
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar
	| exp2;

exp2: exp2 relatiop exp3
		{
		 var node = new RelationNode(); 
					node.type = Compiler.GetRelationType($2);
					Compiler.stackTree.Push(node);}
	|  OpenPar exp ClosePar relatiop exp3  
		 {var node = new RelationNode(); 
					node.type = Compiler.GetRelationType($4);
					Compiler.stackTree.Push(node);}
	|  OpenPar exp ClosePar relatiop OpenPar exp ClosePar 
		 {var node = new RelationNode(); 
					node.type = Compiler.GetRelationType($4);
					Compiler.stackTree.Push(node);}
	| exp2 relatiop OpenPar exp ClosePar 
		 {var node = new RelationNode(); 
					node.type = Compiler.GetRelationType($2);
					Compiler.stackTree.Push(node);}
	| exp3;

exp3: exp3 addop exp4 {var node = new AddNode(); 
					node.type = Compiler.GetAddType($2);
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar addop exp4 {var node = new AddNode(); 
					node.type = Compiler.GetAddType($4);
					Compiler.stackTree.Push(node);}
	| exp3 addop OpenPar exp ClosePar {var node = new AddNode(); 
					node.type = Compiler.GetAddType($2);
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar addop OpenPar exp ClosePar
					{var node = new AddNode(); 
					node.type = Compiler.GetAddType($4);
					Compiler.stackTree.Push(node);}
	| exp4;

exp4: exp4 mulop exp5  {var node = new MulNode(); 
					node.type = Compiler.GetMulType($2);
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar mulop OpenPar exp ClosePar  {var node = new MulNode(); 
					node.type = Compiler.GetMulType($4);
					Compiler.stackTree.Push(node);}
	| OpenPar exp ClosePar mulop exp5  {var node = new MulNode(); 
					node.type = Compiler.GetMulType($4);
					Compiler.stackTree.Push(node);}
	| exp4 mulop OpenPar exp ClosePar  {var node = new MulNode(); 
					node.type = Compiler.GetMulType($2);
					Compiler.stackTree.Push(node);}
	| exp5;

exp5: exp5 bitop exp6  {var node = new BitNode(); 
					node.type = Compiler.GetBitType($2);
					Compiler.stackTree.Push(node);}
	|  OpenPar exp ClosePar bitop exp6  {var node = new BitNode(); 
					node.type = Compiler.GetBitType($4);
					Compiler.stackTree.Push(node);}
	|  exp5 bitop  OpenPar exp ClosePar  {var node = new BitNode(); 
					node.type = Compiler.GetBitType($2);
					Compiler.stackTree.Push(node);}
	|   OpenPar exp ClosePar bitop  OpenPar exp ClosePar {var node = new BitNode(); 
					node.type = Compiler.GetBitType($4);
					Compiler.stackTree.Push(node);}
	| exp6;

exp6: unary exp6   {var node = new UnaryNode(); 
					node.type = Compiler.GetUnaryType($1);
					Compiler.stackTree.Push(node);}
	| unary OpenPar exp ClosePar {var node = new UnaryNode(); 
					node.type = Compiler.GetUnaryType($1);
					Compiler.stackTree.Push(node);}
	| term ;

term: Ident {var node = new IdentNode();
			node.ident = $1;
			Compiler.stackTree.Push(node);}
	| RealNumber {var node = new DoubleNode();	
					node.value=Double.Parse($1,System.Globalization.CultureInfo.InvariantCulture);
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
bitop: BitOr {$$="|";}
	| BitAnd   {$$="&";} ;

unary: Minus  {$$="-";}
	| BitNegation  {$$="~";}
	| Negation  {$$="!";}
	| OpenPar Int ClosePar  {$$="(int)";}
	| OpenPar Double ClosePar  {$$="(double)";};

logicop: Or  {$$="||";}
	| And  {$$="&&";};

relatiop: Equal  {$$="==";}
	| NotEqual  {$$="!=";}
	| Greater {$$=">";}
	| Smaller  {$$="<";}
	| GreaterOrEqual {$$=">=";}
	| SmallerOrEqual  {$$="<=";}
	;

%%

public Parser(Scanner scanner) : base(scanner) { }