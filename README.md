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

## Read Events from Stream

```fsharp
let slice = readForwardAndGet 10
            |> eventsFromStream "Man-Utd-Season-Ticket"
            |> startingAtEvent 765

    
slice.Events |> Array.iter (fun e -> printfn "Original event: %A" (Encoding.UTF8.GetString(e.Event.Metadata)))
```

## Persistent Subscriptions

```fsharp
let processEvent (subscriptionBase:EventStorePersistentSubscriptionBase) (resolvedEvent:ResolvedEvent) =
    // Handle event appearing for example:
    printfn "Event meta data is: %s" (Encoding.UTF8.GetString(resolvedEvent.Event.Metadata))
    subscriptionBase.Acknowledge(resolvedEvent)

let handleDrop (subscriptionBase:EventStorePersistentSubscriptionBase) (reason:SubscriptionDropReason) (e:exn) =
    // Handle drop logic, i.e:
    reconnectToPersistentSubscription()

connectToPersistentSubscriptionStream "Man-Utd-Season-Ticket"
|> andGroupName "ManchesterUnited"
|> whenEventAppears processEvent
|> ifSubscriptionDrops handleDrop
```