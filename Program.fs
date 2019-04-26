open EventStoreClient
open FSharp.Json
open System

type Data = { PolicyHolder: string; Cost: decimal }
type Meta = { LinkId: int; Date: DateTime } 

[<EntryPoint>]
let main argv =

    eventStoreSetup "admin" "changeit" "localhost:1113"

    let data = { PolicyHolder = "David"; Cost = 80.99m }
    let metaData = { LinkId = 123; Date = DateTime.Now }
    
    let eventData = Json.serialize data
    let metaData = Json.serialize metaData

    let eventId = writeEventOfType "OrderPlaced"
                  |> toStream "Man-Utd-Season-Ticket"
                  |> withEventData eventData
                  |> andMetaData metaData

    printfn "Event created with Id: %A" eventId
    0 // return an integer exit code
