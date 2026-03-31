fn nothing SetWindowTitle(){
    win console SetTitle("MiniLang Window");
}

fn nothing ShowIdentity(){
    say "Process id:";
    say win process GetCurrentProcessId();
}

fn nothing PauseForMoment(){
    say "Sleeping for 250ms";
    win time Sleep(250);
    say "Awake again";
}

fn nothing AlertUser(){
    win user MessageBox("MiniLang", "Interop bridge reached the Windows API safely");
}

fn nothing Main(){
    SetWindowTitle();
    ShowIdentity();
    PauseForMoment();
    AlertUser();
}
