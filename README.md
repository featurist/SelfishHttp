# Selfish HTTP

Because at some point, you'll want your own HTTP server.

## Installation

    PM> Install-Package SelfishHttp

## Lets See

SelfishHttp is an easy to use HTTP server that you can configure with regular C# code. Great for mocking out real HTTP servers in tests.

### GET

    using SelfishHttp;

    ...

    var server = new Server(4567);
    server.OnGet("/").RespondWith("hi, this is selfish HTTP!");

### POST

    var server = new Server(4567);
    server.OnPost("/").Respond((req, res) => {
        var requestBody = req.BodyAs<string>();
        ...
        res.Headers["Location"] = "/newthingo";
        res.Body = "all done";
    });

### Loads of other stuff

It supports:

* All the verbs: GET, PUT, POST, DELETE, OPTIONS, HEAD. Any others?
* Basic Authentication.
* [CORS](http://en.wikipedia.org/wiki/Cross-origin_resource_sharing).
* Stream bodies.
* An [connect](http://www.senchalabs.org/connect/)-like handler interface, for injecting HTTP handlers into the request/response pipeline.
* An expressive builder interface for building up handlers.
* Extensible body parsers and writers for different content types.

See the [tests](https://github.com/featurist/SelfishHttp/tree/master/SelfishHttp.Test) for examples.
