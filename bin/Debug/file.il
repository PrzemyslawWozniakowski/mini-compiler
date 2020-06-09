.assembly extern mscorlib { }
.assembly calculator { }
.method static void main()
{
.entrypoint
.try
{

// prolog
.locals init ( float64 temp )
.locals init ( int32 _ia )
.locals init ( float64 _ra )
.locals init ( int32 _ib )
.locals init ( float64 _rb )
.locals init ( int32 _ic )
.locals init ( float64 _rc )
.locals init ( int32 _id )
.locals init ( float64 _rd )
.locals init ( int32 _ie )
.locals init ( float64 _re )
.locals init ( int32 _if )
.locals init ( float64 _rf )
.locals init ( int32 _ig )
.locals init ( float64 _rg )
.locals init ( int32 _ih )
.locals init ( float64 _rh )
.locals init ( int32 _ii )
.locals init ( float64 _ri )
.locals init ( int32 _ij )
.locals init ( float64 _rj )
.locals init ( int32 _ik )
.locals init ( float64 _rk )
.locals init ( int32 _il )
.locals init ( float64 _rl )
.locals init ( int32 _im )
.locals init ( float64 _rm )
.locals init ( int32 _in )
.locals init ( float64 _rn )
.locals init ( int32 _io )
.locals init ( float64 _ro )
.locals init ( int32 _ip )
.locals init ( float64 _rp )
.locals init ( int32 _iq )
.locals init ( float64 _rq )
.locals init ( int32 _ir )
.locals init ( float64 _rr )
.locals init ( int32 _is )
.locals init ( float64 _rs )
.locals init ( int32 _it )
.locals init ( float64 _rt )
.locals init ( int32 _iu )
.locals init ( float64 _ru )
.locals init ( int32 _iv )
.locals init ( float64 _rv )
.locals init ( int32 _iw )
.locals init ( float64 _rw )
.locals init ( int32 _ix )
.locals init ( float64 _rx )
.locals init ( int32 _iy )
.locals init ( float64 _ry )
.locals init ( int32 _iz )
.locals init ( float64 _rz )

// linia   1 :  print 4.3+6.3
ldstr "  Result: {0}{1}"
ldc.r8 4.3
ldc.r8 6.3
add
box [mscorlib]System.Double
ldstr "r"
call void [mscorlib]System.Console::WriteLine(string,object,object)

// linia   2 :  exit
ldstr "\nEnd of execution\n"
call void [mscorlib]System.Console::WriteLine(string)

leave EndMain
}
catch [mscorlib]System.Exception
{
callvirt instance string [mscorlib]System.Exception::get_Message()
call void [mscorlib]System.Console::WriteLine(string)
leave EndMain
}
EndMain: ret
}
