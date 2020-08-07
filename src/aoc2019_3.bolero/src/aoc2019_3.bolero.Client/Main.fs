module aoc2019_3.bolero.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html

type MyApp() =
  inherit ProgramComponent<Types.Model, Types.Msg>()

  override this.Program =
    Program.mkProgram (fun _ -> State.init ()) State.update View.view
