namespace FunctionalBlazor.Program

module ProgramTypes =

    type NavDestination = string

    type ProgramModel =
        {
            Id : string
        }
        
    type ProgramMsg =
        | Init
        | ProgramError of exn

    type IProgramFunc = MailboxProcessor<ProgramMsg>
    type IProgram =
        abstract member Post : ProgramMsg -> unit

