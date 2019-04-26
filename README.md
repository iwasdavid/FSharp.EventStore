# FSharp.EventStore

An F# library for using EventStore: https://eventstore.org/

## Writing an Event:

```fsharp
let eventData = {...}//Json
let metaData = {...}//Json

let eventId = writeEventOfType "OrderPlaced"
              |> toStream "Man-Utd-Season-Ticket"
              |> withEventData eventData
              |> andMetaData metaData

printfn "Event created with Id: %A" eventId
```