use "MiniLangLibraries/Core/Application.mini.c";
use "MiniLangLibraries/Console/Console.mini.c";
use "MiniLangLibraries/IO/FileSystem.mini.c";
use "MiniLangLibraries/Windows/Window.mini.c";
fn string ToString(value) : give "$(value)"; done 


    make stringValue = ToArray("string-string2-string3");
    make splitStrings = [];
   

        make tempstring = "";
        make count = 0;
        make len =  Length(stringValue);
    

        while(count != len){
          make vx = stringValue[count];
            if(vx != "-"){
                tempstring+=ToString(stringValue[count]);
            else
                Push(splitStrings, tempstring);
                tempstring = "";
            }
            count+=1;
        }
        if(tempstring != "") :  Push(splitStrings, tempstring); done


foreach item in splitStrings :
    say item;
done
    


