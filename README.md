# ReaderDemo

A [Giraffe](https://github.com/giraffe-fsharp/Giraffe) web application, which has been created via the `dotnet new giraffe` command.

This is a web application that was developed in order to demonstrate a functional approach to dependency injection using the Reader Monad.

This application pretends to implement the server-side behaviour of an airline booking reference system. The following fictional requirements describe the intended behavior:
* The last name and booking reference will be provided via query params
* The last name must not contain numbers or whitespace
* The booking reference must match the regex pattern in configuration (stored in appsettings.json)
* A registered service IBookingService will return the booking details associated with a specific reference
* If the booking details contains a person with the last name supplied they will be returned

In the application there are two examples of the above implementation, each of which demonstrates a different approach to using the HttpContext. In both cases the HttpContext is used in order to read the input from the query string, read the registered configuration, and get the implementation of IBookingService. 
The first example (WorkflowWithoutReader.fs) uses a standard approach where the HttpContext is passed down through every function that requires it. In this example the functions become tightly coupled to the HttpContext, even if they do not directly use it. 
The second example (WorkflowWithReader.fs) abstracts away the HttpContext from the main workflow and pushes it to the application boundary. In addition, the new BookingDetailsContext is passed through the workflow using the combined Reader Monad, Async, and Result elevated world so it can be accessed only when required. 

The endpoint of the first example is /bookingDetails, and the endpoint of the second example is /bookingDetailsReader. Both endpoints accept the same query params for the last name and booking reference (?last=Smith&ref=AABBCC) 

## Build and test the application

### Windows

Run the `build.bat` script in order to restore, build and test (if you've selected to include tests) the application:

```
> ./build.bat
```

### Linux/macOS

Run the `build.sh` script in order to restore, build and test (if you've selected to include tests) the application:

```
$ ./build.sh
```

## Run the application

After a successful build you can start the web application by executing the following command in your terminal:

```
dotnet run src/ReaderDemo
```

After the application has started visit [http://localhost:5000](http://localhost:5000) in your preferred browser.
