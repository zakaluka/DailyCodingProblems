module aoc2019_3.State

open Path
open Wire

let init() =
  { ui =
      { delayMs = 100
        animate = true
        input1 = ""
        input2 = ""
        lowestX = -10
        highestX = 10
        lowestY = -10
        highestY = 10 }
    pathOne = Path.empty
    pathTwo = Path.empty
    wireOne = Wire.empty
    wireTwo = Wire.empty
    intersectionPoints = [] },
  []

let update msg model =
  match msg with
  | CreatePaths -> failwith ""
  | CreateWires -> failwith ""
  | GetAllIntersectionPoints -> failwith ""
  | UiChangeInput1 -> failwith ""
  | UiChangeInput2 -> failwith ""
  | UiChangeAnimate -> failwith ""
  | UiChangeDelay -> failwith ""


