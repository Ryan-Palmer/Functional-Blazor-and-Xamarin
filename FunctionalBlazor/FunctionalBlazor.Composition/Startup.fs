namespace FunctionalBlazor.Composition

open Microsoft.Extensions.DependencyInjection
open System.Runtime.CompilerServices
open FunctionalBlazor.Common.Ioc
open FunctionalBlazor.Composition

[<Extension>]
type Startup  () =
        
    [<Extension>]
    static member inline ConfigureFunctions (services : IServiceCollection) =
        services.AddScoped<IFunctionRoot, FunctionRoot>() |> ignore