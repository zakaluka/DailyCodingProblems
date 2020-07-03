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
module aoc2019_3.App

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open FsToolkit.ErrorHandling
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open System

// =============================================================================
// Section for `Path`
// =============================================================================
module Path =
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
    let tryParse(c: char) =
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
      | Dist(i) -> i.ToString()

    member d.GetValue() =
      match d with
      | Dist(i) -> i

  module Distance =
    let create i = Ok(Dist i)

    let getValue(d: Distance) = d.GetValue()

    let tryParse(s: string) =
      let mutable x: int = 0
      let success = Int32.TryParse(s,&x)
      if success then
        if x > 0 then create x else Error "Zero distance not allowed"
      else
        Error(sprintf "Unable to parse as Int32: %s" s)

  /// Represents one entry in the `Path`, such as `U10`.
  type PathEntry = { Direction: MovementDirection;Distance: Distance }

  module PathEntry =
    let create(dir,dist) = Ok { Direction = dir;Distance = dist }

    let direction pe = pe.Direction

    let distance pe = pe.Distance

    let tryParse(s: string) =
      let validate(s: string) =
        if s.Length < 2 then
          Error(sprintf "String not of type '[UDLR][0-9]+': %s" s)
        else
          Ok s

      result {
        let! s = validate s
        let! md = s.[0] |> MovementDirection.tryParse
        let! dist = s.[1..] |> Distance.tryParse
        return! create(md,dist)
      }

  type Path =
    { Moves: PathEntry list }

    override p.ToString() =
      p.Moves
      |> List.fold (fun acc e ->
           sprintf "%s,%s%s" acc (e.Direction.ToString())
             (e.Distance.ToString())) ""

  module Path =
    let empty = { Moves = [] }

    let tryParse(s: string) =
      result {
        let! moves =
          s.Split ','
          |> Array.filter(String.IsNullOrWhiteSpace >> not)
          |> Array.map PathEntry.tryParse
          |> Array.toList
          |> List.sequenceResultA

        return { Moves = moves }
      }

// =============================================================================
// Section for `Wire`
// =============================================================================
module Wire =
  /// The orientation of a section of wire
  type Orientation =
    | V
    | H

  /// A point in a 2D plane
  type Point = { x: int;y: int }

  module Point =
    let CENTRAL_PORT = { x = 0;y = 0 }

    let MIN = { x = Int32.MinValue;y = Int32.MinValue }

    /// Calculates the Manhattan distance of a point to the center
    let manhattanDistance p refPoint =
      (abs(p.x - refPoint.x)) + (abs(p.y - refPoint.y))

    let min p q =
      if manhattanDistance p MIN <= manhattanDistance q MIN then p else q

    let max p q =
      if manhattanDistance p MIN > manhattanDistance q MIN then p else q

  type WireSection = { Start: Point;End: Point;Orientation: Orientation }

  module WireSection =
    /// Creates a section of the overall `Wire`.  The WireSection is oriented
    /// from `point1` to `point2`. See `reorient()` for information about
    /// changing the orientation of the WireSection to meet other needs.
    ///
    /// Invariants:
    /// - Sections must be horizontal or vertical
    ///     - i.e. either 'x' or 'y' values must be the same.
    /// - Section length >= 1
    ///     - i.e. either 'x' or 'y' values must be different.
    /// - The central port is at (0,0)
    ///
    /// Graph for reference:
    /// ```text
    ///           |+y
    ///           |
    ///  (-x,+y)  |  (+x,+y)
    ///           |
    /// __________|__________
    /// -x        |        +x
    ///           |
    ///  (-x,-y)  |  (+x,-y)
    ///           |
    ///           |-y
    /// ```
    let create point1 point2 =
      if point1 = point2 then
        sprintf "A WireSection cannot be created with 2 identical Points: %s %s"
          (point1.ToString()) (point2.ToString())
        |> Error
      else if point1.x <> point2.x && point1.y <> point2.y then
        sprintf
          "A WireSection must be horizontal or vertical, not diagonal: %s %s"
          (point1.ToString()) (point2.ToString())
        |> Error
      else
        let sp,ep =
          if point1.x < point2.x || point1.y < point2.y then
            point1,point2
          else
            point2,point1

        let dir = if sp.x = ep.x then V else H
        Ok { Start = sp;End = ep;Orientation = dir }

    /// Reorients a WireSection so that either `x` or `y` value of the start point
    /// is less than the corresponding value in the end point. The other value
    /// will be the same in both points.
    ///
    /// PostCondition:
    /// - If horizontal, start point will be to the left of the end point.
    /// - if vertical, start point will be below the end point.

    let reorient ws =
      if ws.Start.x < ws.End.x || ws.Start.y < ws.End.y then
        ws
      else
        { ws with Start = ws.End;End = ws.Start }

    let tryParse ({ Path.Direction = dir;Path.Distance = dist }) start =
      result {
        let endPt =
          match dir with
          | Path.MovementDirection.U ->
              { start with y = start.y + dist.GetValue() }
          | Path.MovementDirection.D ->
              { start with y = start.y - dist.GetValue() }
          | Path.MovementDirection.L ->
              { start with x = start.x - dist.GetValue() }
          | Path.MovementDirection.R ->
              { start with x = start.x + dist.GetValue() }

        let! pe = create start endPt

        return pe,endPt
      }

    /// Enforce wire section invariants.  See General Invariants section on the
    /// `intersection()` method.
    let isValidWireSection(ws: WireSection) =
      if ws.Start.x > ws.End.x then
        Error "Horizontal line is flipped"
      else if ws.Start.y > ws.End.y then
        Error "Vertical line is flipped"
      else if ws.Orientation = H && ws.Start.y <> ws.End.y then
        Error "Horizontal line has an angle"
      else if ws.Orientation = V && ws.Start.x <> ws.End.x then
        Error "Vertical line has an angle"
      else
        Ok ws

    /// Logic for intersection of 2 horizontal lines. See "Overlap - 2
    /// `Horizontal` lines" section on the `intersection()` method.
    let intersectTwoH ws1 ws2 =
      let leftL,rightL = if ws1.Start.x <= ws2.Start.x then ws1,ws2 else ws2,ws1
      // Both lines are not horizontal
      if leftL.Orientation <> H || rightL.Orientation <> H then
        None
      // Not on the same `y` level
      elif leftL.Start.y <> rightL.Start.y then
        None
      // No overlap between horizontal lines
      elif leftL.End.x < rightL.Start.x then
        None
      // Find point of overlap
      else
        [ rightL.Start.x .. (min leftL.End.x rightL.End.x) ]
        |> List.map(fun e -> { x = e;y = leftL.Start.y })
        |> List.minBy(fun e -> Point.manhattanDistance e Point.CENTRAL_PORT)
        |> Some

    /// Logic for intersection of 2 vertical lines. See "Overlap - 2
    /// `Vertical` lines" section on the `intersection()` method.
    let intersectTwoV ws1 ws2 =
      let lowerL,upperL =
        if ws1.Start.y <= ws2.Start.y then ws1,ws2 else ws2,ws1
      // Both lines are not vertical
      if lowerL.Orientation <> V || upperL.Orientation <> V then
        None
      // Not on the same `x` level
      elif lowerL.Start.x <> upperL.Start.x then
        None
      // No overlap between vertical lines
      elif lowerL.End.y < upperL.Start.y then
        None
      // Find point of overlap
      else
        [ upperL.Start.y .. (min lowerL.End.y upperL.End.y) ]
        |> List.map(fun e -> { x = lowerL.Start.x;y = e })
        |> List.minBy(fun e -> Point.manhattanDistance e Point.CENTRAL_PORT)
        |> Some

    /// Logic for intersection of a horizontal and a vertical line. See
    /// "Overlap - 1 Horizontal and 1 Vertical line" section on the
    /// `intersection()` method.
    let intersectOneHOneV ws1 ws2 =
      let horizL,vertL = if ws1.Orientation = H then ws1,ws2 else ws2,ws1
      // Both lines have the expected orientation
      if horizL.Orientation <> H || vertL.Orientation <> V then
        None
      // `x` values don't intersect
      elif vertL.Start.x < horizL.Start.x || vertL.Start.x > horizL.End.x then
        None
      // `y` values don't intersect
      elif horizL.Start.y < vertL.Start.y || horizL.Start.y > vertL.End.y then
        None
      // Find point of overlap
      else
        Some { x = vertL.Start.x;y = horizL.Start.y }

    /// Calculate the intersection of two `WireSection`s. As context,
    /// the central port is at `(0,0)`, `x` increases to the right, and `y`
    /// increases as you go up. See `create()` for a reference graph.
    ///
    /// Overview / Setup:
    /// - Line is `L.start.x, L.start.y` to `L.end.x, L.end.y`
    /// - Assumptions:
    ///   - When two horizontal or two vertical lines are laid on top of each
    ///     other, their intersection point is the closest point to
    /// - General invariants:
    ///   - Line orientation - both lines are oriented so that the start points
    ///     are less than or equal to the end points.
    ///     - `L.start.x <= L.end.x && L.start.y <= L.end.y`
    ///   - If lines are horizontal, then their the `y` values don't change.
    ///     - Line 1: `L.start.y == L.end.y`
    ///   - If lines are vertical, then their `x` values don't change.
    ///     - Line 1: `L.start.x == L.end.x`
    /// - Identify the following lines based on start points.
    ///   - `leftL` = the line whose starting point `sx` is left-most.
    ///     - `rightL` is the other line.
    ///   - `lowerL` = the line whose starting point `sy` is lowest.
    ///     - `upperL` is the other line.
    ///
    /// There are multiple scenarios to consider:
    /// - No-overlap scenarios:
    ///   - leftL is strictly to the left of rightL.
    ///     - `leftL.end.x < rightL.start.x`
    ///   - lowerL is strictly below upperL.
    ///     - `lowerL.end.y < upperL.start.y`
    ///
    /// - Overlap - 2 `Horizontal` lines
    ///   - Invariants:
    ///     - `leftL.orientation == H`
    ///     - `rightL.orientation == H`
    ///     - `leftL.start.y == rightL.start.y`
    ///   - Scenarios:
    ///     - Same size or `leftL` is longer
    ///       - `leftL.start.x <= rightL.start.x && leftL.end.x >= rightL.end.x`
    ///         - Intersection is between `rightL.start.x` and `rightL.end.x`,
    ///           inclusive
    ///     - `rightL` goes past the end of `leftL`
    ///       - `leftL.start.x <= rightL.start.x && leftL.end.x < rightL.end.x`
    ///         - Intersection is between `rightL.start.x` and `leftL.end.x`,
    ///           inclusive
    ///
    /// - Overlap - 2 `Vertical` lines
    ///   - Similar to 2 Horizontal lines.
    ///   - Invariants:
    ///     - `lowerL.orientation == V`
    ///     - `upperL.orientation == V`
    ///     - `lowerL.start.x == upperL.start.x`
    ///   - Scenarios:
    ///     - Same size or `lowerL` is longer
    ///       - `lowerL.start.y <= upperL.start.y && lowerL.end.y >= upperL.end.y`
    ///         - Intersection is between `upperL.start.y` and `upperL.end.y`,
    ///           inclusive
    ///       - `lowerL.start.y <= upperL.start.y && lowerL.end.y < upperL.end.y`
    ///         - Intersection is between `upperL.start.y` and `lowerL.end.y`,
    ///           inclusive
    ///
    /// - Overlap - 1 Horizontal and 1 Vertical line
    ///   - Invariants
    ///     - `leftL.orientation == H`
    ///     - `lowerL.orientation == V`
    ///     - `leftL.start.x <= lowerL.start.x <= leftL.end.x`
    ///     - `lowerL.start.y <= leftL.start.y <= lowerL.end.y`
    ///   - Scenarios:
    ///     - There is one point of overlap.
    ///       - Intersection at: `lowerL.start.x, leftL.start.y`
    let intersection ws_1 ws_2 =
      result {
        let! ws1 = ws_1 |> reorient |> isValidWireSection
        let! ws2 = ws_2 |> reorient |> isValidWireSection

        match ws1.Orientation,ws2.Orientation with
        | H,H -> return intersectTwoH ws1 ws2
        | V,V -> return intersectTwoV ws1 ws2
        | V,H
        | H,V -> return intersectOneHOneV ws1 ws2
      }

  type Wire = Wire of WireSection list

  module Wire =
    let empty = Wire []

    let tryParse(p: Path.Path) =
      result {
        let! wireSections =
          p.Moves
          |> List.fold (fun acc e ->
               match acc with
               | Ok(accList,lastEndPt) ->
                   match WireSection.tryParse e lastEndPt with
                   | Ok(newPE,newEndPt) -> Ok(accList @ [ newPE ],newEndPt)
                   | Error(e) -> Error(e)
               | Error(_) as x -> x) (Ok([],Point.CENTRAL_PORT))

        return wireSections |> fst |> Wire
      }

    let intersectionPoints w1 w2 =
      let intersectionWtoWS (Wire(w)) ws =
        result {
          let! pointsOfIntersection =
            w
            |> List.traverseResultA(fun wws -> WireSection.intersection ws wws)

          return pointsOfIntersection
                 |> List.filter(Option.isSome)
                 |> List.map(Option.get)
                 |> List.minBy(fun p ->
                      Point.manhattanDistance p Point.CENTRAL_PORT)
        }

      result {
        let (Wire(wire1)) = w1

        let! allIntersections =
          wire1 |> List.traverseResultA(intersectionWtoWS w2)

        return allIntersections
      }

// =============================================================================
// Section for Model and Messages
// =============================================================================

open Path
open Wire

type ModelUi =
  { delayMs: int
    animate: bool
    input1: string
    input2:string
    lowestX: int
    highestX: int
    lowestY: int
    highestY: int }

type Model =
  { ui: ModelUi
    pathOne: Path
    pathTwo: Path
    wireOne: Wire
    wireTwo: Wire
    intersectionPoints:Point list}

let init() =
    { ui =
        { delayMs = 100
          animate=true
          input1 = ""
          input2=""
          lowestX = -10
          highestX = 10
          lowestY = -10
          highestY = 10 }
      pathOne = Path.empty
      pathTwo = Path.empty
      wireOne = Wire.empty
      wireTwo = Wire.empty
      intersectionPoints=[] }, []

type Msg = | CreatePaths | CreateWires |GetAllIntersectionPoints

// =============================================================================
// Section for Update
// =============================================================================

let update msg model =
  match msg with
  | CreatePaths -> failwith ""
  | CreateWires -> failwith ""
  | GetAllIntersectionPoints -> ""

// =============================================================================
// Section for View
// =============================================================================



// =============================================================================
// Original stuff
// =============================================================================




type ModelOrig = { Count: int;Step: int;TimerOn: bool }

type MsgOrig =
  | Increment
  | Decrement
  | Reset
  | SetStep of int
  | TimerToggled of bool
  | TimedTick

let initModelOrig = { Count = 0;Step = 1;TimerOn = false }

let initOrig() = initModelOrig,Cmd.none

let timerCmd =
  async {
    do! Async.Sleep 200
    return TimedTick
  }
  |> Cmd.ofAsyncMsg

let updateOrig msg model =
  match msg with
  | Increment -> { model with Count = model.Count + model.Step },Cmd.none
  | Decrement -> { model with Count = model.Count - model.Step },Cmd.none
  | Reset -> initOrig()
  | SetStep n -> { model with Step = n },Cmd.none
  | TimerToggled on ->
      { model with TimerOn = on },(if on then timerCmd else Cmd.none)
  | TimedTick ->
      if model.TimerOn then
        { model with Count = model.Count + model.Step },timerCmd
      else
        model,Cmd.none

let viewOrig (model: ModelOrig) dispatch =
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
             View.Label(text = "Timer",horizontalOptions = LayoutOptions.Center)
             View.Switch
               (isToggled = model.TimerOn,
                toggled = (fun on -> dispatch(TimerToggled on.Value)),
                horizontalOptions = LayoutOptions.Center)
             View.Slider
               (minimumMaximum = (0.0,10.0),
                value = double model.Step,
                valueChanged =
                  (fun args -> dispatch(SetStep(int(args.NewValue + 0.5)))),
                horizontalOptions = LayoutOptions.FillAndExpand)
             View.Label
               (text = sprintf "Step size: %d" model.Step,
                horizontalOptions = LayoutOptions.Center)
             View.Button
               (text = "Reset",
                horizontalOptions = LayoutOptions.Center,
                command = (fun () -> dispatch Reset),
                commandCanExecute = (model <> initModelOrig)) ]))

// Note, this declaration is needed if you enable LiveUpdate
let programOrig = XamarinFormsProgram.mkProgram initOrig updateOrig viewOrig

type App() as app =
  inherit Application()

  let runner =
    programOrig
#if DEBUG
    |> Program.withConsoleTrace
#endif
    |> XamarinFormsProgram.run app

#if DEBUG
  // Uncomment this line to enable live update in debug mode.
  // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
  //
  do runner.EnableLiveUpdate()
#endif

// Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
// See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
  let modelId = "model"

  override __.OnSleep() =

    let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)

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