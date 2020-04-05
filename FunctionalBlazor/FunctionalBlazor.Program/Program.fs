namespace FunctionalBlazor.Program

module Program =
    open ProgramTypes
    open FunctionalBlazor.Caching.CachingTypes
    open FunctionalBlazor.Program.Pages
    open System.Diagnostics
    open FunctionalBlazor.Common.Ioc
    open FunctionalBlazor.Common

    let init id = 
        {
            Id = id
            CounterPage = Counter.init
        }

    let update
        (modelCache : ICache<ProgramModel>) 
        (counterPageUpdate : MailboxProcessor<CounterPageUpdate>)
        initialState =
        MailboxProcessor<ProgramMsg>.Start (fun inbox ->
            
            let rec innerLoop (model : ProgramModel) =
                async {
                     
                    // 0. Receive a message from view or application process
                    let! message = inbox.Receive()
                    
                    // 1. Send message and part of current model to appropriate child page updater.
                    // 2. Receive reply containing new page model and command list.
                    // 3. Map page cmds to this updater's cmd type
                    // 4. Replace page model in Program model.
                    // 5. Post the new model to the UI via an observable cache
                    // 6. Run the page commands (which may generate messages, returning to step 0)
                    let! newModel, newCommand = 
                        async {
                            match message with
                            | ProgramMsg.Init -> 
                                let! counterPage, counterCmd = counterPageUpdate.PostAndAsyncReply (fun replyChannel -> model.CounterPage, CounterPageMsg.Init, replyChannel)
                                return { model with CounterPage = counterPage }, (Cmd.map ProgramMsg.CounterPageMessage counterCmd)
                            | ProgramMsg.CounterPageMessage msg -> 
                                let! counterPage, counterCmd = counterPageUpdate.PostAndAsyncReply (fun replyChannel -> model.CounterPage, msg, replyChannel)
                                return { model with CounterPage = counterPage }, (Cmd.map ProgramMsg.CounterPageMessage counterCmd)
                            | ProgramMsg.ProgramError ex -> 
                                return model, Cmd.none
                        }
                    
                    // Post new state to UI via observable cache
                    modelCache.Post(Update (Some newModel)) |> ignore

                    // Run commands - this only happens in this updater as it is the top of the state pyramid
                    Cmd.exec inbox.Post newCommand

                    //// Print message (optional)
                    Debug.WriteLine (sprintf "***RECEIVED MESSAGE *** %A" message)
                    //// Print new model (optional)
                    Debug.WriteLine (sprintf "***MODEL UPDATED *** %A" newModel)

                    // Wait for next message
                    do! innerLoop newModel
                }

            innerLoop initialState)
    

    type Program (functionProvider : IFunctionRoot) =
        
        let program : IProgramFunc =
            unbox functionProvider.Functions.[ContainerTags.IProgramFunc]

        interface IProgram with
            member this.Post (command : ProgramMsg) =
                 program.Post command
