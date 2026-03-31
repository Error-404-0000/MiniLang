fn nothing ConsoleWriteLine(value){
    say value;
}

fn nothing ConsoleWriteHeader(title){
    say "==============";
    say title;
    say "==============";
}

fn nothing ConsoleWriteKeyValue(key, value){
    say "$(key):";
    say value;
}

fn nothing ConsolePause(milliseconds){
    win time Sleep(milliseconds);
}
