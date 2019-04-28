module EventStoreClient

open EventStore.ClientAPI
open System
open System.Text

type ProcessEventType = EventStorePersistentSubscriptionBase -> ResolvedEvent -> unit
type SubscriptionDroppedType = EventStorePersistentSubscriptionBase -> SubscriptionDropReason -> exn -> unit

// Change all this
let mutable  username:string = ""
let mutable  password:string = ""
let mutable  address:string = ""

// And this
let eventStoreSetup name pw ad =
  username <- name
  password <- pw
  address <- ad

let createConnection () =
  let uri = Uri(sprintf "tcp://%s:%s@%s" username password address)
  let conn = EventStoreConnection.Create(uri)
  conn.ConnectAsync().Wait()
  conn
  
// Persistent Subscriptions

let mutable psStream:string = ""
let mutable psGroup:string = ""
let mutable psProcess:ProcessEventType = (fun _ _ -> ())
let mutable psDropped:SubscriptionDroppedType = (fun _ _ _ -> ())

let connectToPersistentSubscription (stream:string) (group:string) (pro:ProcessEventType) (sub:SubscriptionDroppedType) =
  psStream <- stream
  psGroup <- group
  psProcess <- pro
  psDropped <- sub

  let conn = createConnection()
  conn.ConnectToPersistentSubscription(stream, group, pro, sub) |> ignore

let reconnectToPersistentSubscription() =
  let conn = createConnection()
  conn.ConnectToPersistentSubscription(psStream, psGroup, psProcess, psDropped) |> ignore

let connectToPersistentSubscriptionStream stream = connectToPersistentSubscription stream

let andGroupName group f = f group

let whenEventAppears (processEvent:ProcessEventType) f = f processEvent

let ifSubscriptionDrops (s:SubscriptionDroppedType) f = f s

// Read events forwards

let readEventsForwards count streamName start =
  let connection = createConnection()
  let events = connection.ReadStreamEventsForwardAsync(streamName, (int64 start), count, true)
               |> Async.AwaitTask
               |> Async.RunSynchronously
  connection.Dispose() |> ignore
  events

let readForwardAndGet count = readEventsForwards count

let eventsFromStream s f = f s

let startingAtEvent c f = f c

// Append events to stream

let appendToStreamAsync (connection:IEventStoreConnection) (eventType:string) (streamName:string) (eventData:string) (metaData:string) =
  let eventBytes = Encoding.ASCII.GetBytes eventData
  let eventMetaBytes = Encoding.ASCII.GetBytes metaData
  let event = EventData(Guid.NewGuid(), eventType, true, eventBytes, eventMetaBytes)

  connection.AppendToStreamAsync(streamName, (int64 -2), event)
  |> Async.AwaitTask
  |> Async.RunSynchronously
  |> ignore

  connection.Dispose() |> ignore
  
  event.EventId

let writeEventOfType (eventType:string) =                          
  let connection = createConnection()
  appendToStreamAsync connection eventType
    
let toStream stream appendToStreamAsync = appendToStreamAsync stream

let withEventData data appendToStreamAsync = appendToStreamAsync data

let andMetaData data appendToStreamAsync = appendToStreamAsync data