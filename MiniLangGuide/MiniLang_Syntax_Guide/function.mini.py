fn nothing Print(string){
    say string;
}
fn nothing print(string){
    Print(string);
}
fn nothing print(strin1,string2){
    say strin1 + string2;
}

fn string isOdd(number){
    if(number % 2 == 0):
        give "false";
    else
        give "true";
    done
}

fn object isEven(number){
    if(number % 2 == 0):
        give "true";
    else
        give "false";
    done
}
fn object isGrterThan(number, compareTo){
    if(number > compareTo):
        give "true";
    else
        give "false";
    done
}

fn object isLessThan(number, compareTo):
    if(number < compareTo):
        give "true";
    else
        give "false";
    done
done
