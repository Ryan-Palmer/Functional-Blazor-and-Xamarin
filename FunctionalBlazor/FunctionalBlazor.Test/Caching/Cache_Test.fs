namespace FuntionalBlazor.Test.Caching

module Cache_Test =
    open NUnit.Framework
    open AutoFixture.NUnit3
    open FsUnitTyped
    open FunctionalBlazor.Caching.CachingTypes
    open FunctionalBlazor.Caching.Cache
    open System
    open FoqNetCoreCompat

    type T1 = { p1 : string }
    type T2 = { p2 : int }
    type Tag = Tag
    type Content = Content

    type IDispatch'=
        abstract member mock : 'a -> unit
        
    type IOnSubscribed'=
        abstract member mock : IDisposable -> 'a

    type IOnCacheUpdated'=
        abstract member mock : 'a option -> 'b 

    [<Test>][<AutoData>]
    let ``Register of token starts cache updates being received``
        (cache1 : T1) =
        
        let t1Cache = cache<T1>

        // Handlers are executed on bg thread so assertions there are missed and test never ends
        // Avoiding this by using a mutable var to store failures
        let mutable cacheCount1 = 0
        let mutable emptyCount1 = 0

        let handler1 (sender, msg : T1 option) = 
            if msg = (Some cache1) 
            then cacheCount1 <- cacheCount1 + 1 
            else emptyCount1 <- emptyCount1 + 1

        let observable1 = t1Cache.PostAndAsyncReply(fun asyncReplyChannel -> (CacheCommand.Subscribe handler1), asyncReplyChannel) |> Async.RunSynchronously

        match observable1 with 
        | CacheResult.Subscribed token1 ->
            t1Cache.PostAndAsyncReply(fun asyncReplyChannel -> (Update (Some cache1)), asyncReplyChannel) |> Async.RunSynchronously |> ignore
            token1.Dispose()
        | _ -> failwith "Wrong type of cache response"

        cacheCount1 |> shouldEqual 1
        emptyCount1 |> shouldEqual 1

    [<Test>][<AutoData>]
    let ``Disposal of token stops cache updates being received``
        (cache1 : T1) =
        
        let t1Cache = cache<T1>
        let mutable count = 0
        let handler1 (sender, msg : T1 option) = count <- count + 1

        let observable1 = t1Cache.PostAndAsyncReply(fun asyncReplyChannel -> (CacheCommand.Subscribe handler1), asyncReplyChannel) |> Async.RunSynchronously

        match observable1 with 
        | CacheResult.Subscribed token ->
            token.Dispose()
            t1Cache.PostAndAsyncReply(fun asyncReplyChannel -> Update (Some cache1), asyncReplyChannel) |> Async.RunSynchronously |> ignore
        | _ -> failwith "Wrong type of cache response"

        count |> shouldEqual 1 // triggered once by initial subscription, would be 2 if still connected after token disposed

    [<Test>][<AutoData>]
    let ``Fetch empty cache returns None`` () =
        
        let t1Cache = cache<T1>

        let emptyCache =
            t1Cache.PostAndAsyncReply(fun asyncReplyChannel -> Fetch, asyncReplyChannel) |> Async.RunSynchronously
        
        match emptyCache with
        | Cache c ->
            match c with
            | Some x -> failwith "Should be None"
            | None -> ()
        | _ -> failwith "Wrong type of cache response"

    [<Test>][<AutoData>]
    let ``Fetch of populated cache returns Some``
        (cache1 : T1) =
        
        let t1Cache = cache<T1>

        t1Cache.PostAndAsyncReply(fun asyncReplyChannel -> (Update (Some cache1)), asyncReplyChannel) |> Async.RunSynchronously |> ignore

        let result =
            t1Cache.PostAndAsyncReply(fun asyncReplyChannel -> Fetch, asyncReplyChannel) |> Async.RunSynchronously
        
        match result with
        | Cache c ->
            match c with
            | Some x -> x |> shouldEqual cache1
            | None -> failwith "Should be Some cache1"
        | _ -> failwith "Wrong type of cache response"

    
    [<Test>][<AutoData>]
    let ``Register of token immediately executes handler``
        (cache1 : T1) =
        
        let t1Cache = cache<T1>
        // Handlers are executed on bg thread so assertions there are missed and test never ends
        // Avoiding this by using a mutable var to store failures
        let mutable failed1 = true 
        let handler1_a (sender, msg : T1 option) = failed1 <- false
        
        t1Cache.PostAndAsyncReply(fun asyncReplyChannel -> (Update (Some cache1)), asyncReplyChannel) |> Async.RunSynchronously |> ignore
        t1Cache.PostAndAsyncReply(fun asyncReplyChannel -> (CacheCommand.Subscribe handler1_a), asyncReplyChannel) |> Async.RunSynchronously |> ignore
        
        failed1 |> shouldEqual false


    [<Test>][<AutoData>]
    let ``Cache dispatcher returns token and posts updates to caller`` () =
        
        let token = Mock.Of<IDisposable>()
        let dispatch = Mock.Of<IDispatch'>()
        let onSubscribed = Mock.Of<IOnSubscribed'>()
        let onCacheUpdated = Mock.Of<IOnCacheUpdated'>()
        
        let mutable cacheHandler = fun (sender, args) -> ()
        let cache = Mock<ICache<Tag*Content>>()
                                        .Setup(fun x -> <@ x.Post(any()) @>)
                                        .Calls<CacheCommand<Tag*Content>>(fun m -> 
                                                match m with
                                                | CacheCommand.Subscribe handler ->
                                                    cacheHandler <- handler  // grab delegate
                                                | _ -> failwith "wrong message type"
                                                async { return CacheResult.Subscribed token } |> Async.StartImmediateAsTask)
                                        .Create()
        
        dispatchCacheUpdates cache onCacheUpdated.mock onSubscribed.mock dispatch.mock
        Async.Sleep 10 |> Async.RunSynchronously

        verify <@onSubscribed.mock(is (fun t -> t = token))@> once
        verify <@dispatch.mock(any())@> once
        verify <@onCacheUpdated.mock(any())@> never

        cacheHandler (null, Some (Tag,Content))
        Async.Sleep 10 |> Async.RunSynchronously

        verify <@dispatch.mock(any())@> (exactly 2)
        verify <@onCacheUpdated.mock(is (fun t -> t.Value = Content))@> once