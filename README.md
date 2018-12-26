# fsharp-slack-webhooks

F# library to send messages to Slack via Incoming webhooks.

## Usage
```fsharp
open SlackWebhooks.Client
[<EntryPoint>]
let main args =
    let payload = new Payload(text="Hello from F#",username="fsharp-bot",channel="#general")
    let response = Send "https://hooks.slack.com/services/foo/bar/baz"  payload
    printfn "Response : %A" response
    // Return 0. This indicates success.
    0
```

## License
Licensed under the Apache License, Version 2.0: http://www.apache.org/licenses/LICENSE-2.0
