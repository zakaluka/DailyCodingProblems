namespace aoc2019_3.Gtk

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.GTK

module Main =
    [<EntryPoint>]
    let Main(args) =
        Gtk.Application.Init()
        Forms.Init()

        let app = new aoc2019_3.App()
        let window = new FormsWindow()
        window.LoadApplication(app)
        window.SetApplicationTitle("Hello Fabulous GTK#")
        window.Show();

        Gtk.Application.Run()
        0
