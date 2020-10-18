namespace ReminderHero.Parser

open System
open System.Collections.Generic
open System.Linq
open System.Text.RegularExpressions
open ReminderHero.Models
open Patterns
open DateTimeUtilities

type ReminderFailureReason = SmsNotSupported | OverReminderLimit | RecurringRemindersNotSupported | CouldntUnderstand | UnknownEndpoint

exception ReminderFailureException of ReminderFailureReason * string

module ReminderParser = 

    type ParsedTimespan( timeSpan : TimeSpan, success : bool, relativeToStartOfDay : bool, tokensUsed : int, ?includesAmPm: bool)  = 
          member x.Success = success
          member x.RelativeToStartOfDay = relativeToStartOfDay 
          member x.TimeSpan = timeSpan
          member x.TokensUsed = tokensUsed
          member x.IncludesAmPm = includesAmPm 

    [<Literal>]
    let MorningHour = 8
    [<Literal>]
    let AfternoonHour = 14
    [<Literal>]
    let EveningHour = 19
    [<Literal>]
    let NightHour = 21
    [<Literal>]
    let NoonHour = 12

    let private _signatureStartRegex = @"^(?i)((thanks|cheers|regards|best regards|warm regards|kind regards|kindest regards|many thanks|all the best|sincerely|best wishes)[\,!\.\r\n' ']+[a-zA-Z]+)|^(?i)- |^(?i)\s*-\s*|^(?i)<http*"

    let private _days = Map.empty.Add("one", 1.0m)
                            .Add("a", 1.0m)
                            .Add("an", 1.0m)
                            .Add("two", 2.0m)
                            .Add("three", 3.0m)
                            .Add("four", 4.0m)
                            .Add("five", 5.0m)
                            .Add("six", 6.0m)
                            .Add("seven", 7.0m)
                            .Add("eight", 8.0m)
                            .Add("nine", 9.0m)
                            .Add("ten", 10.0m)
                            .Add("fifteen", 15.0m)
                            .Add("twenty", 20.0m)
                            .Add("thirty", 30.0m)
                            .Add("first", 1m)
                            .Add("second", 2m)
                            .Add("third", 3m)
                            .Add("fourth", 4m)
                            .Add("fifth", 5m)
                            .Add("sixth", 6m)
                            .Add("seventh", 7m)
                            .Add("eighth", 8m)
                            .Add("ninth", 9m)
                            .Add("tenth", 10m)
                            .Add("eleventh", 11m)
                            .Add("twelfth", 12m)
                            .Add("thirteenth", 13m)
                            .Add("fourteenth", 14m)
                            .Add("fifteenth", 15m)
                            .Add("sixteenth", 16m)
                            .Add("seventeenth", 17m)
                            .Add("eighteenth", 18m)
                            .Add("nineteenth", 19m)
                            .Add("twentieth", 20m)
                            .Add("twentyfirst", 21m)
                            .Add("twenty-second", 22m)
                            .Add("twentythird", 23m)
                            .Add("twentyfourth", 24m)
                            .Add("twenty-fifth", 25m)
                            .Add("twentysixth", 26m)
                            .Add("twentyseventh", 27m)
                            .Add("twentyeighth", 28m)
                            .Add("twenty-ninth", 29m)
                            .Add("thirtieth", 30m)
                            .Add("thirtyfirst", 31m)
  
    // Converts a string like '2.5m' to a tuple like (true, '2.5', 'm')
    let private NumberAndStringFromToken(token:string) = 
        let digitIndex = Enumerable.Range(0, token.Length).FirstOrDefault(fun i -> (not (Char.IsDigit token.[i])) && (not (Char.IsPunctuation token.[i])))
                
        if digitIndex = 0 then 
            (false, "", "") 
        else
            let numberAsString = token.Substring(0, digitIndex).Trim()
            let isReallyANumber,theNumber = Decimal.TryParse(numberAsString) // Check if it's a number. If we have a string like "m it won't be.
            let string = token.Substring(digitIndex, token.Length - digitIndex).Trim()
            if numberAsString.Length > 0 && string.Length > 0 && isReallyANumber then
                (true, numberAsString, string)            
            else
                (false, "", "")

    let private FirstNCharactersAreNumbers(n:int, s:string) = 
        s.Length >= n && s.Substring(0, n).All(fun c -> Char.IsDigit(c))
   

    let private TimeFromString (rest:IEnumerable<string>) =  
        let first, tokensUsed = 
            if rest.Count() > 1  then
                match rest.ElementAt(1) with
                    | CaseInsensitiveEquals "am" | CaseInsensitiveEquals "pm" | CaseInsensitiveEquals "a.m." | CaseInsensitiveEquals "p.m." | CaseInsensitiveEquals "pm." | CaseInsensitiveEquals "am." ->
                        rest.First() + rest.ElementAt(1), 2
                    | _ -> rest.First(), 1
            else
                rest.First(), 1

        let adjustedFirst = first.ToLower().Replace("a.m", "am").Replace("p.m", "pm").TrimEnd('.');
        let adjustedAgain = if adjustedFirst.IndexOfAny([|'.';':'|]) > 0 then
                                adjustedFirst
                            else if (FirstNCharactersAreNumbers(3, adjustedFirst) && (adjustedFirst.Length = 3 || adjustedFirst.Length = 5)) then
                                adjustedFirst.Insert(1, ":")
                            else if (FirstNCharactersAreNumbers(4, adjustedFirst) && (adjustedFirst.Length = 4 || adjustedFirst.Length = 6)) then
                                adjustedFirst.Insert(2, ":")
                            else
                                adjustedFirst

        let formats = [|"h:mm tt";"hh:mm tt";"h:mmtt";"hh:mmtt";"hhtt";"htt";"h tt";"hh tt";"HH:mm";"h:mm";"hh:mm";"HH:mmtt";"HHtt";"HH.mm";"hh.mmtt";"HH.mm tt";"h.mm tt"|]

        let couldParse, parsed = DateTime.TryParseExact(adjustedAgain, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces)
        let includesAmPm = adjustedFirst.EndsWith("am") 
                            || adjustedFirst.EndsWith("pm")
                            || (couldParse && parsed.Hour > 12)
        if couldParse then
            new TimeSpan(parsed.Hour, parsed.Minute, parsed.Second), tokensUsed, includesAmPm
        else
            TimeSpan.MinValue, 0, includesAmPm
            
    let rec private TimespanFromTimeOfDay (timeOfDay:string, rest:IEnumerable<string>) = 
        match timeOfDay with
        | Prefix "morning" suffix->
             new TimeSpan(MorningHour, 0, 0), 1, true
        | Prefix "afternoon" suffix ->
             new TimeSpan(AfternoonHour, 0, 0), 1, true
        | Prefix "evening" suffix ->
            new TimeSpan(EveningHour, 0, 0), 1, true
        | Prefix "night" suffix ->
            new TimeSpan(NightHour, 0, 0), 1, true 
        | CaseInsensitiveEquals "at" | CaseInsensitiveEquals "@" ->
            if rest.Any() && rest.First().Equals("noon", StringComparison.CurrentCultureIgnoreCase) then
                new TimeSpan(NoonHour, 0, 0), 2, true 
            else
                let time, tokensUsed, includesAmPm = TimeFromString(rest)
                time, tokensUsed + 1, includesAmPm
        | Prefix "@" suffix ->
            let l = Seq.append [suffix] rest
            TimespanFromTimeOfDay("at", l)       
        | other -> 
             let all = new List<string>()
             all.Add(timeOfDay)

             let time, tokensUsed, includesAmPm = TimeFromString(all.Concat(rest))
             time, tokensUsed, includesAmPm
  
    let private TimespanFromDayOfWeek (dayOfWeek:string, reminder:Reminder) =
        let day = 
            match dayOfWeek with
            | Prefix "mon" rest -> DayOfWeek.Monday
            | Prefix "tue" rest -> DayOfWeek.Tuesday
            | Prefix "wed" rest -> DayOfWeek.Wednesday
            | Prefix "thu" rest -> DayOfWeek.Thursday
            | Prefix "fri" rest -> DayOfWeek.Friday
            | Prefix "sat" rest -> DayOfWeek.Saturday
            | Prefix "sun" rest -> DayOfWeek.Sunday
            | _ -> raise (new Exception(String.Format("Unknown day of week '{0}'", dayOfWeek)))

        let nextWeekday = DateTimeUtilities.GetNextWeekday(reminder.CreatedDate, day)
        nextWeekday.Subtract(reminder.CreatedDate)
    
    let private ReplaceWord (word:string) = 
        match word with
        | CaseInsensitiveEquals "my" -> "your"
        | CaseInsensitiveEquals "i" -> "you"
        | other -> other

    let private TimespanToDate (month:int, day:int, timeTS:TimeSpan, reminder:Reminder) =

        // Figure out if it's this year or next yaer
        let year = 
            if reminder.CreatedDate.Month < month || (reminder.CreatedDate.Month = month && reminder.CreatedDate.Day < day) then
                reminder.CreatedDate.Year
            else
                reminder.CreatedDate.Year + 1
        
        let date = new DateTime(year, month, day)
        let dateAndTime = 
            if timeTS.Equals(TimeSpan.MinValue) then 
                if reminder.ReminderDate <> DateTime.MinValue then
                    // We already have a time for this reminder. Add it to the date we just found.
                    date.Add(new TimeSpan(reminder.ReminderDate.Hour, reminder.ReminderDate.Minute, 0))
                else
                    // We don't have a time for this reminder. Just use the date.
                    date
            else 
                // This is a valid timespan. Add it to the date.
                date.Add(timeTS)

        dateAndTime.Subtract(reminder.CreatedDate)

    let private RecurrenceTypeFromString(token:string) = 
        let fixedToken = token.TrimEnd('.')
        if DateTimeUtilities.IsDayOfWeek(fixedToken) then 
            Recurrence.Weekly
        else
            match fixedToken with
            | CaseInsensitiveEquals "day" -> Recurrence.Daily
            | CaseInsensitiveEquals "weekday" -> Recurrence.Weekday
            | CaseInsensitiveEquals "weekend" -> Recurrence.Weekend
            | CaseInsensitiveEquals "morning" -> Recurrence.Daily
            | CaseInsensitiveEquals "afternoon" -> Recurrence.Daily
            | CaseInsensitiveEquals "evening" -> Recurrence.Daily
            | CaseInsensitiveEquals "night" -> Recurrence.Daily
            | CaseInsensitiveEquals "year" -> Recurrence.Yearly
            | _ -> Recurrence.Once
           
    let private SuccessFromTS (timespan:TimeSpan) =
        if timespan.Equals(TimeSpan.MinValue) then
            false
        else
            true
   
    // Convert numbers like 3,5 to 3.5 
    let private ConvertCommaToDecimal(str:string) =
        if str.Count(fun c -> c.Equals(',')) = 1 && str.Count(fun c -> c.Equals('.')) = 0 then
            str.Replace(',', '.')
        else
            str

    let private TimespanFromEveryX(reminder:Reminder, intValue:int, tokens:IEnumerable<string>) = 
        match tokens.First() with
        | CaseInsensitiveEquals "days" | CaseInsensitiveEquals "d" ->
            reminder.ReminderDate <- reminder.CreatedDate.AddDays((float)intValue) 
            reminder.Recurrence <- Recurrence.EveryXDays
            reminder.RecurrencePeriod <- intValue;
            true
        | CaseInsensitiveEquals "weeks" | CaseInsensitiveEquals "w" | CaseInsensitiveEquals "wks" ->
            reminder.ReminderDate <- reminder.CreatedDate.AddDays((float)(intValue * 7))
            reminder.Recurrence <- Recurrence.EveryXWeeks
            reminder.RecurrencePeriod <- intValue
            true
        | CaseInsensitiveEquals "months" | CaseInsensitiveEquals "m" | CaseInsensitiveEquals "mo" ->
            reminder.ReminderDate <- reminder.CreatedDate.AddMonths(intValue)
            reminder.Recurrence <- Recurrence.EveryXMonths
            reminder.RecurrencePeriod <- intValue
            true
        | CaseInsensitiveEquals "years" | CaseInsensitiveEquals "yrs" | CaseInsensitiveEquals "y" ->
            reminder.ReminderDate <- reminder.CreatedDate.AddYears(intValue);
            reminder.Recurrence <- Recurrence.EveryXYears
            reminder.RecurrencePeriod <- intValue
            true
        | _ ->
            false
    
    // Converts strings like '1' and 'one' into their numeric representation, if possible
    let private NumberFromString(token:string) = 
        let fixedToken = 
            match token with
            | Suffix "st" head | Suffix "nd" head | Suffix "rd" head | Suffix "th" head ->
                if token.Length > 2 then token.Substring(0, token.Length - 2) else token
            | _ ->
                token

        match System.Decimal.TryParse(fixedToken) with
        | (true, dec) -> true, dec
        | _ ->   if _days.ContainsKey(token) then
                    true, _days.[token]
                 else
                    false, 0.0m
    
    let rec private GrabTimespanFromTokens(tokens:IEnumerable<string>) = 
        if tokens.First().Equals("in", StringComparison.OrdinalIgnoreCase) then 
            let result = GrabTimespanFromTokens(tokens.Skip(1))
            new ParsedTimespan(result.TimeSpan, result.Success, result.RelativeToStartOfDay, result.TokensUsed + 1) 
        else
            let next = ConvertCommaToDecimal(tokens.ElementAt(0).ToLower())
            let success, value = NumberFromString(next)

            if success then                                
                let ts = TimespanFromWord(value, tokens.ElementAt(1))
                let success = if ts = TimeSpan.MinValue then false else true
                new ParsedTimespan(ts, success, false, 2)
            else
                let isNumberAndString, number, string = NumberAndStringFromToken(next)
                if isNumberAndString then
                    let ts = TimespanFromWord(Convert.ToDecimal(number), string)
                    new ParsedTimespan(ts, (if ts = TimeSpan.MinValue then false else true), false, 1)
                else
                    new ParsedTimespan(TimeSpan.MinValue, false, false, 0)

    let rec ParseTimespan (tokens:IEnumerable<string>, reminder:Reminder, previous:string) = 
        let first = tokens.First().TrimEnd('.')
        match first with
        | CaseInsensitiveEquals "tomorrow" ->
            if tokens.Count() > 1 then
                let ts, tokensUsed, includesAmPm = TimespanFromTimeOfDay(tokens.ElementAt(1), tokens.Skip(2))
                new ParsedTimespan(ts.Add(new TimeSpan(1, 0, 0, 0)), SuccessFromTS(ts), true, tokensUsed + 1, includesAmPm )
            else
                new ParsedTimespan(new TimeSpan(1, 0, 0, 0), true, true, 1 )

        | CaseInsensitiveEquals "tonight" ->
            new ParsedTimespan(new TimeSpan(NightHour, 0, 0), true, true, 1)

        | CaseInsensitiveEquals "this" ->
            let ts, tokensUsed,includesAmPm = TimespanFromTimeOfDay(tokens.ElementAt(1), tokens.Skip(2))
            new ParsedTimespan(ts, SuccessFromTS(ts), true, tokensUsed + 1, includesAmPm)

        | CaseInsensitiveEquals "today" ->
            if tokens.Count() > 1 then
                let ts, tokensUsed,includesAmPm = TimespanFromTimeOfDay(tokens.ElementAt(1), tokens.Skip(2))
                new ParsedTimespan(ts, SuccessFromTS(ts), true, tokensUsed + 1, includesAmPm)
            else
                new ParsedTimespan(new TimeSpan(), true, true, 1)

        | CaseInsensitiveEquals "each" | CaseInsensitiveEquals "every" | CaseInsensitiveEquals "eery" | CaseInsensitiveEquals "daily" ->

            let success, value = NumberFromString(tokens.ElementAt(1))
            if success && TimespanFromEveryX(reminder, (int)value, tokens.Skip(2)) then
                new ParsedTimespan(new TimeSpan(), true, true, 3)
            else

                let x = ParseTimespan(tokens.Skip(1), reminder, "")
                
                reminder.Recurrence <-  if x.Success then
                                            if first.Equals("daily", StringComparison.OrdinalIgnoreCase) then 
                                                Recurrence.Daily
                                            else
                                                RecurrenceTypeFromString(tokens.ElementAt(1))  
                                        else
                                            Recurrence.Once

                new ParsedTimespan(x.TimeSpan, x.Success, x.RelativeToStartOfDay, x.TokensUsed + 1)
        | _ ->

            let timespan = GrabTimespanFromTokens(tokens)
            if timespan.Success then
                timespan
            else

                let isForNextXYZ = previous.Equals("next", StringComparison.OrdinalIgnoreCase)

                let nextToken, skipCount = 
                    match first with 
                    | CaseInsensitiveEquals "on" -> tokens.ElementAt(1), 1
                    | _ -> first, 0

                if DateTimeUtilities.IsDayOfWeek(nextToken) then
                    let dayTs = TimespanFromDayOfWeek(nextToken, reminder)

                    if tokens.Count() > 1 + skipCount then
                        // Try to parse time after day, if we can - 
                        // 'saturday at 8pm'
                        let timeTs, tokensUsed,includesAmPm = TimespanFromTimeOfDay(tokens.ElementAt(1 + skipCount), tokens.Skip(2 + skipCount))
                        if timeTs.Equals(TimeSpan.MinValue) then 
                            new ParsedTimespan(dayTs.Add(timeTs), false, true, tokensUsed + 1 + skipCount, includesAmPm)
                        else
                            new ParsedTimespan(dayTs.Add(timeTs), true, true, tokensUsed + 1 + skipCount, includesAmPm) 
                    else
                        // if we can't, it might be 'at 8pm on saturday'
                        new ParsedTimespan(dayTs, true, true, 1 + skipCount)

                else if IsMonth(nextToken) then
                    let monthNumber = NumberForMonth(nextToken)
                    let success, dayNumber = NumberFromString(tokens.Skip(skipCount + 1).First())
                    if success then 
                            if tokens.Count() > 2 + skipCount then
                                // may 26 at 8pm
                                let timeTs, tokensUsed,includesAmPm = TimespanFromTimeOfDay(tokens.ElementAt(2 + skipCount), tokens.Skip(3 + skipCount))
                                let timespan = TimespanToDate(monthNumber, (int)dayNumber, timeTs, reminder)
                                new ParsedTimespan(timespan, true, false, tokensUsed + 2 + skipCount, includesAmPm)
                            else
                                // at 8pm on may 26
                                let timespan = TimespanToDate(monthNumber, (int)dayNumber, TimeSpan.MinValue, reminder)
                                new ParsedTimespan(timespan, true, false, 2 + skipCount)
                    else
                         new ParsedTimespan(TimeSpan.MinValue, false, false, 0)

                else if nextToken.Equals("day", StringComparison.OrdinalIgnoreCase) then
                    new ParsedTimespan(new TimeSpan(), true, true, 1 + skipCount)

                else if nextToken.Equals("weekday", StringComparison.OrdinalIgnoreCase) then
                    new ParsedTimespan(new TimeSpan(), true, true, 1 + skipCount)

                else if nextToken.Equals("weekend", StringComparison.OrdinalIgnoreCase) then
                    new ParsedTimespan(new TimeSpan(), true, true, 1 + skipCount)

                else if not isForNextXYZ && (nextToken.Equals("year", StringComparison.OrdinalIgnoreCase) || nextToken.Equals("yr", StringComparison.OrdinalIgnoreCase)) then
                    let ts = new TimeSpan(reminder.CreatedDate.Hour, reminder.CreatedDate.Minute, 0)
                    new ParsedTimespan(ts, true, true, 1 + skipCount)

                else
                    let timeTs, tokensUsed,includesAmPm = TimespanFromTimeOfDay(tokens.ElementAt(0), tokens.Skip(1))
                    let success = if timeTs.Equals(TimeSpan.MinValue) then false else true
                    new ParsedTimespan(timeTs, success, true, tokensUsed, includesAmPm)
    
    let IsStartOfSignature(i:int, tokens:IEnumerable<string>) = 
        let string = String.Join(" ", tokens.Skip(i).Take(Math.Max(3, tokens.Count() - 1)))     
        let result = Regex.Match(string, _signatureStartRegex)
        result.Success

    let LookForNextXYZ(reminder:Reminder) = 
        if reminder.Description.IndexOf("next week", StringComparison.OrdinalIgnoreCase) <> -1  then //|| reminder.Description.IndexOf("next wk", StringComparison.OrdinalIgnoreCase) <> -1 then
           let newDesc = StringUtils.ReplaceString(reminder.Description, "next week", "", StringComparison.OrdinalIgnoreCase)
           reminder.Description <- newDesc.Replace("  ", " ")
           reminder.ReminderDate <- DateTimeUtilities.GetNextWeekFor(reminder.CreatedDate)
        else if reminder.Description.IndexOf("next month", StringComparison.OrdinalIgnoreCase) <> -1 then //|| reminder.Description.IndexOf("next mo", StringComparison.OrdinalIgnoreCase) <> -1 then 
           let newDesc = StringUtils.ReplaceString(reminder.Description, "next month", "", StringComparison.OrdinalIgnoreCase)
           reminder.Description <- newDesc.Replace("  ", " ")
           reminder.ReminderDate <- DateTimeUtilities.GetNextMonthFor(reminder.CreatedDate) 
        else if reminder.Description.IndexOf("next year", StringComparison.OrdinalIgnoreCase) <> -1 then //|| reminder.Description.IndexOf("next yr", StringComparison.OrdinalIgnoreCase) <> -1 then
           let newDesc = StringUtils.ReplaceString(reminder.Description, "next year", "", StringComparison.OrdinalIgnoreCase)
           reminder.Description <- newDesc.Replace("  ", " ")
           reminder.ReminderDate <- DateTimeUtilities.GetNextYearFor(reminder.CreatedDate)  
      
    let ParseDescription (tokens:IEnumerable<string>, reminder:Reminder) = 
        
        let mutable i = 0
        while i < tokens.Count() do
            if IsStartOfSignature(i, tokens) then
                i <- tokens.Count()
            else
                let word = tokens.ElementAt(i)
                let parsedTimespan = ParseTimespan(tokens.Skip(i), reminder, if i > 0 then tokens.ElementAt(i - 1) else "")
                if not parsedTimespan.Success then
                    reminder.Description <- reminder.Description + ReplaceWord(word) + " "
                    i <- i + 1
                else
                    if parsedTimespan.RelativeToStartOfDay then
                        let date = if reminder.ReminderDate = DateTime.MinValue then
                                        new DateTime(reminder.CreatedDate.Year, reminder.CreatedDate.Month, reminder.CreatedDate.Day)
                                    else
                                        reminder.ReminderDate

                        let moddedDate = date.Add(parsedTimespan.TimeSpan)

                        // If this reminder didn't include am/pm, make sure we use the nearest one, not just AM
                        reminder.ReminderDate <- if moddedDate < reminder.CreatedDate && parsedTimespan.IncludesAmPm.IsSome && parsedTimespan.IncludesAmPm.Value = false then
                                                    moddedDate.AddHours(float 12)
                                                 else
                                                    moddedDate
                    else
                        reminder.ReminderDate <- reminder.CreatedDate.Add(parsedTimespan.TimeSpan)
//                        reminder.ReminderDate <- if reminder.ReminderDate = DateTime.MinValue then
//                                                    reminder.CreatedDate.Add(parsedTimespan.TimeSpan)
//                                                 else
//                                                    reminder.ReminderDate.Add(parsedTimespan.TimeSpan)

                        reminder.IsTimeRelative <- true

                    i <- i + parsedTimespan.TokensUsed

       
        if reminder.ReminderDate.Equals(DateTime.MinValue) then
            LookForNextXYZ(reminder)        
        if reminder.ReminderDate.Equals(DateTime.MinValue) then
            raise (new Exception("Couldn't determine ReminderDate for the message '" + reminder.Description + "'"))
        if reminder.Description.StartsWith("remind me", StringComparison.OrdinalIgnoreCase) then
            let lengthToTrim = "remind me".Length
            reminder.Description <- reminder.Description.Substring(lengthToTrim, reminder.Description.Length - lengthToTrim)
        if reminder.Description.StartsWith("and ", StringComparison.OrdinalIgnoreCase) then
            reminder.Description <- reminder.Description.Substring(3, reminder.Description.Length - 3)
        
        reminder.Description <- reminder.Description.Trim()

    let Parse (message:string, now:DateTime) = 
        let r = new Reminder()
        r.CreatedDate <- now
        r.ReminderDate <- DateTime.MinValue
        let words = message.Trim().Split([|' ';'\n'|], StringSplitOptions.RemoveEmptyEntries).ToList()
        let fixedWords = 
            match words.[0] with
            | Prefix "remind" rest -> 
                match words.[1] with
                | Prefix "me" rest -> words.Skip(2).ToList()
                | _ -> words.Skip(1).ToList()
            | _ -> words

        ParseDescription(fixedWords, r)

        // Adjust days for recurring reminders in case they've already passed today
        if r.Recurrence = Recurrence.Daily && r.ReminderDate.TimeOfDay < now.TimeOfDay then
            r.ReminderDate <- r.ReminderDate.AddDays(1.0)
        if r.Recurrence = Recurrence.Weekday then
            if r.ReminderDate.DayOfWeek = DayOfWeek.Saturday then
                r.ReminderDate <- r.ReminderDate.AddDays(2.0)
            else if r.ReminderDate.DayOfWeek = DayOfWeek.Sunday then
                r.ReminderDate <- r.ReminderDate.AddDays(1.0)
            else if r.ReminderDate.TimeOfDay < now.TimeOfDay then
                if r.ReminderDate.DayOfWeek = DayOfWeek.Friday then
                    r.ReminderDate <- r.ReminderDate.AddDays(3.0)
                else
                    r.ReminderDate <- r.ReminderDate.AddDays(1.0)
           
        r
