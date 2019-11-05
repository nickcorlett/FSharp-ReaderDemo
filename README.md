# ReaderDemo

A [Giraffe](https://github.com/giraffe-fsharp/Giraffe) web application, which has been created via the `dotnet new giraffe` command.

This is a web application that was developed in order to demonstrate a functional approach to dependency injection using the Reader Monad.

This application pretends to implement the server-side behaviour of an airline booking reference system. The following fictional requirements describe the intended behavior:
* The last name and booking reference will be provided via query params
* The last name must not contain numbers or whitespace
* The booking reference must match the regex pattern in configuration (stored in appsettings.json)
* A registered service IBookingService will return the booking details associated with a specific reference
* If the booking details contains a person with the last name supplied they will be returned

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
