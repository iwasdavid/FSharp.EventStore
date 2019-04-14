open EventStoreClient
open FSharp.Json

[<EntryPoint>]
let main argv =

    eventStoreSetup "admin" "changeit" "localhost:1113"

    let data = { Name = "EncIngested"; Something = 12 }
    let metaData = { Name2 = "Blah"; Something2 = 122 }
    
    let encIngestedData = Json.serialize data
    let encIngestedMetaData = Json.serialize metaData

    let eventId = writeEventOfType EncIngested
                  |> toStream "Cell-GB160120"
                  |> withEventData encIngestedData
                  |> andMetaData encIngestedMetaData 

    printfn "Event created with Id: %A" eventId
    0 // return an integer exit code
