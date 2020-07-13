// =============================================================================
// Section for View
// =============================================================================
module aoc2019_3.View

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms
open State

let createButton txt msg dispatch =
  View.Button(text = txt,command = (fun _ -> dispatch msg))

let createLabel txt error =
  View.Label
    (text = txt,
     horizontalTextAlignment = TextAlignment.Center,
     fontSize = FontSize.Named(NamedSize.Small),
     backgroundColor = (if error then Color.Red else Color.Blue),
     textColor = Color.White)

let createTextEntry placeholder value msg dispatch =
  View.Entry
    (placeholder = placeholder,
     text = value,
     textChanged = (fun args -> dispatch(msg args.NewTextValue)))

let layout model dispatch =
  View.StackLayout
    (verticalOptions = LayoutOptions.FillAndExpand,
     horizontalOptions = LayoutOptions.FillAndExpand,
     padding = Thickness 10.,
     children =
       [ createLabel "Advent of Code 2019, Problem 3" model.ui.error
         createTextEntry "Path 1" model.ui.input1 Msg.UiChangeInput1 dispatch
         createTextEntry "Path 2" model.ui.input2 Msg.UiChangeInput2 dispatch
         createButton "Create Paths" Msg.CreatePaths dispatch
         createLabel ("Path 1: " + model.pathOne.ToString()) model.ui.error
         createLabel ("Path 2: " + model.pathTwo.ToString()) model.ui.error
         createButton "Create Wires" Msg.CreateWires dispatch
         createLabel ("Wire 1: " + model.wireOne.ToString()) model.ui.error
         createLabel ("Wire 2: " + model.wireTwo.ToString()) model.ui.error
         createButton "Find Intersections" Msg.GetAllIntersectionPoints dispatch 
         View.Editor(text = sprintf "%s" (model.intersectionPoints.ToString()), isEnabled = false, autoSize= EditorAutoSizeOption.TextChanges)
       ])

let view model dispatch =
  View.ContentPage(
    content = View.ScrollView(
                                  content = layout model dispatch,
                                  verticalOptions = LayoutOptions.FillAndExpand,
                                  horizontalOptions = LayoutOptions.FillAndExpand
  )
  )
