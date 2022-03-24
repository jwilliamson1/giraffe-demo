module RipView

open Giraffe.ViewEngine
open gir.Models

let layout (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ str "Rip And Dip" ]
        ]
        body [] content
    ]

let partial () =
    p [] [ str "Some partial text." ]

let ripView (model : Person) =
    [
        div [ _class "container" ] [
                h3 [_title "Some title attribute"] [ sprintf "Hello, %s" model.Name |> str ]
                a [ _href "It's a wonder I can still break" ] [ str "DT" ]
            ]
        div [] [ partial() ]
    ] |> layout