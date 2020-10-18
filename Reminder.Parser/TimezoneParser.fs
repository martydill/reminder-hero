module TimezoneParser
open System
open System.Linq

let ParseDecimal(utcOffset:decimal) =
    let hour = int(Math.Truncate(utcOffset))
    let minute = int((utcOffset - decimal(hour)) * 60m)
    new TimeSpan(hour, minute, 0)

let ParseString(fullDateString:string) = 
    let tz = fullDateString.Split(' ').Last()    
    match tz with
    |  "UT" -> new TimeSpan(0, 0 ,0)
    |  "GMT" -> new TimeSpan(0, 0 ,0)
    |  "EST" ->  new TimeSpan(-5, 0 ,0)
    |  "EDT" ->  new TimeSpan(-4, 0 ,0)
    |  "CST" ->  new TimeSpan(-6, 0 ,0)
    |  "CDT" ->  new TimeSpan(-5, 0 ,0)
    |  "MST" ->  new TimeSpan(-7, 0 ,0)
    |  "MDT" ->  new TimeSpan(-6, 0 ,0)
    |  "PST" ->  new TimeSpan(-8, 0 ,0)
    |  "PDT" ->  new TimeSpan(-7, 0 ,0)
    | _ ->

        let sign = 
            match tz.[0] with
            | '+' -> 1.0m
            | '-' -> -1.0m
            | _ -> raise (new Exception(String.Format("Expected sign but found '{0}'", tz.[0])))

        let hours = Convert.ToInt32(tz.Substring(1, 2))
        let minutes = Convert.ToInt32(tz.Substring(3, 2))
        new TimeSpan(int (decimal hours * sign), minutes, 0)