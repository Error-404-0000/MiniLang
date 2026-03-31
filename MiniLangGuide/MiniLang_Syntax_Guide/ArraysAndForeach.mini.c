fn array BuildValues(){
    give [5, 10, 15];
}

fn number Main(){
    make values = BuildValues();
    Push(values, 20);
    values[0] = 1;

    foreach item in values:
        say item;
    done

    say Length(values);
    say Contains(values, 10);
    give 0;
}
