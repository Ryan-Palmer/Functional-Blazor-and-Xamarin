namespace FunctionalBlazor.Caching

module CachingTypes =
    open System
    open System.Threading.Tasks


    type EventBus<'T> () = 
        
        let _event = new Event<_>()
        let _onPublish = _event.Publish

        [<CLIEvent>]
        member this.OnPublish with get() = _onPublish
        member this.Invoke(arg : 'T) = _event.Trigger(this, arg)


    type CacheCommand<'T> =
        | Update of 'T option
        | Fetch
        | Subscribe of (obj * 'T option -> unit)
    
    type CacheResult<'T> =
        | Updated
        | Subscribed of IDisposable
        | Cache of 'T option

    type CacheOperation<'T> = CacheCommand<'T> * AsyncReplyChannel<CacheResult<'T>>

    type ICache<'T> =
        abstract member Post : CacheCommand<'T> -> Task<CacheResult<'T>>
    
    


    type IMessageHandler<'TArgs> = obj * 'TArgs -> unit
    
    type MessageCommand<'T> =
        | Subscribe of IMessageHandler<'T>
        | Send of 'T
    
    type MessageResult =
        | MessageSent
        | Subscribed of IDisposable
    
    type MessageOperation<'T> = MessageCommand<'T> * AsyncReplyChannel<MessageResult>
        
    type IMessenger<'T> =
        abstract member Post : MessageCommand<'T> -> Task<MessageResult>
    