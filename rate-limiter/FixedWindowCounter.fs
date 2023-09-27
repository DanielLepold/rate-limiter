namespace RateLimiter

open System
open System.IO
open System.Net.Sockets
open System.Threading
open RateLimiter.Common

module FixedWindowCounter =
  // Define a single window counter for all incoming requests
  // Adjust your values as your needs
  let windowSizeSeconds = 60
  let threshold = 10

   // Define a data structure to represent the fixed window counter
  type FixedWindowCounter(windowSizeSeconds: int, threshold: int) =
    let mutable windowStart =  DateTime.UtcNow
    let mutable counter = threshold

    member this.Increment() =
        let now = DateTime.UtcNow
        if (now - windowStart).TotalSeconds >= float windowSizeSeconds then
            // Move to the next time window
            windowStart <- now
            counter <- threshold

    member this.SubtractCounter() =
      counter <- max (counter - 1) 0
    member this.IsRequestAllowed() =
        counter > 0

    member this.GetCounter() =
          counter
  let windowCounter = FixedWindowCounter(windowSizeSeconds, threshold)
  let asyncHandleClient (client: TcpClient)=
    async {
        let threadId = Thread.CurrentThread.ManagedThreadId
        Console.WriteLine($"Thread: {threadId} is handling the client.")

        let stream = client.GetStream()
        let reader = new StreamReader(stream)
        let writer = new StreamWriter(stream)

        try
            let requestLine = reader.ReadLine()
            match requestLine with
            | null ->
                Console.WriteLine("Received an empty request.")
            | _ ->

            // Increment the window counter for this request if possible
            windowCounter.Increment()

            let path = getRequestPath requestLine
            let response =
                match path with
                | "/unlimited" -> $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUnlimited! Let's Go!"
                | "/limited" ->
                  match windowCounter.IsRequestAllowed() with
                  | false ->
                    // Request exceeds the threshold; discard it
                    "HTTP/1.1 429 Too Many Requests\r\nContent-Type: text/plain\r\n\r\n429 - Too Many Requests"
                  | true ->
                    windowCounter.SubtractCounter()
                    $"HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\nLimited, don't overuse me! Counter: {windowCounter.GetCounter()}"
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
    Console.WriteLine("Fixed window counter algorithm has started.")
    while true do
        let client = listener.AcceptTcpClient()
        async {
            let! _ = Async.StartChild(asyncHandleClient client)
            return ()
        } |> Async.Start

