namespace FunctionalBlazor.Caching

module Cache =
    open CachingTypes

    let cache<'T> =
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
    
    type Cache<'T> () =
        
        let mailbox = cache<'T>
        
        let postCacheCommand
            (message : CacheCommand<'T>) =
            mailbox.PostAndAsyncReply(fun asyncReplyChannel -> message, asyncReplyChannel)

        interface ICache<'T> with
            member this.Post (command : CacheCommand<'T>) =
                 (postCacheCommand command) |> Async.StartAsTask



    /// Subscribes to memory cache updates.
    ///
    /// Immediately dispatches the disposable token back to the caller 
    /// after subscription so they can stop updates.
    //
    /// From then on, whenever update received we dispatch the cache 
    /// to caller.
    ///
    /// To use, just partially apply the appropriate cache during composition in 
    /// Function Root then pass into the page updater that wants to subscribe.
    ///
    /// You get the cache by injecting it into the FunctionRoot constructor,
    /// it is all set up, just ask for one in the same way that the IProgram cache
    /// is being passed in.
    ///
    /// Call this func in the updater using as Cmd.OfSub because it isn't a one-time 
    /// event, it needs a long held ref to the dispatch func to call back every
    /// time an update comes in.
    ///
    /// As you can see, I usually use a tuple for the cache type where the first 
    /// element acts as a tag, because the type effectively defines the 
    /// 'communication channel' and this allows more than one channel of the 
    /// same data type i.e. you could have ICache<TagOne*DataTypeOne> and also
    /// ICache<TagTwo*DataTypeOne> 
    ///
    /// If you don't want to do that you could simplify this func to use
    /// a single type like 'dispatchMessages' in the Messenger module
    /// which is nearly identical.
    ///
    let dispatchCacheUpdates
        (cache : ICache<'Tag*'Content>)
        onCacheUpdated
        onSubscribed
        dispatch =
        async {

                let cacheUpdatedHandler (sender, (maybeCache : ('Tag*'Content) option)) =
                    maybeCache
                    |> Option.map (fun (tag,data) ->  data)
                    |> onCacheUpdated
                    |> dispatch

                match! cache.Post(CacheCommand.Subscribe cacheUpdatedHandler) |> Async.AwaitTask with
                | CacheResult.Subscribed token -> dispatch (onSubscribed token)     
                | _ -> ()

            } |> Async.Start
    
