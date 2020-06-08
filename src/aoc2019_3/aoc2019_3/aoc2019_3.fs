// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace aoc2019_3

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open FsToolkit.ErrorHandling
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open System

/// Strategy:
///
/// - Input string is of the form `D15,U25,L30,R40`.
///   - This is represented in its entirety by a `Path`.
///     - Each entry, separated by commas, is a `PathEntry`, which consists of 2
///       items:
///       - `MovementDirection` that represents up, down, left, or right.
///       - Distance represented by an `i32`.
/// - A `Path` can be turned into a `Wire`.
/// - A `Wire` is a series of segments that represents the main item in the
///   problem.
///     - A `Wire` is composed of one or more `WireSection`s.  Each
///       `WireSection` is composed of 3 parts:
///         - An `Orientation`, which is either vertical or horizontal.
///         - A start `Point` (x and y `i32` coordinates).
///         - An end `Point` (x and y `i32` coordinates).
module App =
  // =============================================================================
  // Section for `Path`
  // =============================================================================

  /// The direction to move in to create the next `WireSection`
  type MovementDirection =
    | U
    | D
    | L
    | R
    override md.ToString() =
      match md with
      | U -> "U"
      | D -> "D"
      | L -> "L"
      | R -> "R"

  module MovementDirection =
    let tryParse (c: char) =
      match c with
      | 'u'
      | 'U' -> Ok U
      | 'd'
      | 'D' -> Ok D
      | 'l'
      | 'L' -> Ok L
      | 'r'
      | 'R' -> Ok R
      | _ -> sprintf "Not one of U, D, L, R: %c" c |> Error

  type Distance =
    | Dist of int
    override d.ToString() =
      match d with
      | Dist (i) -> i.ToString()

  module Distance =
    let create i = Ok(Dist i)

    let tryParse (s: string) =
      let mutable x: int = 0
      let success = Int32.TryParse(s,&x)
      if success then
        if x > 0 then create x else Error "Zero distance not allowed"
      else
        Error(sprintf "Unable to parse as Int32: %s" s)

  /// Represents one entry in the `Path`, such as `U10`.
  type PathEntry =
    { Direction: MovementDirection
      Distance: Distance }

  module PathEntry =
    let create (dir,dist) = Ok { Direction = dir;Distance = dist }

    let direction pe = pe.Direction

    let distance pe = pe.Distance

    let tryParse (s: string) =
      let validate (s: string) =
        if s.Length < 2 then
          Error(sprintf "String not of type '[UDLR][0-9]+': %s" s)
        else
          Ok s

      result {
        let! s = validate s
        let! md = s.[0] |> MovementDirection.tryParse
        let! dist = s.[1..] |> Distance.tryParse
        return! create (md,dist)
      }

  type Path =
    { Moves: PathEntry list }
    override p.ToString() =
      p.Moves
      |> List.fold (fun acc e ->
           sprintf "%s,%s%s" acc (e.Direction.ToString())
             (e.Distance.ToString())) ""

  module Path =
    let tryParse (s: string) =
      result {
        let! moves =
          s.Split ','
          |> Array.filter (String.IsNullOrWhiteSpace >> not)
          |> Array.map PathEntry.tryParse
          |> Array.toList
          |> List.sequenceResultA

        return { Moves = moves }
      }

  // =============================================================================
  // Section for `Wire`
  // =============================================================================

  /// The orientation of a section of wire
  type Orientation =
    | V
    | H

  /// A point in a 2D plane
  type Point = { x: int;y: int }

  module Point =
    let CENTRAL_PORT = { x = 0;y = 0 }

    let MIN =
      { x = Int32.MinValue
        y = Int32.MinValue }

    /// Calculates the Manhattan distance of a point to the center
    let manhattanDistance p refPoint =
      (abs (p.x - refPoint.x))
      + (abs (p.y - refPoint.y))

    let min p q =
      let mdP = manhattanDistance p MIN
      let mdQ = manhattanDistance q MIN
      if mdP <= mdQ then p else q

  type WireSection =
    { Start: Point
      End: Point
      Orientation: Orientation }

  module WireSection =
    let create point1 point2 =
      if point1 = point2 then
        Error "A WireSection cannot be created with 2 identical Points"
      // Point1 and Point2 must share either the `x` or `y` values
      // i.e. point1.x = point2.x || point1.y = point2.y. If this is not true,
      // then raise an error.
      else if point1.x <> point2.x && point1.y <> point2.y then
        Error "A WireSection must be horizontal or vertical, not diagonal"
      else
        let md1 = Point.manhattanDistance point1 Point.MIN
        let md2 = Point.manhattanDistance point2 Point.MIN

        let sp,ep =
          if md1 < md2 then point1,point2
          else if md1 > md2 then point2,point1
          else if point1.x < point2.x then point1,point2
          else point2,point1
        let dir = if sp.x = ep.x then V else H
        Ok
          { Start = sp
            End = ep
            Orientation = dir }

  type Model = { Count: int;Step: int;TimerOn: bool }

  type Msg =
    | Increment
    | Decrement
    | Reset
    | SetStep of int
    | TimerToggled of bool
    | TimedTick

  let initModel = { Count = 0;Step = 1;TimerOn = false }

  let init () = initModel,Cmd.none

  let timerCmd =
    async {
      do! Async.Sleep 200
      return TimedTick
    }
    |> Cmd.ofAsyncMsg

  let update msg model =
    match msg with
    | Increment ->
        { model with
            Count = model.Count + model.Step },
        Cmd.none
    | Decrement ->
        { model with
            Count = model.Count - model.Step },
        Cmd.none
    | Reset -> init ()
    | SetStep n -> { model with Step = n },Cmd.none
    | TimerToggled on ->
        { model with TimerOn = on },(if on then timerCmd else Cmd.none)
    | TimedTick ->
        if model.TimerOn then
          { model with
              Count = model.Count + model.Step },
          timerCmd
        else
          model,Cmd.none

  let view (model: Model) dispatch =
    View.ContentPage
      (content =
        View.StackLayout
          (padding = Thickness 20.0,
           verticalOptions = LayoutOptions.Center,
           children =
             [ View.Label
                 (text = sprintf "%d" model.Count,
                  horizontalOptions = LayoutOptions.Center,
                  width = 200.0,
                  horizontalTextAlignment = TextAlignment.Center)
               View.Button
                 (text = "Increment",
                  command = (fun () -> dispatch Increment),
                  horizontalOptions = LayoutOptions.Center)
               View.Button
                 (text = "Decrement",
                  command = (fun () -> dispatch Decrement),
                  horizontalOptions = LayoutOptions.Center)
               View.Label
                 (text = "Timer",horizontalOptions = LayoutOptions.Center)
               View.Switch
                 (isToggled = model.TimerOn,
                  toggled = (fun on -> dispatch (TimerToggled on.Value)),
                  horizontalOptions = LayoutOptions.Center)
               View.Slider
                 (minimumMaximum = (0.0,10.0),
                  value = double model.Step,
                  valueChanged =
                    (fun args -> dispatch (SetStep(int (args.NewValue + 0.5)))),
                  horizontalOptions = LayoutOptions.FillAndExpand)
               View.Label
                 (text = sprintf "Step size: %d" model.Step,
                  horizontalOptions = LayoutOptions.Center)
               View.Button
                 (text = "Reset",
                  horizontalOptions = LayoutOptions.Center,
                  command = (fun () -> dispatch Reset),
                  commandCanExecute = (model <> initModel)) ]))

  // Note, this declaration is needed if you enable LiveUpdate
  let program =
    XamarinFormsProgram.mkProgram init update view

type App() as app =
  inherit Application()

  let runner =
    App.program
#if DEBUG
    |> Program.withConsoleTrace
#endif
    |> XamarinFormsProgram.run app

#if DEBUG
// Uncomment this line to enable live update in debug mode.
// See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
//
//do runner.EnableLiveUpdate()
#endif

// Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
// See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
  let modelId = "model"

  override __.OnSleep() =

    let json =
      Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)

    Console.WriteLine
      ("OnSleep: saving model into app.Properties, json = {0}",json)

    app.Properties.[modelId] <- json

  override __.OnResume() =
    Console.WriteLine "OnResume: checking for model in app.Properties"
    try
      match app.Properties.TryGetValue modelId with
      | true,(:? string as json) ->

          Console.WriteLine
            ("OnResume: restoring model from app.Properties, json = {0}",json)

          let model =
            Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

          Console.WriteLine
            ("OnResume: restoring model from app.Properties, model = {0}",
             (sprintf "%0A" model))
          runner.SetCurrentModel(model,Cmd.none)

      | _ -> ()
    with ex ->
      App.program.onError
        ("Error while restoring model found in app.Properties",ex)

  override this.OnStart() =
    Console.WriteLine "OnStart: using same logic as OnResume()"
    this.OnResume()
#endif
