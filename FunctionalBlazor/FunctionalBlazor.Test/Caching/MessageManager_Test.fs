namespace SurveyIt.Test.Caching

module MessageManager_Test =
    open NUnit.Framework
    open AutoFixture.NUnit3
    open FunctionalBlazor.Caching.MessageManager
    open FoqNetCoreCompat
    open FunctionalBlazor.Caching.CachingTypes
    open System

    type IDispatch'=
        abstract member mock : 'a -> unit
        
    type IOnSubscribed'=
        abstract member mock : IDisposable -> unit

    type IOnMessageReceived'=
        abstract member mock : 'a -> 'b 

    type Message = Message


    [<Test>][<AutoData>]
    let ``Cache observer returns token and posts updates to caller`` () =
        
        let token = Mock.Of<IDisposable>()
        let dispatch = Mock.Of<IDispatch'>()
        let onSubscribed = Mock.Of<IOnSubscribed'>()
        let onMessageReceived = Mock.Of<IOnMessageReceived'>()
        
        let mutable messageHandler = fun (sender, args) -> ()
        let cache = Mock<IMessenger<Message>>()
                                        .Setup(fun x -> <@ x.Post(any()) @>)
                                        .Calls<MessageCommand<Message>>(fun m -> 
                                                match m with
                                                | MessageCommand.Subscribe handler ->
                                                    messageHandler <- handler  // grab delegate
                                                | _ -> failwith "wrong message type"
                                                async { return MessageResult.Subscribed token } |> Async.StartImmediateAsTask)
                                        .Create()
        
        initMessageObserver cache onMessageReceived.mock onSubscribed.mock dispatch.mock
        Async.Sleep 10 |> Async.RunSynchronously

        verify <@onSubscribed.mock(is (fun t -> t = token))@> once
        verify <@dispatch.mock(any())@> once
        verify <@onMessageReceived.mock(any())@> never

        messageHandler (null, Message)
        Async.Sleep 10 |> Async.RunSynchronously

        verify <@dispatch.mock(any())@> (exactly 2)
        verify <@onMessageReceived.mock(is (fun t -> t = Message))@> once


