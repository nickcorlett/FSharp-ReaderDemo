namespace WorkflowWithoutReader

open System
open System.Text.RegularExpressions
open Models
open DataAccess

module Validation = 
    open ElevatedHelpers
    open DomainModels
    open Dtos
    open Config
    open Giraffe
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.Options
    
    let (>>=) x f = Result.bind f x
    let (<!>) = Result.map
    let (<*>) = Result.apply
    let (|>>) x f = Result.map f x

    let stringChecker predicate error (name:string) = 
        match name |> Seq.forall predicate with
        | true -> Ok (name)
        | false -> Error error

    let noSpaces = 
        stringChecker (string >> String.IsNullOrWhiteSpace >> not) "Name contains whitespace"

    let noNumbers = 
        stringChecker (Char.IsDigit >> not) "Name contains numbers"

    let validateName (request:DtoGetBookingDetailsRequest) = 
        noSpaces request.Last
        >>= noNumbers
        |>> LastName

    let getRegexPattern (ctx:HttpContext) = 
        match ctx.GetService<IOptions<ReferenceConfiguration>>() with
        | null -> Error "No reference configuration"
        | cfg -> Ok <| Regex(cfg.Value.regexPattern)

    let checkRegex (request:DtoGetBookingDetailsRequest) (regex:Regex) =
        match regex.IsMatch(request.Ref) with
        | true -> Ok <| BookingReference request.Ref
        | false -> Error "Request does not match configuration" 

    let validateReference (request:DtoGetBookingDetailsRequest) (ctx:HttpContext) = 
        getRegexPattern ctx
        >>= (checkRegex request)

    let validateDto (request:DtoGetBookingDetailsRequest) (ctx:HttpContext) = 
        GetBookingDetailsRequest.Create
        <!> validateName request
        <*> validateReference request ctx        

module GetBookingDetails = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open DomainModels

    let getBookingService (ctx:HttpContext) = ctx.GetService<IBookingService>()

    let callService (bookingReference:BookingReference) (service:IBookingService) = 
        service.GetBookingDetails bookingReference

    let getBookingDetailsFromService (bookingReference:BookingReference) (ctx:HttpContext) = 
        let service = getBookingService ctx
        callService bookingReference service 

    let tryFindPassengerDetails lastName bookingDetails = 
        match bookingDetails.Passengers |> List.exists (fun passenger -> passenger.LastName = lastName) with
        | true ->  Ok bookingDetails
        | false -> Error "couldn't find booking details"       

module FindBookingDetails =                                      
    open Giraffe
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks
    open DomainModels
    open Dtos
    open Validation
    open ElevatedHelpers
    open GetBookingDetails

    let getRequestFromQueryString (ctx:HttpContext) = ctx.TryBindQueryString<DtoGetBookingDetailsRequest>()

    let findBookingDetails (ctx:HttpContext) = 
        async {
            let dtoRequest = getRequestFromQueryString ctx
            match dtoRequest with
            | Ok d ->
                let request = validateDto d ctx
                match request with
                | Ok r ->
                    return! 
                        async {
                            let lastName = r.LastName
                            let bookingRef = r.BookingReference
                            let! bookingDetails = getBookingDetailsFromService bookingRef ctx
                            return                         
                                bookingDetails 
                                |> Result.bind (tryFindPassengerDetails lastName)
                        }
                | Error err -> return Error err
            | Error err -> return Error err
        }        