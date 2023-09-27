namespace RateLimiter

open System
open System.Collections.Generic
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading
open RateLimiter.Common

module TokenBucket =
  // Define your token parameters here
  let getTokenCapacity = 10
  let getRefillRate = 1
  // Define a data structure to represent the token bucket per IP address
  type TokenBucket(ipAddress: IPAddress, capacity: int, refillRate: int) =
    let mutable tokens = capacity
    let mutable lastRefillTimestamp =  DateTime.UtcNow

    member this.TryConsumeToken() =
        let now = DateTime.UtcNow
        let timeElapsed = int (abs ((now - lastRefillTimestamp).TotalSeconds))

        Console.WriteLine($"Elapsed time: {timeElapsed}")
        Console.WriteLine($"Current token at: {tokens}, at ip address: {ipAddress}")
        // Refill tokens based on the elapsed time
        if timeElapsed >= 1 then
            lastRefillTimestamp <- now
            tokens <- min (tokens + refillRate * timeElapsed) capacity

        if tokens > 0 then
            tokens <- tokens - (int)1
            true
        else
            false
    member this.GetToken() =
      tokens

  // Create a dictionary to store token buckets for each IP address
  let tokenBuckets = new Dictionary<IPAddress, TokenBucket>()

  let getBucket (ipAddress : IPAddress) (tokenCapacity : int) (refillRate : int)=
    if tokenBuckets.ContainsKey(ipAddress) then
                tokenBuckets[ipAddress]
    else
        let newBucket = new TokenBucket(ipAddress, capacity = tokenCapacity, refillRate = refillRate)
        tokenBuckets.Add(ipAddress, newBucket)
        newBucket
  let asyncHandleClient (client: TcpClient) =
    async {
        let threadId = Thread.CurrentThread.ManagedThreadId
        Console.WriteLine($"Thread: {threadId} is handling the client.")

        Console.WriteLine($"Client remote endpoint: {client.Client.RemoteEndPoint}.")
        let ipAddress = ((client.Client.RemoteEndPoint :?> IPEndPoint)).Address

        // Check if there's a token bucket for this IP address; create one if not
        let bucket = getBucket ipAddress getTokenCapacity getRefillRate

        let stream = client.GetStream()
        let reader = new StreamReader(stream)
        let writer = new StreamWriter(stream)

        try
            let requestLine = reader.ReadLine()
            match requestLine with
            | null ->
                Console.WriteLine("Received an empty request.")
            | _ ->

            let path = getRequestPath requestLine
            let response =
                match path with
                | "/unlimited" -> $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUnlimited! Let's Go!"
                | "/limited" ->
                  match bucket.TryConsumeToken() with
                  | true ->
                    $"HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\nLimited, don't overuse me! Token count: {bucket.GetToken()}."
                  | false ->
                    "HTTP/1.1 429 Too Many Requests\r\nContent-Type: text/plain\r\n\r\n429 - Too Many Requests"
                | _ -> "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n404 - Not found"

            Console.WriteLine($"Thread: {threadId} is sending the response back to the client... {response}.")
            writer.WriteLine(response)
            writer.Flush()

        with
            | ex -> Console.WriteLine($"An error occurred: {ex.Message}")

        Console.WriteLine("Closing client...")
        client.Close()
    }

  let handleClient (listener : TcpListener) =
    Console.WriteLine("Token bucket algorithm has started.")
    while true do
        let client = listener.AcceptTcpClient()
        async {
            // Handle each client asynchronously with the window counter
            let! _ = Async.StartChild(asyncHandleClient client)
            return ()
        } |> Async.Start

