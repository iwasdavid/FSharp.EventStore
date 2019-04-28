open EventStoreClient
open FSharp.Json
open System
open System.Text
open EventStore.ClientAPI

type Data = { PolicyHolder: string; Cost: decimal }
type Meta = { LinkId: int; Date: DateTime } 

[<EntryPoint>]
let main argv =

    eventStoreSetup "admin" "changeit" "localhost:1113"

    let connectToPs () =
        let processEvent (subscriptionBase:EventStorePersistentSubscriptionBase) (resolvedEvent:ResolvedEvent) =
          // Handle event appearing for example:
          printfn "Event meta data is: %s" (Encoding.UTF8.GetString(resolvedEvent.Event.Metadata))
          subscriptionBase.Acknowledge(resolvedEvent)

        let handleDrop (subscriptionBase:EventStorePersistentSubscriptionBase) (reason:SubscriptionDropReason) (e:exn) =
          // Handle drop logic, i.e:
          reconnectToPersistentSubscription()

        connectToPersistentSubscriptionStream "Man-Utd-Season-Ticket"
        |> andGroupName "ManchesterUnited"
        |> whenEventArrives processEvent
        |> ifSubscriptionDrops handleDrop

    let writeEvents () =
        let eventTypes =[ "OrderPlaced"; "OrderShipped"; "InvoiceSent"; "TicketDelivered" ]

        for i in [1..20] do
            let r = Random()
            let data = { PolicyHolder = "Lucas"; Cost = 2223.99m }
            let metaData = { LinkId = r.Next(1000); Date = DateTime.Now }
            
            let eventData = Json.serialize data
            let metaData = Json.serialize metaData

            let e = r.Next(0,3)

            writeEventOfType eventTypes.[e]
            |> toStream "Man-Utd-Season-Ticket"
            |> withEventData eventData
            |> andMetaData metaData |> ignore

    let readEvents () =
        let slice = readForwardAndGet 10
                    |> eventsFromStream "Man-Utd-Season-Ticket"
                    |> startingAtEvent 985

        slice.Events |> Array.iter (fun e -> printfn "Original event: %A" (Encoding.UTF8.GetString(e.Event.Data)))

    connectToPs ()
    //readEvents ()

    printfn "End of file"
    Console.ReadLine() |> ignore
    0 // return an integer exit code
