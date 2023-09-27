module Test.TokenBucketTest

open System
open System.Net
open System.Threading
open NUnit.Framework
open RateLimiter.TokenBucket


[<Test>]
let ``Token Bucket Allows Requests Within Limit`` () =

    let ipAddress = new IPAddress [|192uy; 168uy; 1uy; 1uy|]
    // Initialize the token parameters with small values
    let tokenCapacity = 10 //int
    let refillRate = 2 //int - second

    // Check if there's a token bucket for this IP address; create one if not
    let bucket = getBucket ipAddress tokenCapacity refillRate


    // Consume N tokens with the limit
    for i in 1..tokenCapacity do
        Console.WriteLine($"Loop count: {i}")
        Console.WriteLine($"Token count before consume: {bucket.GetToken()}")
        Assert.True(bucket.TryConsumeToken())

    // Token is empty
    Assert.False(bucket.TryConsumeToken())
    // Wait for refill
    Thread.Sleep(refillRate * 1000)
    Assert.True(bucket.TryConsumeToken())

