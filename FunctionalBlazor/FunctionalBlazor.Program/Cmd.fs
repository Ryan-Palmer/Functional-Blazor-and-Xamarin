namespace FunctionalBlazor.Program

/// Dispatch - feed new message into the processing loop
type Dispatch<'msg> = 'msg -> unit

/// Subscription - return immediately, but may schedule dispatch of a message at any time
type Sub<'msg> = Dispatch<'msg> -> unit

/// Cmd - container for subscriptions that may produce messages
type Cmd<'msg> = Sub<'msg> list

module Cmd =
    open System.Threading.Tasks

    /// Execute the commands using the supplied dispatcher
    let exec (dispatch:Dispatch<'msg>) (cmd:Cmd<'msg>)=
        cmd |> List.iter (fun sub -> sub dispatch)

    /// None - no commands, also known as `[]`
    let none : Cmd<'msg> =
        []

    /// Command that will evaluate an async block and map the result
    /// into success or error (of exception)
    let ofAsync (task: 'a -> Async<_>) 
                (arg: 'a) 
                (ofSuccess: _ -> 'msg) 
                (ofError: _ -> 'msg) : Cmd<'msg> =
        let bind (dispatch : Dispatch<'msg>) =
            async {
                let! r = task arg |> Async.Catch
                dispatch 
                    (match r with
                    | Choice1Of2 x -> ofSuccess x
                    | Choice2Of2 x -> ofError x)
            }
        [bind >> Async.StartImmediate]


    /// Command that will evaluate an async Result block and map the result
    /// into success or error
    let ofAsyncResult (task: 'a -> Async<Result<_,_>>) 
                (arg: 'a) 
                (ofSuccess: _ -> 'msg) 
                (ofError: _ -> 'msg) : Cmd<'msg> =
        let bind (dispatch : Dispatch<'msg>) =
            async {
                let! r = task arg
                dispatch 
                    (match r with
                    | Ok x -> ofSuccess x
                    | Error e -> ofError e)
            }
        [bind >> Async.StartImmediate]


    /// Command to issue a specific message
    let ofMsg (msg:'msg) : Cmd<'msg> =
        [fun dispatch -> dispatch msg]

    /// When emitting the message, map to another type
    let map (f: 'a -> 'msg) (cmd: Cmd<'a>) : Cmd<'msg> =
        cmd |> List.map (fun g -> (fun dispatch -> f >> dispatch) >> g)

    /// Aggregate multiple commands
    let batch (cmds: #seq<Cmd<'msg>>) : Cmd<'msg> =
        cmds |> List.concat

    /// Command to evaluate a simple function and map the result
    /// into success or error (of exception)
    let ofFunc (task: 'a -> _) (arg: 'a) (ofSuccess: _ -> 'msg) (ofError: _ -> 'msg) : Cmd<'msg> =
        let bind dispatch =
            try
                task arg
                |> (ofSuccess >> dispatch)
            with x ->
                x |> (ofError >> dispatch)
        [bind]

    /// Command to evaluate a simple function and dispatch the result
    let ofFuncResult (task: 'a -> _) (arg: 'a) (ofSuccess: _ -> 'msg) (ofError: _ -> 'msg) : Cmd<'msg> =
        let bind dispatch =
            match task arg with
            | Ok x ->  x |> (ofSuccess >> dispatch)
            | Error e -> e |> (ofError >> dispatch)
        [bind]

    /// Command to evaluate a simple function and map the success to a message
    /// discarding any possible error
    let performFunc (task: 'a -> _) (arg: 'a) (ofSuccess: _ -> 'msg) : Cmd<'msg> =
        let bind dispatch =
            try
                task arg
                |> (ofSuccess >> dispatch)
            with x ->
                ()
        [bind]

    /// Command to evaluate a simple function and map the error (in case of exception)
    let attemptFunc (task: 'a -> unit) (arg: 'a) (ofError: _ -> 'msg) : Cmd<'msg> =
        let bind dispatch =
            try
                task arg
            with x ->
                x |> (ofError >> dispatch)
        [bind]

    /// Command to call the subscriber
    let ofSub (sub: Sub<'msg>) : Cmd<'msg> =
        [sub]

    let inline ofTask (task: 'a -> Task<_>) 
                      (arg:'a) 
                      (ofSuccess: _ -> 'msg) 
                      (ofError: _ -> 'msg) : Cmd<'msg> =
        ofAsync (task >> Async.AwaitTask) arg ofSuccess ofError

