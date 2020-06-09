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

%type <type> main declare vtype assign exp exp exp2 exp3 exp4 exp5 exp6  logicop addop unary relatiop mulop bitop

%%

start : Program OpenBracket main CloseBracket {Console.WriteLine("It's a program PogChamp");};

main: declare main
	| assign main
	| ;

declare : vtype Ident SemiCol {Console.WriteLine("It's a declaration");};

vtype: Int 
	| Double;

assign: Ident Assign exp SemiCol {Console.WriteLine("It's an assignment");};


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
	| Ident  {//Console.WriteLine("Ident");
	};

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