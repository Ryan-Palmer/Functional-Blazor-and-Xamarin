namespace FuntionalBlazor.Test.Program

module Program_Test =
    open NUnit.Framework
    open FunctionalBlazor.Program
    open FoqNetCoreCompat
    open FsUnitTyped
    open AutoFixture.NUnit3
    open FunctionalBlazor.Program.ProgramTypes
    open FunctionalBlazor.Caching.CachingTypes
    
    [<Test>][<AutoData>]
    let ``Init updates model with initialised pages``
        (inputModel : ProgramModel)
        (counterPage : CounterPageModel) =

        let cache = Mock.Of<ICache<ProgramModel>>()

        let counterPageUpdater =
            MailboxProcessor<CounterPageUpdate>.Start (fun inbox ->
                let rec innerLoop () =
                    async {
                        let! (model, message, replyChannel) = inbox.Receive()
                        match message with
                        | CounterPageMsg.Init  ->
                            replyChannel.Reply(counterPage, Cmd.none)
                        | _ -> failwith "Wrong message"
                        do! innerLoop ()
                    }
                innerLoop ())

        let sut = Program.update cache counterPageUpdater inputModel
        
        sut.Post Init
        Async.Sleep 75 |> Async.RunSynchronously

        verify <@cache.Post(is (fun m -> 
            match m with
            | CacheCommand.Update maybePrgrm -> 
                let prgrm = maybePrgrm.Value
                prgrm.CounterPage = counterPage
            | _ -> failwith "Should be update message"))@> atleastonce


    [<Test>][<AutoData>]
    let ``CounterPage Init return cmds executed`` 
        (inputModel : ProgramModel) =

        let cache = Mock.Of<ICache<ProgramModel>>()
        let mutable surveyMessageReceived = false;

        let counterPageUpdater =
            MailboxProcessor<CounterPageUpdate>.Start (fun inbox ->
                let rec innerLoop () =
                    async {
                        let! (model, message, replyChannel) = inbox.Receive()
                        match message with
                        | CounterPageMsg.Init ->
                            replyChannel.Reply(model, Cmd.ofMsg CounterPageMsg.AlertShown)
                        | CounterPageMsg.AlertShown ->
                            surveyMessageReceived <- true
                            replyChannel.Reply(model, Cmd.none)
                        | _ -> failwith "Wrong message"
                        do! innerLoop ()
                    }
                innerLoop ())
       
        let sut = Program.update cache counterPageUpdater inputModel
        
        sut.Post Init
        Async.Sleep 75 |> Async.RunSynchronously
                
        surveyMessageReceived |> shouldEqual true


    [<Test>][<AutoData>]
    let ``CounterPageMsg forwarded to Counter page``
        (inputModel : ProgramModel) =
    
        let cache = Mock.Of<ICache<ProgramModel>>()
        let mutable surveyMessageReceived = false
        
        let counterPageUpdater =
            MailboxProcessor<CounterPageUpdate>.Start (fun inbox ->
                let rec innerLoop () =
                    async {
                        let! ( model, message, replyChannel) = inbox.Receive()
                        match message with
                        | CounterPageMsg.AlertShown ->
                            surveyMessageReceived <- true
                            replyChannel.Reply(model, Cmd.none)
                        | _ -> failwith "Wrong message"
                        do! innerLoop ()
                    }
                innerLoop ())
       
        let sut = Program.update cache counterPageUpdater inputModel

        sut.Post (CounterPageMessage CounterPageMsg.AlertShown)
        Async.Sleep 75 |> Async.RunSynchronously
                
        surveyMessageReceived |> shouldEqual true

    
    [<Test>][<AutoData>]
    let ``Counter page return cmds executed`` 
        (inputModel : ProgramModel) =

        let cache = Mock.Of<ICache<ProgramModel>>()
        let mutable surveyMessageReceived = false;

        let counterPageUpdater =
            MailboxProcessor<CounterPageUpdate>.Start (fun inbox ->
                let rec innerLoop () =
                    async {
                        let! (model, message, replyChannel) = inbox.Receive()
                        match message with
                        | CounterPageMsg.AlertShown ->
                            replyChannel.Reply(model, Cmd.ofMsg CounterPageMsg.NavigationComplete)
                        | CounterPageMsg.NavigationComplete ->
                            surveyMessageReceived <- true
                            replyChannel.Reply(model, Cmd.none)
                        | _ -> failwith "Wrong message"
                        do! innerLoop ()
                    }
                innerLoop ())
        
        let sut = Program.update cache counterPageUpdater inputModel
        
        sut.Post (CounterPageMessage(CounterPageMsg.AlertShown))
        Async.Sleep 75 |> Async.RunSynchronously

        surveyMessageReceived |> shouldEqual true