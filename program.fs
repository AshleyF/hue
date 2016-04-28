open System
open System.IO
open System.Text
open System.Net
open System.Threading

let http (url: string) meth (post: string) =
    let request = WebRequest.Create url
    request.Method <- meth
    if post.Length > 0 then
        let byteArray = Encoding.UTF8.GetBytes(post)
        request.ContentLength <- int64 byteArray.Length
        use data = request.GetRequestStream()
        data.Write(byteArray, 0, byteArray.Length)
        data.Close()
    use response = request.GetResponse()
    use reader = new StreamReader(response.GetResponseStream())
    reader.ReadToEnd()

let getLights host id = http (sprintf "http://%s/api/%s/lights" host id) "GET" ""

let setLight host id num on b s h =
    let url = sprintf "http://%s/api/%s/lights/%i/state" host id num
    let put = sprintf "{ \"on\": %b, \"bri\": %i, \"sat\": %i, \"hue\": %i }" on b s h
    http url "PUT" put |> ignore

// ----------------------------------------------------------------------------------------------------

let host = "10.0.0.37" // https://www.meethue.com/api/nupnp
let id = "36f917875c54d9ff610a52c844d2529" // http://www.developers.meethue.com/documentation/getting-started

printfn "Getting lights status..."
getLights host id |> printfn "Lights: %s"

let random num =
    let set = setLight host id num true
    let rand = new Random()
    while true do
        let h = rand.Next 65525
        let s = rand.Next 255
        let b = rand.Next 255
        printfn "Hue: %i Saturation: %i Brightness: %i" h s b
        set b s h
        Thread.Sleep 100

let pulse num hue =
    let set b =
        printfn "Brightness: %i" b
        setLight host id num true b 255 hue
    while true do
        for b in 0 .. 255 do set b
        for b in 255 .. -1 .. 0 do set b

let rainbow num =
    while true do
        for h in 0 .. 100 .. 65535 do
            printfn "Hue: %i" h
            setLight host id num true 255 255 h

let saturate num hue =
    let set s =
        printfn "Saturation %i" s
        setLight host id num true 255 s hue
    while true do
        for s in 0 .. 255 do set s
        for s in 255 .. -1 .. 0 do set s

let control num =
    let rec control' h s b on =
        let h = max 0 (min 65535 h)
        let s = max 0 (min 255 s)
        let b = max 0 (min 255 b)
        printfn "Control: on=%b hue=%i sat=%i bright=%i" on h s b
        setLight host id num on b s h
        match Console.Read() |> char with
        | 'h' -> control' (h - 500) s b on
        | 'H' -> control' (h + 500) s b on
        | 's' -> control' h (s - 5) b on
        | 'S' -> control' h (s + 5) b on
        | 'b' -> control' h s (b - 5) on
        | 'B' -> control' h s (b + 5) on
        | 'o' -> control' h s b (not on)
        | 'q' -> ()
        | k -> printfn "Unknown key: %c" k
               control' h s b on
    control' 0 255 255 true

printfn "\n\n"
printfn """Demos
0 Random
1 Pulse
2 Rainbow
3 Saturate
4 White
5 Tracey's Favorite
9 Control"""
let light = 3
match Console.Read() |> char with
| '0' -> random light
| '1' -> pulse light 0
| '2' -> rainbow light
| '3' -> saturate light 0
| '4' -> setLight host id light true 255 0 0
| '5' -> setLight host id light true 255 255 6000
| '9' -> control light
| _   -> printfn "No demo"

printfn "\nDone"
