fn nothing SetMainWindowTitle(title){
    win console SetTitle(title);
}

fn number ShowDesktopAlert(title, message){
    give win user MessageBox(title, message);
}

fn string GetWindowTitle(){
    give win console GetTitle();
}

fn number GetWindowProcessId(){
    give win process GetCurrentProcessId();
}

fn number GetRuntimeTickCount(){
    give win time GetTickCount();
}

fn nothing SleepFor(milliseconds){
    win time Sleep(milliseconds);
}
