use "MiniLangLibraries/Collections/ArrayTools.mini.c";
use "MiniLangLibraries/Console/Console.mini.c";

fn number Main(){
    make values = BuildReleaseNumbers();
    Push(values, 8);

    ConsoleWriteHeader("Collection workspace sample");
    foreach value in values:
        ConsoleWriteLine(value);
    done

    ConsoleWriteKeyValue("Count", CountValues(values));
    ConsoleWriteKeyValue("Has 4", HasValue(values, 4));
    ConsoleWriteKeyValue("Total", SumValues(values));
    give 0;
}
