namespace RateLimiter
open System
open System.Net
open System.Net.Sockets
open RateLimiter.Common

module Program =

    [<EntryPoint>]
    let main args =
        let ipAddress = IPAddress.Parse("127.0.0.1")
        let port = 8080
        let listener = new TcpListener(ipAddress, port)
        listener.Start()
        Console.WriteLine($"Server is listening on port {port}...")

        let rateLimiterType = RateLimiterAlgorithm.FixedWindowCounter

        match rateLimiterType with
        | TokenBucket ->  TokenBucket.handleClient listener
        | FixedWindowCounter -> FixedWindowCounter.handleClient listener

        0 // Exit code

