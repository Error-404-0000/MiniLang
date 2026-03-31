fn nothing AppPrintBanner(name){
    say "========================================";
    say name;
    say "========================================";
}

fn nothing AppPrintStep(label){
    say "-> $(label)";
}

fn nothing AppPrintKeyValue(key, value){
    say "$(key):";
    say value;
}

fn nothing AppPrintDivider(){
    say "----------------------------------------";
}
