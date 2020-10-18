namespace ReminderHero.Parser
open NodaTime

module ReminderBuilder = 

    open System
    open ReminderHero.Models
    open ReminderParser
    open TimezoneParser
    open ReminderHero.Models.DataAccess
    open System.Linq

    let private TryParse(str,date:DateTime, tz:DateTimeZone) =

        if String.IsNullOrEmpty(str) || str.IndexOf("no subject", StringComparison.OrdinalIgnoreCase) <> -1 then
            None, None
        else
            try
                Some(ReminderParser.Parse(str, date)), None
            with
                | ex -> None, Some(ex)

    let Build(address, subject, textMessage, date:DateTime, endpoint:Endpoint, user:User, repo:IReminderRepository) = 
    
        let plan = match user with 
                   | null -> PricePlan.PlanForId(new System.Nullable<int>())
                   | _ -> PricePlan.PlanForId(user.PricePlanId)

        match endpoint with
        | null -> 
            raise (ReminderFailureException(ReminderFailureReason.UnknownEndpoint, ""))
        | _ -> 
            if endpoint.EndpointType = EndpointType.Phone && plan.SmsPerMonth = 0 then
                raise (ReminderFailureException(ReminderFailureReason.SmsNotSupported, ""))
            else
                ()

        let tz = match user with 
                    | null -> DateTimeZoneProviders.Tzdb.["America/Los_Angeles"]
                    | _ -> match user.TimeZone with
                            | null -> DateTimeZoneProviders.Tzdb.["America/Los_Angeles"]
                            | _ -> DateTimeZoneProviders.Tzdb.[user.TimeZone]
        
        let utc = date.ToUniversalTime()
        let instant = Instant.FromDateTimeUtc(utc)
        let inZone = instant.InZone(tz)
        let adjustedDate = inZone.ToDateTimeUnspecified()

        let thisMonthReminders = repo.RemindersForCurrentMonth(user, endpoint.EndpointType, utc)
        if thisMonthReminders.Count() >= plan.EmailPerMonth && endpoint.Type = (int)EndpointType.Email then
            raise (ReminderFailureException(ReminderFailureReason.OverReminderLimit, ""))
        else if thisMonthReminders.Count() >= plan.SmsPerMonth && endpoint.EndpointType = EndpointType.Phone then
            raise (ReminderFailureException(ReminderFailureReason.OverReminderLimit, ""))     

        let r1, e1 = TryParse(subject, adjustedDate, tz)
        let r2, e2 = TryParse(textMessage, adjustedDate, tz)

        let r = 
            match r1 with
            | Some x -> x 
            | None -> 
                match r2 with
                | Some y -> y
                | None ->
                    raise (ReminderFailureException(ReminderFailureReason.CouldntUnderstand, "Didn't match subject or message"))

        r.Email <- address
        r.SourceTitle <- subject
        r.SourceMessage <- textMessage

        if r.IsTimeRelative then       
            let localDateTime = LocalDateTime.FromDateTime(r.ReminderDate)
            let zoned = localDateTime.InZoneLeniently(tz)
            r.ReminderDate <- zoned.ToDateTimeUtc()
         else 
            let localDateTime = LocalDateTime.FromDateTime(r.ReminderDate)
            let zoned = localDateTime.InZoneLeniently(tz)
            r.ReminderDate <- zoned.ToDateTimeUtc()

        r.CreatedDate <- r.CreatedDate.ToUniversalTime()

        match user with
        | null -> ()
        | _ ->
            r.Reminder1Fired <-  date.ToUniversalTime() > r.ReminderDate.AddMinutes(-(float)user.Reminder1Number) && user.Reminder1Enabled
            r.Reminder2Fired <-  date.ToUniversalTime() > r.ReminderDate.AddMinutes(-(float)user.Reminder2Number) && user.Reminder2Enabled
            r.Reminder3Fired <-  date.ToUniversalTime() > r.ReminderDate.AddMinutes(-(float)user.Reminder3Number) && user.Reminder3Enabled
            r.Reminder4Fired <-  date.ToUniversalTime() > r.ReminderDate.AddMinutes(-(float)user.Reminder4Number) && user.Reminder4Enabled
             
            r.UserId <- System.Nullable user.UserId

        if r.Recurrence <> Recurrence.Once && not plan.RecurringReminders then
            raise (ReminderFailureException(ReminderFailureReason.RecurringRemindersNotSupported, ""))
        else 
            ()
        r