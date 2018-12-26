module Main
open Newtonsoft.Json

[<JsonObject(MemberSerialization.OptIn)>]
type Field(title: string, value: string, short: bool) = class
    [<JsonProperty("title")>]
    member me.Title = title
    [<JsonProperty("value")>]
    member me.Value = value
    [<JsonProperty("short")>]
    member me.Short = short
end

[<JsonObject(MemberSerialization.OptIn)>]
type Action(typeStr: string, text: string, url:string, style:string) = class
    [<JsonProperty("type")>]
    member me.Type = typeStr
    [<JsonProperty("text")>]
    member me.Text = text
    [<JsonProperty("url")>]
    member me.Url = url
    [<JsonProperty("style")>]
    member me.Style = style
end

[<JsonObject(MemberSerialization.OptIn)>]
type Attachment(fallback: string, 
                color:string, 
                preText:string,
                authorLink:string,
                authorName:string,
                authorIcon:string,
                title:string,
                titleLink:string,
                text:string,
                imageUrl:string,
                fields: Field list, 
                footer:string,
                footerIcon:string,
                timestamp: int64,
                markdownIn:string list, 
                actions: Action list,
                callbackId:string,
                thumbnailUrl:string) = class
    [<JsonProperty("fallback")>]
    member me.Fallback = fallback
    [<JsonProperty("color")>]
    member me.Color = color
    [<JsonProperty("pretext")>]
    member me.Pretext = preText
    [<JsonProperty("author_name")>]
    member me.AuthorName = authorName
    [<JsonProperty("author_link")>]
    member me.AuthorLink = authorLink
    [<JsonProperty("author_icon")>]
    member me.AuthorIcon = authorIcon
    [<JsonProperty("title")>]
    member me.Title = title
    [<JsonProperty("title_link")>]
    member me.TitleLink = titleLink
    [<JsonProperty("text")>]
    member me.Text = text
    [<JsonProperty("image_url")>]
    member me.ImageUrl = imageUrl
    [<JsonProperty("fields")>]
    member me.Fields = fields
    [<JsonProperty("footer")>]
    member me.Footer = footer
    [<JsonProperty("footer_icon")>]
    member me.FooterIcon = footerIcon
    [<JsonProperty("ts")>]
    member me.Timestamp = timestamp
    [<JsonProperty("markdwn_in")>]
    member me.Markdowns = markdownIn
    [<JsonProperty("actions")>]
    member me.Actions = actions
    [<JsonProperty("callback_id")>]
    member me.CallbackId = callbackId
    [<JsonProperty("thumb_url")>]
    member me.ThumbnailUrl = thumbnailUrl
end

type Payload(parse: string option,
                username: string option,
                iconUrl: string option,
                iconEmoji:string option,
                channel:string option,
                text:string option,
                linkNames:string option,
                unfurlLinks: bool option,
                unfurlMedia: bool option,
                isMarkdown:bool option) = class
    [<JsonProperty("parse")>]
    member me.Parse = parse
    [<JsonProperty("username")>]
    member me.Username = username
    [<JsonProperty("icon_url")>]
    member me.IconUrl = iconUrl
    [<JsonProperty("icon_emoji")>]
    member me.IconEmoji = iconEmoji
    [<JsonProperty("channel")>]
    member me.Channel = channel
    [<JsonProperty("text")>]
    member me.Text = text
    [<JsonProperty("link_names")>]
    member me.LinkNames = linkNames
    [<JsonProperty("unfurl_links")>]
    member me.UnfurlLinks = unfurlLinks
    [<JsonProperty("unfurl_media")>]
    member me.UnfurlMedia = unfurlMedia
    [<JsonProperty("mrkdwn")>]
    member me.IsMarkdown = isMarkdown
end

module SlackWebhooks = 
    type ApiResponse = {Body:string; StatusCode:int}
    type Response = {Success: bool; Error: string option}
    open HttpFs.Client
    open Hopac

    let captureBodyAndStatusCode statusCode responseBody: ApiResponse =
        {Body=responseBody; StatusCode=statusCode}
    let apiResponse2Response apiResponse = 
        match apiResponse.StatusCode with
        | x when x < 300 -> {Success=false; Error=Some(apiResponse.Body)}
        | x -> {Success=true; Error=None}

    let SendAsync webhookUrl payload = 
        Request.createUrl Post webhookUrl
                    |> Request.bodyString (JsonConvert.SerializeObject(payload))
                    |> Request.setHeader (ContentType (ContentType.create("application", "json")))
                    |> getResponse
                    |> Alt.afterJob(fun resp ->
                        resp 
                            |> Response.readBodyAsString 
                            |> Job.map (captureBodyAndStatusCode resp.statusCode)
                            |> Job.map apiResponse2Response
                    )
                    |> Hopac.run
