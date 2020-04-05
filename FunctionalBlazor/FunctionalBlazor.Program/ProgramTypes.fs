namespace FunctionalBlazor.Program

module ProgramTypes =
    open System
    
    type NavDestination = string

    type CounterPageModel =
        {
            Title : string
            UTC : string
            Count : int
            PendingAlert : (string * Guid) option
            PendingNavigation : (NavDestination * Guid) option
        }

    type ProgramModel =
        {
            Id : string
            CounterPage : CounterPageModel
        }
        
    type ItemId = string

    type CounterPageMsg =
        | Init
        | IncreaseCount
        | SetUsername of string
        | ItemSelected of ItemId
        | NavigationComplete
        | AlertShown
        | CounterPageError of exn


    type ProgramMsg =
        | Init
        | CounterPageMessage of CounterPageMsg
        | ProgramError of exn

    type IProgramFunc = MailboxProcessor<ProgramMsg>
    type IProgram =
        abstract member Post : ProgramMsg -> unit

    type CounterPageUpdate = CounterPageModel*CounterPageMsg*AsyncReplyChannel<CounterPageModel*Cmd<CounterPageMsg>>
