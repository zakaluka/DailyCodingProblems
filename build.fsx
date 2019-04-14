#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
//https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#r "Facades/netstandard"
#endif

open System.IO

open Fake.Core
open Fake.DotNet
open Fake.IO

// The directory that hold the source code for problems.
let srcDir = "./src"

// Output sub-directory
let outputDir = "output"

// The problems
let problemDirs =
    srcDir
    |> DirectoryInfo.ofPath
    |> DirectoryInfo.getSubDirectories

// Output directories (existing)
let outputDirs =
    problemDirs
    |> Seq.map
        (fun p ->
            DirectoryInfo.ofPath(
                p.FullName +
                (string Path.DirectorySeparatorChar) +
                outputDir))
    |> Seq.filter DirectoryInfo.exists
    |> Seq.map (fun p -> p.FullName)

// Debugging output
let tee f x = f x |> ignore; x

Target.create "Clean" (fun _ ->
    Seq.iter (printfn ".. %A") outputDirs
    outputDirs
    |> Shell.deleteDirs
)

Target.create "Build" (fun _ ->
    let paketFsi = @"packages\Common\FSharp.Compiler.Tools\tools\fsi.exe"

    let res =
        Fsi.exec (fun p ->
            { p with
                ConsoleColors = Some true
                Debug = Some false
                Optimize = Some true
                TailCalls = Some true
                ToolPath = Fsi.FsiTool.External paketFsi
            })
            (Path.Combine(".", "generateHtml.fsx"))
            []

    if fst res = 0 then
        snd res
        |> Seq.iter (printfn "%s")
    else
        snd res
        |> Seq.fold
            (fun acc e ->
                sprintf "%s%s%s" acc System.Environment.NewLine e
            )
            ""
        |> failwith
)

open Fake.Core.TargetOperators

"Clean"
    ==> "Build"

Target.runOrDefaultWithArguments "Build"
