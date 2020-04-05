namespace FunctionalBlazor.Caching

module Messenger =
    open CachingTypes

    let messageProcessor<'T> =
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
        
        let mailbox =  messageProcessor<'T>

        let postMessage
            (message : MessageCommand<'T>) =
            mailbox.PostAndAsyncReply(fun asyncReplyChannel -> message, asyncReplyChannel)
        
        interface IMessenger<'T> with
            member this.Post (command : MessageCommand<'T>) =
                 (postMessage command) |> Async.StartAsTask