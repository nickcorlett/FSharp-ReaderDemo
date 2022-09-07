module Handler
open System
open System.Text.RegularExpressions
open Models
open DataAccess
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open ElevatedHelpers
open WorkflowWithReader.BookingDetailsContext
open Models.Dtos
open DataAccess
open Microsoft.Extensions.Options
open Models.Config

module GetBookingDetails = 
    let getBookingDetailsHandlerNoReader : HttpHandler = 
        fun (next:HttpFunc) (ctx:HttpContext) ->
        task {
            let! response = WorkflowWithoutReader.FindBookingDetails.findBookingDetails ctx 
            match response with
            | Ok details -> return! (json details) next ctx
            | Error err -> return! (ServerErrors.internalError (text err)) next ctx            
        }

    let getBookingDetailsHandlerReader : HttpHandler = 
        fun (next:HttpFunc) (ctx:HttpContext) ->
        task {
            let bookingDetailsContext = {
                GetDtoRequest = (fun () -> ctx.TryBindQueryString<DtoGetBookingDetailsRequest>() |> Async.retn )
                GetRegexConfig = ctx.GetService<IOptions<ReferenceConfiguration>>
                GetBookingService = ctx.GetService<IBookingService> >> AsyncResult.retn
            }

            let! response = WorkflowWithReader.FindBookingDetails.findBookingDetails |> Reader.run bookingDetailsContext 
            match response with
            | Ok details -> return! (json details) next ctx
            | Error err -> return! (ServerErrors.internalError (text err)) next ctx            
        }    
