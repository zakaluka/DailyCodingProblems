namespace aoc2019_3.bolero.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Bolero.Remoting.Client
open Syncfusion.Blazor

module Program =

  [<EntryPoint>]
  let Main args =
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense
      "Mjk0NjE1QDMxMzgyZTMyMmUzMGxGQXJJYmErVlpUU0NVL3Vmc3ZrR2oweFVEcHRGbHV5dFJYb2cwVyt4N1E9"

    let builder =
      WebAssemblyHostBuilder.CreateDefault(args)

    builder.RootComponents.Add<Main.MyApp>("#main")
    builder.Services.AddRemoting(builder.HostEnvironment)
    |> ignore
    builder.Services.AddSyncfusionBlazor() |> ignore

    let host = builder.Build()
    host.RunAsync() |> ignore

    0
