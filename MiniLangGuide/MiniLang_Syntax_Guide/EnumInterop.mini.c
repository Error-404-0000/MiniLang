enum Tone {
    Warm;
    Cool;
    Accent;
}

fn nothing ShowTone(Tone selected){
    if(selected == Tone.Warm):
        say "Warm tone selected";
    else
        say "Other tone selected";
    done
}

fn nothing RunInterop(){
    win console SetTitle("MiniLang Legacy Runtime");
    say "Process:";
    say win process GetCurrentProcessId();
    say "Ticks:";
    say win time GetTickCount();
}

fn nothing PreviewDialog(){
    win user MessageBox("MiniLang", "Legacy runtime interop is active");
}

fn nothing Main(){
    ShowTone(Tone.Warm);
    RunInterop();
}
