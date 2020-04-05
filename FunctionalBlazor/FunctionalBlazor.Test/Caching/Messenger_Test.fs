namespace SurveyIt.Test.Caching

module Messenger_Test =
    open NUnit.Framework
    open AutoFixture.NUnit3
    open FsUnitTyped
    open FunctionalBlazor.Caching.CachingTypes
    open FunctionalBlazor.Caching.Messenger
    open System

    type T1 = { p1 : string }
    type T2 = { p2 : int }

    [<Test>][<AutoData>]
    let ``Register of token starts messages being received``
        (message1 : T1) =

        let t1Messenger = messageProcessor<T1>

        // Handlers are executed on bg thread so assertions there are missed and test never ends
        // Avoiding this by using a mutable var to store failures
        let mutable messageSent = false 

        let handler1 (sender, msg : T1) = messageSent <- msg = message1

        let observable1= t1Messenger.PostAndAsyncReply(fun asyncReplyChannel -> (Subscribe handler1), asyncReplyChannel) |> Async.RunSynchronously

        match observable1 with 
        | Subscribed token1 ->
            t1Messenger.PostAndAsyncReply(fun asyncReplyChannel -> (Send message1), asyncReplyChannel) |> Async.RunSynchronously |> ignore
            token1.Dispose()
        | _ -> failwith "Wrong type of message response"

        messageSent |> shouldEqual true

    [<Test>][<AutoData>]
    let ``Disposal of token stops messages being received``
        (message : T1) =
        
        let t1Messenger = messageProcessor<T1>
        let mutable count = 0
        let handler1 (sender, msg : T1) = count <- count + 1

        let observable1 = t1Messenger.PostAndAsyncReply(fun asyncReplyChannel -> (Subscribe handler1), asyncReplyChannel) |> Async.RunSynchronously
        
        count |> shouldEqual 0
        
        t1Messenger.PostAndAsyncReply(fun asyncReplyChannel -> Send message, asyncReplyChannel) |> Async.RunSynchronously |> ignore
        
        count |> shouldEqual 1

        match observable1 with 
        | Subscribed token ->
            token.Dispose()
            t1Messenger.PostAndAsyncReply(fun asyncReplyChannel -> Send message, asyncReplyChannel) |> Async.RunSynchronously |> ignore
        | _ -> failwith "Wrong type of cache response"

        count |> shouldEqual 1 // would be 2 if still connected after token disposed
