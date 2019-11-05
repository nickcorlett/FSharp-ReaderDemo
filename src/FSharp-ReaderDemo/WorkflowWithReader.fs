namespace WorkflowWithReader

open System
open System.Text.RegularExpressions
open Models
open DataAccess
open ElevatedHelpers

module Validation = 
    open DomainModels
    open Dtos
    open Config
    open Giraffe
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.Options

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
        |> Result.bind noNumbers
        |> Result.map LastName

    let validateName' = 
        validateName >> ReaderAsync.retn 

    let getRegexPattern =
        Reader (fun (ctx:HttpContext) -> 
            async {
                return 
                    match ctx.GetService<IOptions<ReferenceConfiguration>>() with
                    | null -> Error "No reference configuration"
                    | cfg -> Ok <| Regex(cfg.Value.regexPattern)
            }
        )

    let checkRegex (request:DtoGetBookingDetailsRequest) (regex:Regex) =
        match regex.IsMatch(request.Ref) with
        | true -> Ok <| BookingReference request.Ref
        | false -> Error "Request does not match configuration" 

    let checkRegex' (request:DtoGetBookingDetailsRequest) = 
        checkRegex request >> ReaderAsync.retn    

    let validateReference (request:DtoGetBookingDetailsRequest) = 
        getRegexPattern 
        >>= (checkRegex' request)

    let validateDto (request:DtoGetBookingDetailsRequest) = 
        GetBookingDetailsRequest.Create
        <!> validateName' request
        <*> validateReference request      

module GetBookingDetails = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open DomainModels
    open Dtos

    let getRequestFromQueryString = 
        Reader (fun (ctx:HttpContext) -> 
            ctx.TryBindQueryString<DtoGetBookingDetailsRequest>() |> Async.retn)

    let getBookingService = 
        Reader (fun (ctx:HttpContext) -> 
            ctx.GetService<IBookingService>() |> Ok |> Async.retn)

    let callService (bookingReference:BookingReference) (service:IBookingService) = 
        service.GetBookingDetails bookingReference

    let getBookingDetailsFromService (bookingReference:BookingReference) = 
        getBookingService
        >>= (callService bookingReference >> Reader.retn)

    let tryFindPassengerDetails lastName bookingDetails = 
        match bookingDetails.Passengers |> List.exists (fun passenger -> passenger.LastName = lastName) with
        | true ->  Ok bookingDetails
        | false -> Error "couldn't find booking details"

    let tryFindPassengerDetails' lastName bookingDetails =  
        tryFindPassengerDetails lastName bookingDetails |> ReaderAsync.retn

module FindBookingDetails = 
    open Giraffe
    open Microsoft.AspNetCore.Http
    open DomainModels
    open Dtos
    open Validation
    open GetBookingDetails

    let getRequestFromQueryString = 
        Reader (fun (ctx:HttpContext) -> 
            ctx.TryBindQueryString<DtoGetBookingDetailsRequest>() |> Async.retn)

    let getBookingDetails request = 
        getBookingDetailsFromService request.BookingReference
        >>= tryFindPassengerDetails' request.LastName

    let findBookingDetails = 
        getRequestFromQueryString
        >>= validateDto
        >>= getBookingDetails