namespace FunctionalBlazor.Common

module Ioc =
    open System
    open Microsoft.Extensions.DependencyInjection
    open System.Collections.Generic
   
    type IScopedService<'service> =
        inherit IDisposable
        abstract member Instance : 'service

    type ScopedService<'service> (rootProvider : IServiceProvider) =
        
        let serviceScope : IServiceScope = rootProvider.CreateScope()
        let mutable disposed = false

        interface IScopedService<'service> with
            member this.Instance = serviceScope.ServiceProvider.GetRequiredService<'service>()
            member this.Dispose () =
                this.Dispose(true) |> ignore
                GC.SuppressFinalize(this)

        abstract member Dispose : bool -> unit
        default this.Dispose (disposing : bool) =
            if disposed
            then ()
            else 
                if disposing
                then serviceScope.Dispose()
                disposed <- true

    type IScopedServiceFactory<'a> = Func<IScopedService<'a>>

    let createScopedServiceFactory<'a> serviceProvider =
        new Func<IScopedService<'a>> (fun () ->
            new ScopedService<'a> (serviceProvider) 
            :> IScopedService<'a>)

    type IFunctionRoot = 
        abstract member Functions : IDictionary<string, obj>

