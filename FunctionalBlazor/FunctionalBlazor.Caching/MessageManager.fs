namespace FunctionalBlazor.Caching

module MessageManager =
    open CachingTypes
    
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
    let initMessageObserver
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

