namespace FunctionalBlazor.Caching

module CacheManager =
    open FunctionalBlazor.Caching.CachingTypes

    /// Subscribe to memory cache updates.
    ///
    /// Immediately dispatch the disposable token back to the 
    /// caller after subscription so they can stop updates.
    //
    /// From then on, whenever update received we dispatch the
    /// cache to caller.
    ///
    /// Called as Cmd.OfSub because it isn't a one-off, it needs 
    /// a long held ref to the dispatch func to call back every
    /// time an update comes in.
    ///
    let initCacheObserver
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
    

   