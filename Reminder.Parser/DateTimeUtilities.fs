namespace ReminderHero.Parser
open System
open Patterns

module DateTimeUtilities = 

    let private _daysOfTheWeek = [|"monday"; "tuesday"; "wednesday"; "thursday"; "friday"; "saturday"; "sunday"|]
    
    // Returns whether or not the given string matches a day of the week 
    let IsDayOfWeek (day:string) = 
        _daysOfTheWeek |> Seq.exists(fun weekday -> weekday.StartsWith(day.ToLower()))
    
    let private _months = Map.empty.Add("jan", 1).Add("feb", 2).Add("mar", 3).Add("apr", 4).Add("may", 5).Add("jun", 6)
                            .Add("jul", 7).Add("aug", 8).Add("sep", 9).Add("oct", 10).Add("nov", 11).Add("dec", 12)

    let IsMonth (monthString:string) = 
        _months|> Seq.exists(fun month-> monthString.StartsWith(month.Key, StringComparison.OrdinalIgnoreCase))
   
    let NumberForMonth (monthString:string) = 
        let m = _months|> Seq.find(fun month-> monthString.StartsWith(month.Key, StringComparison.OrdinalIgnoreCase))
        m.Value

    // Returns the next weekday for the specified datetime
    let GetNextWeekday (start:DateTime, day:DayOfWeek) =
        let adjustedStartDay = start.AddDays(1.0)
        let daysToAdd = (((int)day - (int)adjustedStartDay.DayOfWeek + 7) % 7) |> float
        adjustedStartDay.AddDays(daysToAdd)

    // Returns the 'next week' for the given date 
    let GetNextWeekFor(date:DateTime) = 
        let days = 
            match date.DayOfWeek with
            | DayOfWeek.Monday -> 7
            | DayOfWeek.Tuesday -> 6
            | DayOfWeek.Wednesday -> 5
            | DayOfWeek.Thursday -> 4
            | DayOfWeek.Friday -> 3
            | DayOfWeek.Saturday -> 2
            | DayOfWeek.Sunday -> 7
            | _ -> 7

        date.AddDays((float)days)

    // Returns the 'next month' for the given date
    let GetNextMonthFor(date:DateTime) = 
        let tempDate = date.AddMonths(1)
        let daysToAdd = tempDate.Day - 1
        tempDate.AddDays(-(float)daysToAdd)

    // Returns the next year for the given date
    let GetNextYearFor(date:DateTime) =
        new DateTime(date.Year + 1, 1, 1)

    // Turns a number and a word like (20, "min") into a timespan
    let rec TimespanFromWord(v:decimal, s:string) = 
        let valueTimes60 = (int)(v * 60m)
        let valueTimes24 = (int)(v * 24m)
        let valueTimes7 = (int)(v * 7m)
        
        match s with
        | Prefix "min" rest ->
            new TimeSpan(0, 0, valueTimes60);
        | CaseInsensitiveEquals "m" ->
            new TimeSpan(0, 0, valueTimes60);

        | Prefix "hour" rest | Prefix "hr" rest ->
            new TimeSpan(0, valueTimes60, 0);
        | CaseInsensitiveEquals "h" ->
            new TimeSpan(0, valueTimes60, 0);

        | Prefix "day" rest ->
            new TimeSpan(0, valueTimes24, 0, 0);
        | CaseInsensitiveEquals "d" ->
            new TimeSpan(0, valueTimes24, 0, 0);

        | Prefix "week" rest | Prefix "wk" rest ->
            new TimeSpan(0, valueTimes24 * 7, 0, 0)
        | CaseInsensitiveEquals "w" ->
            new TimeSpan(0, valueTimes24 * 7, 0, 0);

        | _ ->
        // Do i need this?
//            let success, num,str = NumberAndStringFromToken(s)
//            if success then
//                TimespanFromWord(0.0m, str)
//            else
            TimeSpan.MinValue