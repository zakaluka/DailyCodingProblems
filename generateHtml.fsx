#load @"packages/Common/FSharp.Formatting/FSharp.Formatting.fsx"

#I @"packages/common/Fue/lib/netstandard2.0"
#r @"Fue.dll"

#I @"packages\common\Scriban\lib\net40"
#r "Scriban.dll"

open FSharp.Literate
open System.IO
open Scriban
open Scriban.Runtime
open System.Text.RegularExpressions

// The directory from which we are executing
let baseDir = __SOURCE_DIRECTORY__

let templateFile = Path.Combine(baseDir, @"template.html")

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
            let fsharpCoreDir = @"-I:../../packages/common/FSharp.Formatting/lib"
            let systemRuntime = "-r:System.Runtime"

            if src.EndsWith "fsx" then
                Literate.ParseScriptFile(
                    path = src,
                    references = false,
                    fsiEvaluator = FSharp.Literate.FsiEvaluator([|fsharpCoreDir|]),
                    formatAgent = FSharp.CodeFormat.CodeFormat.CreateAgent(),
                    compilerOptions = systemRuntime + " " + fsharpCoreDir
                )
            elif src.EndsWith "md" then
                Literate.ParseMarkdownFile(
                    path = src,
                    references = false,
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

    // Common, parsed file.
    let parsedFile = file |> parse

    // Main body, in HTML
    let body = parsedFile |> format

    // Tips (on mouseover), in HTML
    let tips = parsedFile.FormattedTips

    // Source to include in article
    let src = parsedFile.Source.ToString()

    // Create `ScriptObject` for Scriban settings
    let createSO toc =
        let scriptobject = ScriptObject()
        scriptobject.Add("tooltips", tips)
        scriptobject.Add("page-title", (Path.GetFileNameWithoutExtension(file)))
        scriptobject.Add("document", body)
        scriptobject.Add("source", src)
        if Option.isSome toc then scriptobject.Add("toc", toc.Value)
        scriptobject

    // Create final HTML file using Scriban library.
    let compiledHtml includeTOC =
        let tp = Template.Parse(File.ReadAllText templateFile, templateFile)

        let context = TemplateContext()
        context.PushGlobal(createSO includeTOC)

        tp.Render(context)

    // Generate a table of contents using a temporary version of the HTML file
    // We cannot use `src` because it doesn't contain the link names that the
    // TOC must point to.
    let toc () =
        // Pattern to match
        // Group 0 = the entire matched string
        // Group 1 = header level, e.g. 'h1'
        // Group 2 = href, e.g. 'href="#First-level-heading"'
        // Group 3 = name, e.g. 'First-level heading'
        let pattern =
            @"\<(h[1-6])\>\w*\<a .*class=""anchor""[^>]*(href=[^>]+)\>([^<]*)"

        // Create the regex
        let regex =
            Regex(pattern,
                RegexOptions.IgnoreCase |||
                RegexOptions.Multiline |||
                RegexOptions.Compiled)

        // Convenience function to turn a `Match` into a string
        // NOTE: Entries within a group are 0-based (like normal .NET arrays)
        let matchToString (m:Match) =
            // From a header tag, get the number.  E.g. H1 becomes 1
            let getHeadingLevel (hd:string) = hd.Substring(1) |> int
            // Create spacing based on header level.  H1 is not indented.
            let rec spacing acc n =
                if n <= 1 then acc
                else spacing (acc + "&nbsp;&nbsp;&nbsp;&nbsp;") (n - 1)
            let gs = m.Groups
            sprintf "%s<a %s>%s</a><br>"
                (gs.[1].Value |> getHeadingLevel |> spacing "")
                (gs.[2].Value)
                (gs.[3].Value)

        // Get the matches.  Each match is a successful match of the pattern
        // and should contain all the groups detailed above.
        let matches : seq<Match> = regex.Matches(compiledHtml None) |> Seq.cast

        // Create the TOC
        Seq.fold (fun acc e -> acc + (matchToString e)) "" matches

    // Write the output to the file
    System.IO.File.WriteAllText(
        Path.Combine(od, (Path.GetFileNameWithoutExtension(file)) + ".html"),
        toc () |> Some |> compiledHtml)

// For each sub-directory containing a problem, generate an output HTML file.
for d in problemDirs do
    let files = Directory.GetFiles d
    if Array.isEmpty files then
        failwith (sprintf "Found no files in directory %s" d)
    elif Array.length files > 1 then
        failwith (sprintf "Found multiple files in directory %s" d)
    else
        processSingleProblem d files.[0]