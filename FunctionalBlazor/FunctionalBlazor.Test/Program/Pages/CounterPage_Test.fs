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

    [<Test>][<AutoData>]
    let ``Init returns the model unchanged`` 
        (inputModel : CounterPageModel) =

        let sut = Counter.update createGuid

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

        let sut = Counter.update createGuid

        let outputModel, _ = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.NavigationComplete, asyncReplyChannel)
            |> Async.RunSynchronously

        outputModel.PendingNavigation |> shouldEqual None


    [<Test>][<AutoData>]
    let ``NavigationComplete returns no further commands`` 
        (inputModel : CounterPageModel) =

        let sut = Counter.update createGuid

        let _, nextCmd = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.NavigationComplete, asyncReplyChannel)
            |> Async.RunSynchronously

        nextCmd |> shouldEqual Cmd.none


    [<Test>][<AutoData>]
    let ``AlertShown returns model without pending dialog`` 
        (inputModel : CounterPageModel)
        (alertId : Guid)
        (alert : string) =

        let inputModel = { inputModel with PendingAlert = Some (alert, alertId) }

        let sut = Counter.update createGuid

        let outputModel, _ = 
            sut.PostAndAsyncReply(fun asyncReplyChannel -> inputModel, CounterPageMsg.AlertShown, asyncReplyChannel)
            |> Async.RunSynchronously

        outputModel.PendingAlert |> shouldEqual None


    [<Test>][<AutoData>]
    let ``AlertShown returns no further commands`` 
        (inputModel : CounterPageModel) =

        let sut = Counter.update createGuid

        let _, nextCmd = 
            sut.PostAndAsyncReply(fun asyncReplyChannel -> inputModel, CounterPageMsg.AlertShown, asyncReplyChannel)
            |> Async.RunSynchronously

        nextCmd |> shouldEqual Cmd.none


    [<Test>][<AutoData>]
    let ``CounterPageError returns the model with pending alert and loading false`` 
        (inputModel : CounterPageModel) 
        (error : exn) =

        let inputModel = { inputModel with PendingAlert = None}

        let sut = Counter.update createGuid

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

        let sut = Counter.update createGuid

        let _, nextCmd = 
            sut.PostAndAsyncReply(fun asyncReplyChannel ->  inputModel, CounterPageMsg.CounterPageError error, asyncReplyChannel)
            |> Async.RunSynchronously

        nextCmd |> shouldEqual Cmd.none

