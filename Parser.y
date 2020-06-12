%namespace GardensPoint

%union
{
public string  val;
public int i_val;
public double  d_val;
public string  type;
}

%token Comment String DoubleConv IntConv Write Program If Else While Read Int Double Bool True False Return And BitAnd Or BitOr Negation BitNegation  Equal NotEqual GreaterOrEqual SmallerOrEqual Smaller Greater Negation BitNegation  Assign Plus Minus Multiply Divide OpenPar ClosePar SemiCol OpenBracket CloseBracket Endl Eof Error
%token <val> Ident IntNumber RealNumber

%type <type> maincandeclare main declare vtype assign exp exp exp2 exp3 exp4 exp5 exp6  logicop addop unary relatiop mulop bitop term read while whilebody 

%%

start : Program OpenBracket maincandeclare CloseBracket {Console.WriteLine("It's a program PogChamp");};

read: Read Ident  {Console.WriteLine("read ident");};

write: Write exp {Console.WriteLine("write ident");}
	| Write String {Console.WriteLine("write string");};

maincandeclare: declare maincandeclare
	| main;

	
expression: assign 
	| read 
	| write;

main:	expression expression {Console.WriteLine("Syntax error"); } SemiCol  main 
	| expression SemiCol main
	| Comment {Console.WriteLine("It's a comment");} main
	| while main
	| error SemiCol  {Console.WriteLine("Error: in line {0}", $1.i_val);} main
	| ;

while: While OpenPar exp ClosePar OpenBracket main CloseBracket {Console.WriteLine("While long");}
	| While OpenPar exp ClosePar whilebody {Console.WriteLine("While short");};

whilebody: assign SemiCol {Console.WriteLine("Single operation while body - assign");}
	| exp SemiCol {Console.WriteLine("Single operation while body - exp");}
	| while {Console.WriteLine("Single operation while body - while");}
	| read SemiCol {Console.WriteLine("Single operation while body - read");}
	| write SemiCol {Console.WriteLine("Single operation while body - write");};

declare : vtype Ident SemiCol {Console.WriteLine("It's a declaration");};

vtype: Int {Console.WriteLine("Int");}
	| Double {Console.WriteLine("Double");};

assign: Ident Assign exp {Console.WriteLine("It's an assignment");};


exp: exp2 logicop exp  {Console.WriteLine("Logicop");}
	| exp2; 

exp2: exp3 relatiop exp2  {Console.WriteLine("Relatiop");}
	| exp3;

exp3: exp4 addop exp3  {Console.WriteLine("Addop");}
	| exp4;

exp4: exp5 mulop exp4  {Console.WriteLine("Mulop");}
	| exp5;

exp5: exp6 bitop exp5  {Console.WriteLine("Bitop");}
	| exp6;

exp6: unary exp6  {Console.WriteLine("Unary op");}
	| term  {//Console.WriteLine("Ident");
	};

term: Ident {Console.WriteLine("Ident");}
	| RealNumber {Console.WriteLine("Real Number");}
	| IntNumber {Console.WriteLine("Int number");}
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