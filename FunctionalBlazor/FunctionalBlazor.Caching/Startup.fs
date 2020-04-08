namespace FunctionalBlazor.Caching

open Microsoft.Extensions.DependencyInjection
open FunctionalBlazor.Caching.CachingTypes
open FunctionalBlazor.Caching.Messenger
open FunctionalBlazor.Caching.Cache
open System.Runtime.CompilerServices


[<Extension>]
type Startup  () =
        
    [<Extension>]
    static member inline ConfigureCaching (services : IServiceCollection) =
        services.AddScoped(typedefof<IMessenger<_>>,typedefof<Messenger<_>>) |> ignore
        services.AddScoped(typedefof<ICache<_>>,typedefof<Cache<_>>) |> ignore