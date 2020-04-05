namespace SurveyIt.Test.Caching

module CacheManager_Test =
    open NUnit.Framework
    open AutoFixture.NUnit3
    open FunctionalBlazor.Caching.CacheManager
    open FoqNetCoreCompat
    open FunctionalBlazor.Caching.CachingTypes
    open System

    type IDispatch'=
        abstract member mock : 'a -> unit
        
    type IOnSubscribed'=
        abstract member mock : IDisposable -> 'a

    type IOnCacheUpdated'=
        abstract member mock : 'a option -> 'b 

    type Tag = Tag
    type Content = Content
    type Message = Message


    [<Test>][<AutoData>]
    let ``Cache observer returns token and posts updates to caller`` () =
        
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
        
        initCacheObserver cache onCacheUpdated.mock onSubscribed.mock dispatch.mock
        Async.Sleep 10 |> Async.RunSynchronously

        verify <@onSubscribed.mock(is (fun t -> t = token))@> once
        verify <@dispatch.mock(any())@> once
        verify <@onCacheUpdated.mock(any())@> never

        cacheHandler (null, Some (Tag,Content))
        Async.Sleep 10 |> Async.RunSynchronously

        verify <@dispatch.mock(any())@> (exactly 2)
        verify <@onCacheUpdated.mock(is (fun t -> t.Value = Content))@> once


