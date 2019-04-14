#load @"packages/Common/FSharp.Formatting/FSharp.Formatting.fsx"

#I @"packages/common/Fue/lib/netstandard2.0"
#r @"Fue.dll"

open FSharp.Literate
open System.IO
open Fue.Data
open Fue.Compiler

// The directory from which we are executing
let baseDir = __SOURCE_DIRECTORY__

let templateFile = @"template.html"

// The source sub-directory
let source = Path.Combine(baseDir, "src")

// The output sub-directory for each problem
let outputDir = "output"

// Find all the directories in `source` containing problems.
let problemDirs = Directory.GetDirectories source

// Process a single problem directory `d`.
let processSingleProblem d file =
    let od =
        Path.Combine(d, outputDir)
        |> Directory.CreateDirectory
        |> fun d -> d.FullName

    // https://thinkbeforecoding.com/post/2018/12/07/full-fsharp-blog
    let parse (src:string) =
        let doc =
            let fsharpCoreDir = @"-I:../../packages/common/FSharp.Formatting/lib -I:../../packages/common/Fue/lib"
            let systemRuntime = "-r:System.Runtime"

            if src.EndsWith "fsx" then
                Literate.ParseScriptFile(
                    path = src,
                    // output = Path.Combine(od, "output.html"),
                    // format = OutputKind.Html,
                    // prefix = @"fs",
                    // lineNumbers = true,
                    // references = false,//true,
                    // generateAnchors = true,
                    // includeSource = false,//true,
                    fsiEvaluator = FSharp.Literate.FsiEvaluator([|fsharpCoreDir|]),
                    formatAgent = FSharp.CodeFormat.CodeFormat.CreateAgent(),
                    compilerOptions = systemRuntime + " " + fsharpCoreDir
                )
            elif src.EndsWith "md" then
                Literate.ParseMarkdownFile(
                    path = src,
                    // output = Path.Combine(od, "output.html"),
                    // format = OutputKind.Html,
                    // prefix = @"fs",
                    // lineNumbers = true,
                    // references = false,//true,
                    // generateAnchors = true,
                    // includeSource = false,//true,
                    fsiEvaluator = FSharp.Literate.FsiEvaluator([|fsharpCoreDir|]),
                    formatAgent = FSharp.CodeFormat.CodeFormat.CreateAgent(),
                    compilerOptions = systemRuntime + " " + fsharpCoreDir
                )
            else failwithf "Unknown file type: %s" src

        Literate.FormatLiterateNodes(
            doc = doc,
            format = OutputKind.Html,
            prefix = "fsout",
            lineNumbers = true,
            generateAnchors = true
        )

    // Turns a LiterateDocument into a string (HTML)
    let format (doc:LiterateDocument) =
        Formatting.format doc.MarkdownDocument true OutputKind.Html

    // Main body, in HTML
    let body = file |> parse |> format

    // Tips (on mouseover), in HTML
    let tips = file |> parse |> fun d -> d.FormattedTips

    let compiledHtml =
        init
        |> add "page-title" (Path.GetFileNameWithoutExtension(file))
        |> add "document" body
        |> add "tooltips" tips
        |> add "source" "test"
        // |> fromFile (Path.Combine(baseDir, templateFile))
        |> fromText @"<div>{{{tooltips}}}</div>"

    printfn ".. compiledHtml: %s" compiledHtml
    ()

// For each sub-directory containing a problem, generate an output HTML file.
for d in problemDirs do
    let files = Directory.GetFiles d
    if Array.isEmpty files then
        failwith (sprintf "Found no files in director %s" d)
    elif Array.length files > 1 then
        failwith (sprintf "Found multiple files in directory %s" d)
    else
        processSingleProblem d files.[0]