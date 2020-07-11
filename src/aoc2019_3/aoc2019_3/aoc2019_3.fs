namespace aoc2019_3

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open Fabulous.SimplerHelpers

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
  // Original stuff
  // =============================================================================

  type OrigModel = { Count: int;Step: int;TimerOn: bool }

  type OrigMsg =
    | Increment
    | Decrement
    | Reset
    | SetStep of int
    | TimerToggled of bool
    | TimedTick

  let origInitModel = { Count = 0;Step = 1;TimerOn = false }

  let origInit() = origInitModel,Cmd.none

  let timerCmd =
    async {
      do! Async.Sleep 200
      return TimedTick
    }
    |> Cmd.ofAsyncMsg

  let origUpdate msg model =
    match msg with
    | Increment -> { model with Count = model.Count + model.Step },Cmd.none
    | Decrement -> { model with Count = model.Count - model.Step },Cmd.none
    | Reset -> origInit()
    | SetStep n -> { model with Step = n },Cmd.none
    | TimerToggled on ->
        { model with TimerOn = on },(if on then timerCmd else Cmd.none)
    | TimedTick ->
        if model.TimerOn then
          { model with Count = model.Count + model.Step },timerCmd
        else
          model,Cmd.none

  let origView (model: OrigModel) dispatch =
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
                  commandCanExecute = (model <> origInitModel)) ]))

  // Note, this declaration is needed if you enable LiveUpdate
  let origProgram = XamarinFormsProgram.mkProgram origInit origUpdate origView

type App() as app =
  inherit Application()

  let runner =
    App.origProgram
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
