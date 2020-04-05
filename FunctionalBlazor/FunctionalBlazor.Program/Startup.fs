namespace FunctionalBlazor.Program

open Microsoft.Extensions.DependencyInjection
open ProgramTypes
open Program
open System.Runtime.CompilerServices

[<Extension>]
type Startup  () =
        
    [<Extension>]
    static member inline ConfigureProgram (services : IServiceCollection) =
        services.AddScoped<IProgram, Program>() |> ignore