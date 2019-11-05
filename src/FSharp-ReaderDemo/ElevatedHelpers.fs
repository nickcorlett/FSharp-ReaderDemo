module ElevatedHelpers

[<AutoOpen>]
module Result = 
    let retn = Ok

    let apply fResult xResult =
        match fResult,xResult with
        | Ok f, Ok x                -> Ok (f x)
        | Error errs, Ok x          -> Error errs
        | Ok f, Error errs          -> Error errs
        | Error errs1, Error errs2  -> Error errs1 

[<AutoOpen>]
module Async = 
    let map f xAsync = async {
        let! x = xAsync 
        return f x
        }

    let retn x = async {
        return x
        }

    let bind f xAsync = async {
        let! x = xAsync 
        return! f x
        }

[<AutoOpen>]
module AsyncResult = 
    type AsyncResult<'a, 'b> = Async<Result<'a, 'b>>
    let map f = f |> Result.map |> Async.map

    let retn x = x |> Result.retn |> Async.retn

    let apply fAsyncResult xAsyncResult = 
        fAsyncResult |> Async.bind (fun fResult -> 
        xAsyncResult |> Async.map (fun xResult -> 
        Result.apply fResult xResult))
            
    let bind f xAsyncResult = async {
        let! xResult = xAsyncResult 
        match xResult with
        | Ok x -> return! f x
        | Error err -> return (Error err)
        }

[<AutoOpen>]
module Reader = 
    type Reader<'environment,'a> = Reader of ('environment -> 'a)
    
    let run environment (Reader action) = 
        let resultOfAction = action environment
        resultOfAction

    let map f action = 
        let newAction environment =
            let x = run environment action 
            f x
        Reader newAction

    let retn x = 
        let newAction environment = x
        Reader newAction

    let apply fAction xAction = 
        let newAction environment =
            let f = run environment fAction 
            let x = run environment xAction 
            f x
        Reader newAction

    let bind f xAction = 
        let newAction environment =
            let x = run environment xAction 
            run environment (f x)
        Reader newAction 

[<AutoOpen>]
module ReaderAsync = 
    let retn x =
        Reader.retn (Async.retn x)

[<AutoOpen>]
module ReaderAsyncResult = 
    let map f = 
        Reader.map (AsyncResult.map f)
        
    let retn x = 
        Reader.retn (AsyncResult.retn x)

    let apply fActionAsyncResult xActionAsyncResult = 
        let newAction environment = 
                Reader.run environment fActionAsyncResult 
                |> Async.bind (fun fResult ->
                    Reader.run environment xActionAsyncResult
                    |> Async.map (fun xResult -> 
                        Result.apply fResult xResult))
        Reader newAction
        
    let bind (f: 'a -> Reader<'b,Async<Result<'c,'d>>>) xActionResult : Reader<'b,Async<Result<'c,'d>>> =
        let newAction environment =
            let xAsyncResult = Reader.run environment xActionResult
            AsyncResult.bind (fun a -> f a |> run environment) xAsyncResult
        Reader newAction

    type ReaderAsyncResultBuilder() = 
        member this.Bind(x, f) = bind f x
        member this.Return(x) = retn x
        member this.ReturnFrom(x) = x

    let readerAsyncResult = ReaderAsyncResultBuilder()

    let (>>=) x f = bind f x
    let (|>>) x f = map f x  
    let (<!>) = map
    let (<*>) = apply 