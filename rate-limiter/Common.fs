namespace RateLimiter
module Common =
  let getRequestPath (requestLine: string) =
    let parts = requestLine.Split [|' '|]
    if parts.Length >= 2 then
        parts.[1]
    else
        "/"

  type RateLimiterAlgorithm =
    | TokenBucket
    | FixedWindowCounter
