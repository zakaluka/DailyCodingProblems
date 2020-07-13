module aoc2019_3.State

open Path
open Wire
open Fabulous
open FsToolkit.ErrorHandling

let init() =
  { ui =
      { delayMs = 100
        animate = true
        input1 = ""
        input2 = ""
        lowestX = -10
        highestX = 10
        lowestY = -10
        highestY = 10
        error = false }
    pathOne = Path.empty
    pathTwo = Path.empty
    wireOne = Wire.empty
    wireTwo = Wire.empty
    intersectionPoints = [] },
  Cmd.none

let update msg model =
  match msg with
  | CreatePaths ->
      let p1 = Path.tryParse model.ui.input1 |> Result.defaultValue Path.empty
      let p2 = Path.tryParse model.ui.input2 |> Result.defaultValue Path.empty
      { model with pathOne = p1;pathTwo = p2 },
      (if p1 = Path.empty || p2 = Path.empty then
        Cmd.ofMsg UiSetError
       else
         Cmd.ofMsg UiClearError)
  | CreateWires ->
      let w1 = Wire.tryParse model.pathOne |> Result.defaultValue Wire.empty
      let w2 = Wire.tryParse model.pathTwo |> Result.defaultValue Wire.empty
      { model with wireOne = w1;wireTwo = w2 },
      (if w1 = Wire.empty || w2 = Wire.empty then
        Cmd.ofMsg UiSetError
       else
         Cmd.ofMsg UiClearError)
  | GetAllIntersectionPoints ->
    match Wire.intersectionPoints model.wireOne model.wireTwo with
    | Ok(l) -> {model with intersectionPoints = l}, Cmd.ofMsg UiClearError
    | Error(e) -> {model with intersectionPoints=[]}, Cmd.ofMsg UiSetError
  | UiChangeInput1 t ->
      { model with ui = { model.ui with input1 = t } },Cmd.none
  | UiChangeInput2 t ->
      { model with ui = { model.ui with input2 = t } },Cmd.none
  | UiChangeAnimate b ->
      { model with ui = { model.ui with animate = b } },Cmd.none
  | UiChangeDelay i ->
      { model with ui = { model.ui with delayMs = i } },Cmd.none
  | UiSetError -> { model with ui = { model.ui with error = true } },Cmd.none
  | UiClearError -> { model with ui = { model.ui with error = false } },Cmd.none
