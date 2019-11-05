module DataAccess
open Models
open Models.DomainModels
open ElevatedHelpers
open System

type IBookingService = 
    abstract GetBookingDetails: BookingReference -> Async<Result<BookingDetails, string>>

type MockBookingService() = 
    interface IBookingService with
        member __.GetBookingDetails _ = 
            {
                Passengers = [
                    {
                        FirstName = FirstName "John"
                        LastName = LastName "Smith"
                    }
                    {
                        FirstName = FirstName "Jane"
                        LastName = LastName "Smith"
                    }
                ]
                FlightDate = DateTime.Now
                FlightNumber = FlightNumber "BA123"
            }
            |> Ok 
            |> Async.retn