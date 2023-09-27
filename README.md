# rate-limiter

## General introduction
Rate limiters are a critical component in the development of APIs and large-scale distributed systems.
They serve as a mechanism to control and manage incoming traffic based on user interactions.
Rate limiting strategies play a vital role in enhancing the reliability of your system in various scenarios:

# Token Bucket rate limiter
Is a rate limiting algorithm used in computer networking and distributed systems
to control the rate of incoming requests or events.
It operates on the principle of tokens being added to a virtual "bucket" at a fixed rate
and the bucket having a limited capacity.

Here's how it works:

### Token Generation:
Tokens are generated and added to the token bucket at a constant rate.
For example, a token might be added to the bucket every second.
### Bucket Capacity:
The token bucket has a fixed capacity, which represents the maximum number of tokens
that it can hold at any given time.
### Request Acceptance:
When a request or event arrives, it can only be accepted if there are enough tokens in the bucket.
Each request consumes one token from the bucket when accepted.
### Rate Limiting:
If the bucket is full (reached its capacity), any additional tokens generated are discarded.
This enforces a maximum rate at which requests can be accepted.
### Token Consumption:
When a request is accepted and processed, one token is removed from the bucket.
If there are no tokens in the bucket when a request arrives, it's either delayed until there are enough tokens or rejected,
depending on the implementation.

# Fixed window size algorithm
The fixed window-sized rate limiter algorithm is designed to control the rate of incoming requests
within a defined time window. Here's how it works:

### Time Window:
A fixed time window of N seconds is established to monitor the request rate.
### Request Count:
Each incoming request increments a counter associated with the current time window.
#### Threshold Check:
If the counter exceeds a predefined threshold, the incoming request is rejected or discarded.

# Usage
Select on of the algorithms which can be seen in Common.fs, and adjust it in the Program.fs main function.
```
RateLimiterAlgorithm
```
Adjust your test parameters at the beginning of the TokenBucket.fs or FixedWindowCounter.fs files. 

Run the applications at your ip and port definitions in the main.
You can test at the example endpoint:
```
http://localhost:8080/limited
```
### 
Unit test were written for testing the token bucket algorithm and also for the fixed window size algorithm.