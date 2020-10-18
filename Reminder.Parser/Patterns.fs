namespace ReminderHero.Parser
open System

module Patterns = 

    let (|Prefix|_|) (p:string) (s:string) =
        if s.StartsWith(p, StringComparison.OrdinalIgnoreCase) then
            Some(s.Substring(p.Length))
        else
            None
    
    let (|Suffix|_|) (p:string) (s:string) =
        if s.EndsWith(p, StringComparison.OrdinalIgnoreCase) then
            Some(s.Substring(p.Length))
        else
            None

    let (|CaseInsensitiveEquals|_|) (str:string) arg = 
        if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
            then Some() else None