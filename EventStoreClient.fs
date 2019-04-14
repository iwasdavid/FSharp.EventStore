module EventStoreClient

open EventStore.ClientAPI
open System

type EventType =
 | EncIngested
 | EncChecked

type EventData = 
    { Name: string
      Something: int }

type MetaData = 
    { Name2: string
      Something2: int }

let mutable private username:string = ""
let mutable private password:string = ""
let mutable private address:string = ""

let eventStoreSetup name pw ad =
    username <- name
    password <- pw
    address <- ad

let createConnection() =
    let uri = Uri(sprintf "tcp://%s:%s@%s" username password address)
    let conn = EventStoreConnection.Create(uri)
    conn.ConnectAsync().Wait()
    conn

let appendToStreamAsync (connection:IEventStoreConnection) (eventType:string) (streamName:string) (eventData:string) (metaData:string) =
    let eventBytes = System.Text.Encoding.ASCII.GetBytes eventData
    let eventMetaBytes = System.Text.Encoding.ASCII.GetBytes metaData
    let event = EventData(Guid.NewGuid(), eventType, true, eventBytes, eventMetaBytes)

    connection.AppendToStreamAsync(streamName, (int64 -2), event)
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore
    
    event.EventId

let writeEventOfType (eventType:EventType) =
    let stringEventType = match eventType with
                          | EncIngested -> "EncIngested"
                          | EncChecked -> "EncChecked"
                          
    let connection = createConnection()
    appendToStreamAsync connection stringEventType
    
let toStream stream func = func stream

let withEventData data func = func data

let andMetaData data func = func data