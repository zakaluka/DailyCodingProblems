// =============================================================================
// Section for View
// =============================================================================
module  aoc2019_3.View

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms
open Fabulous.SimplerHelpers

let createButton txt msg dispatch =
  Button.button [
    Button.Text txt
    Button.OnClick (fun _ -> dispatch msg )
  ]

let layout =
  StackLayout.stackLayout [
    StackLayout.Orientation StackOrientation.Vertical

  ]

let view model dispatch =
  ContentPage.contentPage[
    ContentPage.Content <|

  ]
