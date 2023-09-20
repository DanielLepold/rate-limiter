open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

let getRequestPath (requestLine: string) =
        let parts = requestLine.Split [|' '|]
        if parts.Length >= 2 then
            parts.[1]
        else
            "/"
let asyncHandleClient (client: TcpClient) =
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
                // Simulate a 4-second delay
                //do! Async.Sleep(TimeSpan.FromSeconds(4.0))

                let path = getRequestPath requestLine
                let response =
                    match path with
                    | "/unlimited" -> "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUnlimited! Let's Go!"
                    | "/limited" -> "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\nLimited, don't over use me!"
                    | _ -> "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n404 - Not Found"

                Console.WriteLine($"Thread: {threadId} is sending the response back to the client... {response}.")
                writer.WriteLine(response)
                writer.Flush()
        with
            | ex -> Console.WriteLine($"An error occurred: {ex.Message}")

        Console.WriteLine("Closing client...")
        client.Close()
    }

[<EntryPoint>]
let main args =
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let port = 8080
    let listener = new TcpListener(ipAddress, port)
    listener.Start()
    Console.WriteLine($"Server is listening on port {port}...")

    while true do
        let client = listener.AcceptTcpClient()
        async {
            // Handle each client asynchronously
            let! _ = Async.StartChild(asyncHandleClient client)
            return ()
        } |> Async.Start

    0 // Exit code
