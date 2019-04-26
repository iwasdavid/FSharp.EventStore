module EventStoreClient

open EventStore.ClientAPI
open System
open System.Text

let mutable private username:string = ""
let mutable private password:string = ""
let mutable private address:string = ""

// Change all this
let eventStoreSetup name pw ad =
    username <- name
    password <- pw
    address <- ad

let createConnection () =
    let uri = Uri(sprintf "tcp://%s:%s@%s" username password address)
    let conn = EventStoreConnection.Create(uri)
    conn.ConnectAsync().Wait()
    conn

let appendToStreamAsync (connection:IEventStoreConnection) (eventType:string) (streamName:string) (eventData:string) (metaData:string) =
    let eventBytes = Encoding.ASCII.GetBytes eventData
    let eventMetaBytes = Encoding.ASCII.GetBytes metaData
    let event = EventData(Guid.NewGuid(), eventType, true, eventBytes, eventMetaBytes)

    connection.AppendToStreamAsync(streamName, (int64 -2), event)
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore
    
    event.EventId

let writeEventOfType (eventType:string) =                          
    let connection = createConnection()
    appendToStreamAsync connection eventType
    
let toStream stream appendToStreamAsync = appendToStreamAsync stream

let withEventData data appendToStreamAsync = appendToStreamAsync data

let andMetaData data appendToStreamAsync = appendToStreamAsync data