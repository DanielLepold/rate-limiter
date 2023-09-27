module Test.FixedWindowCounterTest

open System
open System.Threading
open NUnit.Framework
open RateLimiter.FixedWindowCounter

[<Test>]
let ``Fixed Window Counter Allows Requests Within Limit`` () =
    // Initialize the rate limiter with a small threshold
    let windowSizeSeconds = 5
    let threshold = 3  // For testing purposes, use a low threshold
    let rateLimiter = FixedWindowCounter(windowSizeSeconds, threshold)


    // Make requests with the limit
    for i in 1..threshold do
        Assert.True(rateLimiter.IsRequestAllowed())
        Console.WriteLine($"Loop count: {i}")
        Console.WriteLine($"Counter before subtract: {rateLimiter.GetCounter()}")
        rateLimiter.SubtractCounter()
        Console.WriteLine($"Counter after subtract: {rateLimiter.GetCounter()}")
        rateLimiter.Increment()
        Console.WriteLine($"Counter after increment: {rateLimiter.GetCounter()}")

    // Counter is empty now
    Assert.False(rateLimiter.IsRequestAllowed())
    // Wait for windowSizeSeconds and refill counter
    Thread.Sleep(windowSizeSeconds * 1000)
    rateLimiter.Increment()
    Console.WriteLine($"Counter after sleep and increment: {rateLimiter.GetCounter()}")
    // Counter is refilled
    Assert.True(rateLimiter.IsRequestAllowed())
