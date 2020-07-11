// =============================================================================
// Section for View
// =============================================================================
module  aoc2019_3.View

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms
open State

let createButton txt msg dispatch =
  View.Button(text = txt,command = (fun _ -> dispatch msg ))

let createLabel txt =
  View.Label(text = txt, horizontalTextAlignment = TextAlignment.Center, margin = Thickness 10.,
    fontSize = FontSize.Named(NamedSize.Large))

let createTextEntry placeholder value msg dispatch =
  View.Entry(placeholder=placeholder,text=value,
    textChanged=(fun args -> dispatch (msg args.NewTextValue)))
  

let layout model dispatch =
  View.StackLayout(
    verticalOptions = LayoutOptions.CenterAndExpand,
    padding=Thickness 20.,
    children=[
      createLabel "Advent of Code 2019, Problem 3"
      createTextEntry "Path 1" model.ui.input1 Msg.UiChangeInput1 dispatch
      createTextEntry "Path 2" model.ui.input2 Msg.UiChangeInput2 dispatch
      createButton "Create Paths" Msg.CreatePaths dispatch
      createButton "Create Wires" Msg.CreateWires dispatch
    ]
  )

let view model dispatch =
  View.ContentPage(content=layout model dispatch 
  )
