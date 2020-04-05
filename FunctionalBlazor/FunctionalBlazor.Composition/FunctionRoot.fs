namespace FunctionalBlazor.Composition

open System
open FunctionalBlazor
open FunctionalBlazor.Common.Ioc
open FunctionalBlazor.Common
open FunctionalBlazor.Program
open FunctionalBlazor.Caching.CachingTypes
open System.Collections.Generic
open FunctionalBlazor.Program.ProgramTypes
open System.Threading.Tasks
    
// WARNING Don't inject anything here which ultimately depends on IFunctionRoot as it will obviously fail!
type FunctionRoot 
    (programCache : ICache<ProgramModel>) = 
        
    // Holds all composed funcs that need to be injected into class constructors.
    // This is mainly application services for API Controllers and the Program mailbox for Pages.
    // They are injected into wrapper classes defined in the respective application service modules
    // or, in the case of IProgram, directly into the razor page.
    // The wrapper classes are registered with ASP.Net Core's DI in their project's Startup module.
    let container = new Dictionary<string,obj>() :> IDictionary<string,obj>
        
   
    // Compose and Initalise Program

    let program =
        Program.update programCache (Program.init (string (Guid.NewGuid())))
    
    do program.Post(ProgramMsg.Init)

    do container.Add(ContainerTags.IProgramFunc, (box program))


    // Inject into all function-wrapping classes so they can resolve composed functions
    // Registered at Startup as Scoped so each user's Circuit gets a unique instance.
    interface IFunctionRoot with
        member this.Functions = container
        

        

