namespace FunctionalBlazor.Caching

module Cache =
    open CachingTypes

    let cacheProcessor<'T> =
        MailboxProcessor<CacheOperation<'T>>.Start (fun inbox ->
            let rec innerLoop ((currentCache : 'T option),(eventBus : EventBus<'T option>))  =
                async {
                        
                    let! (command, replyChannel) = inbox.Receive()

                    let result, newCache = 
                        match command with
                        | CacheCommand.Update maybeItem -> 
                            try
                                eventBus.Invoke(maybeItem)
                            with _ as ex -> () // log?
                            Updated, maybeItem
                        | CacheCommand.Fetch -> 
                            (Cache currentCache), currentCache
                        | CacheCommand.Subscribe handler ->
                            let token = eventBus.OnPublish.Subscribe(handler)
                            try
                                handler(obj(), currentCache)
                            with _ as ex -> () // log?
                            (CacheResult.Subscribed token), currentCache

                    replyChannel.Reply(result)
                        
                    do! innerLoop (newCache,eventBus)
                }

            innerLoop (None, new EventBus<'T option>()))
    
    type ObservableCache<'T> () =
        
        let mailbox = cacheProcessor<'T>
        
        let postCacheCommand
            (message : CacheCommand<'T>) =
            mailbox.PostAndAsyncReply(fun asyncReplyChannel -> message, asyncReplyChannel)

        interface ICache<'T> with
            member this.Post (command : CacheCommand<'T>) =
                 (postCacheCommand command) |> Async.StartAsTask