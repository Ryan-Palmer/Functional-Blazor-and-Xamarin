namespace FunctionalBlazor.Program.Pages

module Counter =
    open FunctionalBlazor.Program
    open System
    open FunctionalBlazor.Program.ProgramTypes


    let init : CounterPageModel =
        {
            Title = "Counter"
            UTC = "Initialising clock"
            Count = 0
            PendingAlert = None
            PendingNavigation = None
        }


    let update
        createGuid 
        subscribeToTimeString =
        MailboxProcessor<CounterPageUpdate>.Start (fun inbox ->
        let rec innerLoop () =            
            async {

                let! (model, message, replyChannel) = inbox.Receive()
            
                let newModel, newCmd =
                    match message with
                    | CounterPageMsg.Init -> 
                        let timeSubCmd = Cmd.ofSub (subscribeToTimeString TimeChanged)
                        model, timeSubCmd
                    | CounterPageMsg.TimeChanged time -> 
                        { model with UTC = time.ToLongTimeString() }, Cmd.none
                    | CounterPageMsg.IncreaseCount ->  { model with Count = model.Count + 1 }, Cmd.none
                    | CounterPageMsg.ItemSelected id -> { model with PendingNavigation = Some ((sprintf "items/%s" id), createGuid()) }, Cmd.none
                    | CounterPageMsg.SetUsername usr -> { model with Title = (sprintf "Hello %s!") usr}, Cmd.none
                    | CounterPageMsg.NavigationComplete -> { model with PendingNavigation = None }, Cmd.none
                    | CounterPageMsg.AlertShown -> { model with PendingAlert = None }, Cmd.none
                    | CounterPageMsg.CounterPageError ex -> 
                        let newModel =
                            match model.PendingAlert with
                            | Some _ -> model
                            | None -> { model with PendingAlert = Some(ex.Message, (createGuid())) }
                        newModel, Cmd.none

                replyChannel.Reply(newModel,newCmd)
                do! innerLoop ()
            }
        innerLoop ())
        