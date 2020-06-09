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

%type <type> main 

%%

start : Program OpenBracket main CloseBracket {Console.WriteLine("It's a program PogChamp");};

main:  ;



%%

public Parser(Scanner scanner) : base(scanner) { }