module aoc2019_3.bolero.Client.View

open Types
open State
open Bolero
open Bolero.Html
open Zanaptak.TypedCssClasses
open Syncfusion.Blazor.Charts

type B = CssClasses<"wwwroot/css/bulma-0.9.0.min.css">
type FA = CssClasses<"wwwroot/css/fontawesome-5.14.0-all.min.css">

let createButton txt msg dispatch =
  div [ attr.``class`` B.field ] [
    div [ attr.``class`` B.control ] [
      button [ on.click (fun _ -> dispatch msg)
               attr.``class`` B.button ] [
        text txt
      ]
    ]
  ]

let createLabel txt error =
  div [ attr.``class``
        <| if error then
             B.``has-background-info``
           else
             B.``has-background-danger``
        attr.``class`` B.``has-text-centered`` ] [
    text txt
  ]

let createTextEntry lbl error value msg dispatch =
  div [ attr.``class`` B.field ] [
    label [ attr.``class`` B.label ] [
      text lbl
    ]
    div [ attr.``class`` B.control ] [
      input [ bind.input.string value (msg >> dispatch)
              attr.``class`` B.input
              attr.``type`` "text"
              attr.placeholder lbl
              attr.``class``
                (if error then B.``is-danger`` else B.``is-success``) ]
    ]
  ]

let createViewBox txt =
  div [ attr.``class`` B.field ] [
    div [ attr.``class`` B.control ] [
      textarea [ attr.``class`` B.textarea
                 attr.value txt ] []
    ]
  ]

type ChartData(x: int, y: int) =
  member this.X = x
  member this.Y = y

let viewChart (Wire.Wire (ws1)) (Wire.Wire (ws2)) intersections =
  let pts1 =
    ws1
    |> List.map (fun ws -> ws.End)
    |> List.map (fun pt -> ChartData(pt.x, pt.y))
    |> fun l -> ChartData(0, 0) :: l

  let pts2 =
    ws2
    |> List.map (fun ws -> ws.End)
    |> List.map (fun pt -> ChartData(pt.x, pt.y))
    |> fun l -> ChartData(0, 0) :: l

  comp<SfChart>
    [ "Title" => "Wires"
      "Height" => "2500" ]
    [ comp<ChartSeriesCollection>
        []
        [ comp<ChartSeries>
            [ "DataSource" => pts1
              "XName" => "X"
              "YName" => "Y"
              "Fill" => "red"
              "Type" => ChartSeriesType.Line ]
            []
          comp<ChartSeries>
            [ "DataSource" => pts2
              "XName" => "X"
              "YName" => "Y"
              "Fill" => "blue"
              "Type" => ChartSeriesType.Line ]
            [] ] ]

let view model dispatch =
  div [ attr.classes [ B.container
                       B.``is-fluid`` ] ] [
    h1 [ attr.``class`` B.title ] [
      text "Advent of Code 2019, Problem 3a"
    ]
    createTextEntry
      "Path 1"
      model.UI.Error
      model.UI.Input1
      Msg.UiChangeInput1
      dispatch
    createTextEntry
      "Path 2"
      model.UI.Error
      model.UI.Input2
      Msg.UiChangeInput2
      dispatch
    createButton "Create Paths" Msg.CreatePaths dispatch
    createViewBox <| model.PathOne.ToString()
    createViewBox <| model.PathTwo.ToString()
    createButton "Create Wires" Msg.CreateWires dispatch
    createViewBox <| model.WireOne.ToString()
    createViewBox <| model.WireTwo.ToString()
    createButton "Find Intersections 1" Msg.GetAllIntersectionPoints dispatch
    createViewBox
      (model.IntersectionPoints
       |> List.sortBy (fun e -> e.distanceFromCenter)
       |> List.fold (fun acc e -> acc + "," + e.ToString()) "")
    createViewBox
      (model.IntersectionPoints
       |> List.sortBy (fun e -> e.distanceBySections)
       |> List.fold (fun acc e -> acc + "," + e.ToString()) "")
    viewChart model.WireOne model.WireTwo model.IntersectionPoints
  ]
