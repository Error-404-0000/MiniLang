fn array BuildReleaseNumbers(){
    give [2, 4, 6];
}

fn array BuildEmptyBuffer(){
    give [];
}

fn number SumValues(array values){
    make total = 0;
    foreach item in values:
        total = total + item;
    done
    give total;
}

fn number CountValues(array values){
    give Length(values);
}

fn number HasValue(array values, value){
    give Contains(values, value);
}
