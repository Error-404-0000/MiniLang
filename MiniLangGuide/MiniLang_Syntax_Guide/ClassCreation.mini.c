
struct FullName{
    public FirstName -> string;
    public LastName -> string;
    fn string FullNameToString(){
        give "$(FirstName) $(LastName)";
    }
}
struct Country{
    public Country -> string;
    public State -> string;
    public ZipCode-> number;

    fn string CountryToString():
        give "$(Country) $(State) $(ZipCode)";
    done

}
struct DayMonthYear{
    public Day->number;
    public Month -> number;
    public Year -> number;
    fn string DayMonthYearToString():
        give "$(Day)/$(Month)/$(Year)";
    done
}
struct Time{
    public Min->number;
    public Hours->number;
    public Seconds->number;


     fn string TimeToString(){
        
        make h = "$(Hours)";
        if(Hours < 10){
            h = "0$(h)";
        }

        
        make m = "$(Min)";
        if(Min < 10){
            m = "0$(m)";
        }

        
        make s = "$(Seconds)";
        if(Seconds < 10){
            s = "0$(s)";
        }

        give "$(h):$(m):$(s)";
    }
}
struct DateTime {
    public Day->DayMonthYear;
    public Time -> Time;

    fn string DateTimeToString(day,time,TimeStruct):
        make dayinfo =  "$(day)  $(time)";
        if(TimeStruct.Hours >= 12):
           dayinfo +=" PM";
        else
            dayinfo +=" AM";
        done

        give dayinfo;
    done
    
}


struct User{
    public Name ->FullName;
    public Country->Country;
    public JoinedDate->DateTime;

 
}

make user = new User;


user.Name.FirstName = "hello world";
user.Name.LastName = "tester";

user.Country.Country = "USA";
user.Country.State = "CA";
user.Country.ZipCode = 90210;

user.JoinedDate.Day.Day = 27;
user.JoinedDate.Day.Month = 11;
user.JoinedDate.Day.Year = 2025;

user.JoinedDate.Time.Hours = 9;
user.JoinedDate.Time.Min = 32;
user.JoinedDate.Time.Seconds = 12;




fn nothing wait(number1){
    
    make outer = number1 * 100;

    while(outer > 0):
        make inner = 6000;
        while(inner > 0):
            inner -= 1;
        done
        outer -= 1;
    done
}




fn nothing InCressTime(){
    
    user.JoinedDate.Time.Seconds += 1;

    
    if(user.JoinedDate.Time.Seconds >= 60){
        user.JoinedDate.Time.Seconds = 0;
        user.JoinedDate.Time.Min += 1;
    }

    
    if(user.JoinedDate.Time.Min >= 60){
        user.JoinedDate.Time.Min = 0;
        user.JoinedDate.Time.Hours += 1;
    }
}






while(1==1){
    InCressTime();
    make DateToString = user.JoinedDate.Time();
    say DateToString;
    wait(1);
}











