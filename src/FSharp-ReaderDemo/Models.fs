namespace Models
open System

module DomainModels = 
    type LastName = LastName of string
    type FirstName = FirstName of string
    type Person = {
        FirstName: FirstName
        LastName: LastName
    }
    type FlightNumber = FlightNumber of string
    type BookingReference = BookingReference of string

    type BookingDetails = {
        Passengers: Person list
        FlightDate: DateTime
        FlightNumber: FlightNumber
    }

    type GetBookingDetailsRequest = {
        LastName: LastName
        BookingReference: BookingReference
    }
    with static member Create last ref = { LastName = last; BookingReference = ref }

module Dtos = 
    [<CLIMutable>]
    type DtoGetBookingDetailsRequest = {
        Last: string
        Ref: string
    }

module Config = 
    [<AllowNullLiteralAttribute>]
    type ReferenceConfiguration() = 
        member val regexPattern : string = null with get, set