namespace FunctionalBlazor.Caching

module Messenger =
    open CachingTypes

    let messenger<'T> =
        MailboxProcessor<MessageOperation<'T>>.Start (fun inbox ->
            let rec innerLoop (eventBus : EventBus<'T>) =
                async {
                        
                    let! (command, replyChannel) = inbox.Receive()

                    let result = 
                        match command with
                        | Subscribe handler ->
                            let token = eventBus.OnPublish.Subscribe(handler)
                            Subscribed token
                        | Send message -> 
                            try
                                eventBus.Invoke(message)
                            with _ as ex -> () // log?
                            MessageSent

                    replyChannel.Reply(result)
                        
                    do! innerLoop eventBus
                }

            innerLoop (new EventBus<'T>()))


    type Messenger<'T> () =
        
        let mailbox =  messenger<'T>

        let postMessage
            (message : MessageCommand<'T>) =
            mailbox.PostAndAsyncReply(fun asyncReplyChannel -> message, asyncReplyChannel)
        
        interface IMessenger<'T> with
            member this.Post (command : MessageCommand<'T>) =
                 (postMessage command) |> Async.StartAsTask



    /// Subscribe to messenger updates.
    ///
    /// Immediately dispatch the disposable token back to the 
    /// caller after subscription so they can stop updates.
    //
    /// From then on, whenever message received it is dispatched
    /// to caller.
    ///
    /// Called as Cmd.OfSub because it isn't a one-off, it needs 
    /// a long held ref to the dispatch func to call back every
    /// time an update comes in.
    ///
    let dispatchMessages
        (messenger : IMessenger<'Content>)
        onMessageReceived
        onSubscribed
        dispatch =
        async {
            let messageHandler (sender, message : 'Content) = dispatch (onMessageReceived message)
            match! messenger.Post(MessageCommand.Subscribe messageHandler) |> Async.AwaitTask with
            | MessageResult.Subscribed token -> dispatch (onSubscribed token)     
            | _ -> ()
        } |> Async.Start

