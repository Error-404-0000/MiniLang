{

say "Hello world in new scope";
{
	say "Hello world in inner scope";
	{
	
		say "Hello world in inner inner scope";
		{
			make iminvisible = "I am invisible in this scope";
			say "Hello world in inner inner inner scope";
		} 
	} 
}
	
}
:
 say "Hello world in Cool scope";
 : 
		say "Hello world in Cool inner scope";
		{
			make iminvisible = "I am invisible in this scope";
			say "Hello world in Cool inner inner scope";
			say "more cool stuff";

			fn nothing imalsoinvisible(){
				say "I am also invisible in this scope";
			}
		}
 done

done
