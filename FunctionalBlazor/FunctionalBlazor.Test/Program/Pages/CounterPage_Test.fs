namespace FuntionalBlazor.Test.Program

module CounterPage_Test =
    open NUnit.Framework
    open FunctionalBlazor.Program
    open System
    open FsUnitTyped
    open AutoFixture.NUnit3
    open FunctionalBlazor.Program.ProgramTypes
    open FunctionalBlazor.Program.Pages

    let testId = Guid.NewGuid()
    let createGuid () = testId
    let tsGen onUpdated dispatch = ()

    [<Test>][<AutoData>]
    let ``Init returns the model unchanged`` 
        (inputModel : CounterPageModel) =

        let sut = Counter.update createGuid tsGen

        let outputModel, _ = 
            sut.PostAndAsyncReply(fun asyncReplyChannel -> inputModel, CounterPageMsg.Init, asyncReplyChannel)
            |> Async.RunSynchronously

        outputModel |> shouldEqual inputModel


    [<Test>][<AutoData>]
    let ``NavigationComplete returns model without pending navigation`` 
        (inputModel : CounterPageModel)
        (navId : Guid) 
        (path : NavDestination) =

        let inputModel = { inputModel with PendingNavigation = Some (path, navId) }

        let sut = Counter.update createGuid tsGen

        let outputModel, _ = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.NavigationComplete, asyncReplyChannel)
            |> Async.RunSynchronously

        outputModel.PendingNavigation |> shouldEqual None


    [<Test>][<AutoData>]
    let ``NavigationComplete returns no further commands`` 
        (inputModel : CounterPageModel) =

        let sut = Counter.update createGuid tsGen

        let _, nextCmd = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.NavigationComplete, asyncReplyChannel)
            |> Async.RunSynchronously

        nextCmd |> shouldEqual Cmd.none


    [<Test>][<AutoData>]
    let ``IncreaseCount returns model with incremented count`` 
        (inputModel : CounterPageModel) =

        let inputModel = { inputModel with Count = 0 }

        let sut = Counter.update createGuid tsGen

        let outputModel, _ = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.IncreaseCount, asyncReplyChannel)
            |> Async.RunSynchronously

        outputModel |> shouldEqual { inputModel with Count = 1 }


    [<Test>][<AutoData>]
    let ``IncreaseCount returns no further commands`` 
        (inputModel : CounterPageModel) =

        let sut = Counter.update createGuid tsGen

        let _, nextCmd = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.IncreaseCount, asyncReplyChannel)
            |> Async.RunSynchronously

        nextCmd |> shouldEqual Cmd.none


    [<Test>][<AutoData>]
    let ``SetUsername returns model with Welcome title`` 
        (inputModel : CounterPageModel)
        (username : string) =

        let inputModel = { inputModel with Title = String.Empty }

        let sut = Counter.update createGuid tsGen

        let outputModel, _ = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.SetUsername username, asyncReplyChannel)
            |> Async.RunSynchronously

        outputModel |> shouldEqual { inputModel with Title  = (sprintf "Hello %s!" username) }


    [<Test>][<AutoData>]
    let ``SetUsername returns no further commands`` 
        (inputModel : CounterPageModel)
        (username : string) =

        let sut = Counter.update createGuid tsGen

        let _, nextCmd = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.SetUsername username, asyncReplyChannel)
            |> Async.RunSynchronously

        nextCmd |> shouldEqual Cmd.none


    [<Test>][<AutoData>]
    let ``ItemSelected returns model with Pending nav`` 
        (inputModel : CounterPageModel)
        (itemId : ItemId) =

        let inputModel = { inputModel with PendingNavigation = None}
       
        let sut = Counter.update createGuid tsGen
       
        let outputModel, _ = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.ItemSelected itemId, asyncReplyChannel)
            |> Async.RunSynchronously
       
        let (path, id) = outputModel.PendingNavigation.Value
        id |> shouldEqual testId
        path |> shouldEqual (sprintf "items/%s" itemId)
       


    [<Test>][<AutoData>]
    let ``ItemSelected returns no further commands`` 
        (inputModel : CounterPageModel)
        (itemId : ItemId) =

        let sut = Counter.update createGuid tsGen

        let _, nextCmd = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.ItemSelected itemId, asyncReplyChannel)
            |> Async.RunSynchronously

        nextCmd |> shouldEqual Cmd.none


    [<Test>][<AutoData>]
    let ``AlertShown returns model without pending dialog`` 
        (inputModel : CounterPageModel)
        (alertId : Guid)
        (alert : string) =

        let inputModel = { inputModel with PendingAlert = Some (alert, alertId) }

        let sut = Counter.update createGuid tsGen

        let outputModel, _ = 
            sut.PostAndAsyncReply(fun asyncReplyChannel -> inputModel, CounterPageMsg.AlertShown, asyncReplyChannel)
            |> Async.RunSynchronously

        outputModel.PendingAlert |> shouldEqual None


    [<Test>][<AutoData>]
    let ``AlertShown returns no further commands`` 
        (inputModel : CounterPageModel) =

        let sut = Counter.update createGuid tsGen

        let _, nextCmd = 
            sut.PostAndAsyncReply(fun asyncReplyChannel -> inputModel, CounterPageMsg.AlertShown, asyncReplyChannel)
            |> Async.RunSynchronously

        nextCmd |> shouldEqual Cmd.none


    [<Test>][<AutoData>]
    let ``CounterPageError returns the model with pending alert and loading false`` 
        (inputModel : CounterPageModel) 
        (error : exn) =

        let inputModel = { inputModel with PendingAlert = None}

        let sut = Counter.update createGuid tsGen

        let outputModel, _ = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.CounterPageError error, asyncReplyChannel)
            |> Async.RunSynchronously

        let (alert, id) = outputModel.PendingAlert.Value
        id |> shouldEqual testId
        alert |> shouldEqual error.Message


    [<Test>][<AutoData>]
    let ``CounterPageError returns no further commands`` 
        (inputModel : CounterPageModel) 
        (error : exn) =

        let sut = Counter.update createGuid tsGen

        let _, nextCmd = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.CounterPageError error, asyncReplyChannel)
            |> Async.RunSynchronously

        nextCmd |> shouldEqual Cmd.none

