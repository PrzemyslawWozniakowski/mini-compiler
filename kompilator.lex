
%using QUT.Gppg;
%namespace GardensPoint
%{
static int linecount=1;
%}
alpha      [a-zA-Z]
digit      [0-9]
alnum      {alpha}|{digit}

IntNumber   ([1-9]{digit}*)|0
RealNumber  ({digit}\.[0-9]+)|([1-9]{digit}*\.[0-9]+)
Ident       {alpha}{alnum}*

%%

"//".*        { }
\"([^\"\\"\r\n"]|\\.)*\"  {  yylval.val=yytext;	return (int)Tokens.String; } 



"write"       { return (int)Tokens.Write; }
"program"     { return (int)Tokens.Program;}
"if"	      { return (int)Tokens.If;}
"else"	      { return (int)Tokens.Else;}
"while"	      { return (int)Tokens.While;}
"read"	      { return (int)Tokens.Read;}
"int"         { return (int)Tokens.Int;}
"double"      { return (int)Tokens.Double;}  
"bool"        { return (int)Tokens.Bool;}
"true"        { return (int)Tokens.True;}
"false"       { return (int)Tokens.False;}
"return"	  { return (int)Tokens.Return;}

{Ident}       { yylval.val=yytext; return (int)Tokens.Ident; }

{IntNumber}   { yylval.val=yytext; return (int)Tokens.IntNumber; }
{RealNumber}  { yylval.val=yytext; return (int)Tokens.RealNumber; }

"&&"          { return (int)Tokens.And; }
"&"           { return (int)Tokens.BitAnd; }

"||"          { return (int)Tokens.Or; }
"|"           { return (int)Tokens.BitOr; }

"=="            { return (int)Tokens.Equal;}
"!="            { return (int)Tokens.NotEqual;}
">="            { return (int)Tokens.GreaterOrEqual;}
"<="            { return (int)Tokens.SmallerOrEqual;}
"<"             { return (int)Tokens.Smaller;}
">"             { return (int)Tokens.Greater;}

"!"           { return (int)Tokens.Negation; }
"~"           { return (int)Tokens.BitNegation; }

"="           { return (int)Tokens.Assign; }
"+"           { return (int)Tokens.Plus; }
"-"           { return (int)Tokens.Minus; }
"*"           { return (int)Tokens.Multiply; }
"/"           { return (int)Tokens.Divide; }
"("           { return (int)Tokens.OpenPar; }
")"           { return (int)Tokens.ClosePar; }
";"           { yylval.i_val=linecount; return (int)Tokens.SemiCol;}
"{"	          { return (int)Tokens.OpenBracket;}
"}"	          { return (int)Tokens.CloseBracket;}

"\r"          { linecount++;}
<<EOF>>       { return (int)Tokens.Eof; }
" "           { }
"\t"          { }
.             { yylval.i_val=linecount; return (int)Tokens.Error; }