(*** hide ***)
#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"

(*** hide ***)
#I "packages/Aether/lib/net45"
#I "packages/Chiron/lib/net452"
#r "Aether.dll"
#r "Chiron.dll"

(*** hide ***)
/// Aids in debugging.
let tee f x =
  f x |> ignore
  x

(**
# Daily Coding Problem 3, Revisited

A commenter on the original [Daily Coding Problem 3 post](https://www.znprojects.com/2019/03/daily-coding-problem-3.html) suggested using an [alternate technique](https://www.znprojects.com/2019/03/daily-coding-problem-3.html?showComment=1552137989127#c386341444873547265), specifically CBOR, as an additional contender.

This got me thinking that there are [lots of serialization libraries](https://nugetmusthaves.com/Tag/serialization) available for .NET and, like many people before me, I think it would be fun and interesting to do a more general comparison.  It also gives me a chance to try out [FSharp.Literate](https://fsprojects.github.io/FSharp.Formatting/literate.html) through the package set put together by the [FsLab project](https://fslab.org/).

# Problem Statement
As a reminder, here is the problem statement:

This problem was asked by Google.

Given the root to a binary tree, implement serialize(root), which serializes the tree into a string, and deserialize(s), which deserializes the string back into the tree.

For example, given the following Node class

    [lang=python]
    class Node:
        def __init__(self, val, left=None, right=None):
            self.val = val
            self.left = left
            self.right = right

The following test should pass:

    [lang=python]
    node = Node('root', Node('left', Node('left.left')), Node('right'))
    assert deserialize(serialize(node)).left.left.val == 'left.left'

# Solution

## Data structures

In the original solution, I used 3 separate data structures to model the problem (trying to stay close to the definition provided in the problem statement):

* Discriminated union
* Record type
* Class

Given that I am attempting more libraries than in the past, the first step is to organize the code in a way as to avoid 'cluttering' the type definition with library-specific code (when possible).
*)

/// The base models that will be used in the rest of the evaluation.
module Model =
  /// Tree as Discriminated Union
  type DU = Node of v:string * l:DU option * r:DU option

  /// Tree as Record.
  type R =
    { v: string
      l: R option
      r: R option }

  /// Tree as Class.
  type C(v: string, l: C option, r: C option) =
    member this.V = v
    member this.L = l
    member this.R = r

(**
## Libraries

The next question is what libraries should be tested. There are two major types:

* IDL based
* Non-IDL based

### Initial List of Libraries

I started by compiling a list of libraries that may make sense to test out:

* `Apache.Avro`: http://avro.apache.org/
* `Biser`: https://github.com/hhblaze/Biser
* `Bond.CSharp`: https://github.com/Microsoft/bond/
* `Chiron`: https://github.com/xyncro/chiron
* `Cuemon.Serialization.Xml`: https://github.com/gimlichael/cuemoncore
* `Dap.FlatBuffers`: https://github.com/google/flatbuffers
* `FsPickler`: https://github.com/mbraceproject/FsPickler
* `FsPickler.Json`: https://github.com/mbraceproject/FsPickler
* `Google.Protobuf`: https://github.com/protocolbuffers/protobuf
* `Hessian`: https://github.com/xuanye/DotXxlJob
* `HessianCSharp.IO`: https://github.com/yuniansheng/HessianCSharp
* `Hocon`: https://github.com/akkadotnet/HOCON
* `Hyperion`: https://github.com/akkadotnet/Hyperion
* `Jil`: https://github.com/kevin-montrose/Jil
* `KdSoft.FlatBuffers`: https://github.com/kwaclaw/flatbuffers
* `LibraProgramming.Hessian`: https://github.com/VlaTo/Hessian.NET
* `MessagePack`: https://github.com/neuecc/MessagePack-CSharp
* `MongoDB.Bson`: https://github.com/mongodb/mongo-csharp-driver
* `MsgPack.Cli`: https://github.com/msgpack/msgpack-cli
* `NetJSON`: https://github.com/rpgmaker/NetJSON
* `Nett`: https://github.com/paiden/Nett
* `Newtonsoft.Json`: https://github.com/JamesNK/Newtonsoft.Json
* `Newtonsoft.Json.Bson`: https://github.com/JamesNK/Newtonsoft.Json.Bson
* `PeterO.Cbor`: https://github.com/peteroupc/CBOR
* `Platform.Xml.Serialization`: https://github.com/platformdotnet/Platform
* `protobuf-net`: https://github.com/mgravell/protobuf-net
* `Thoth.Json.Net`: https://github.com/MangelMaxime/Thoth
* `Thrift`: http://thrift.apache.org/
* `YamlDotNet`: https://github.com/aaubry/YamlDotNet
* `ZeroFormatter`: https://github.com/neuecc/ZeroFormatter/

### Removing IDL-based Libraries

Personally, I'm not currently interested in a serialization library that requires a separate IDL definition of each data structure.  As such, I removed the following libraries from the list:

* `Apache.Avro`
* `Bond.CSharp`
* `Dap.FlatBuffers`
* `Google.Protobuf`
* `KdSoft.FlatBuffers`
* `protobuf-net`
* `Thrift`

### Removing Libraries That Don't Implement The Full Serialization Protocol

There are some libraries that use part of a serialization protocol but don't implement the whole thing.  These libraries were removed from the list also.

* `Hessian`: the website states that it only implements the types used in the Hessian 2 protocol, but not the full protocol

### Removing Libraries Without Documentation

If a library is too new, experimental, or is just a "toy", it won't really have documentation available.  In that case, I have removed them from the list.

### Final List of Libraries

* `Biser`: https://github.com/hhblaze/Biser
* `Chiron`: https://github.com/xyncro/chiron
* `Cuemon.Serialization.Xml`: https://github.com/gimlichael/cuemoncore
* `FsPickler`: https://github.com/mbraceproject/FsPickler
* `FsPickler.Json`: https://github.com/mbraceproject/FsPickler
* `HessianCSharp.IO`: https://github.com/yuniansheng/HessianCSharp
* `Hocon`: https://github.com/akkadotnet/HOCON
* `Hyperion`: https://github.com/akkadotnet/Hyperion
* `Jil`: https://github.com/kevin-montrose/Jil
* `LibraProgramming.Hessian`: https://github.com/VlaTo/Hessian.NET
* `MessagePack`: https://github.com/neuecc/MessagePack-CSharp
* `MongoDB.Bson`: https://github.com/mongodb/mongo-csharp-driver
* `MsgPack.Cli`: https://github.com/msgpack/msgpack-cli
* `NetJSON`: https://github.com/rpgmaker/NetJSON
* `Nett`: https://github.com/paiden/Nett
* `Newtonsoft.Json`: https://github.com/JamesNK/Newtonsoft.Json
* `Newtonsoft.Json.Bson`: https://github.com/JamesNK/Newtonsoft.Json.Bson
* `PeterO.Cbor`: https://github.com/peteroupc/CBOR
* `Platform.Xml.Serialization`: https://github.com/platformdotnet/Platform
* `Thoth.Json.Net`: https://github.com/MangelMaxime/Thoth
* `YamlDotNet`: https://github.com/aaubry/YamlDotNet
* `ZeroFormatter`: https://github.com/neuecc/ZeroFormatter/

## Patterns for operations

The next step is to find common sets of functions and create types (records of functions) that can be implemented for each of the data structures.  We have a few different sets of operations that can be performed on each data structure.

* Creating a node
*)

/// Create nodes in a tree
type 'a CreateRepo =
  { node: string * 'a option * 'a option -> 'a
    node2: string * 'a option -> 'a
    node1: string -> 'a
    nodeOption: string * 'a option * 'a option -> 'a option
    nodeOption2: string * 'a option -> 'a option
    nodeOption1: string -> 'a option }

(**
* Accessing the `val`, `left`, and `right` attributes of a node.
*)
/// Access node elements
type 'a AccessRepo =
  { ``val``: 'a -> string
    left: 'a -> 'a option
    right: 'a -> 'a option }

(**
* Serialization functions for Chiron
*)
/// Chiron helpers
type 'a ChironRepo =
  { serialize: 'a -> Chiron.Json
    deserialize: 'a -> Chiron.Json }

(**
### Discriminated Union operations

Now that we have the functions defined, implementations can be provided for the Discriminated Union.
*)

/// Various operations for the Discriminated Union model.
module DU =
  open Model
  let Create : DU CreateRepo =
    let n3 = fun (v, l, r) -> Node(v, l, r)
    let n2 = fun (v, l) -> Node(v, l, None)
    let n1 = fun v -> Node(v, None, None)
    { node = n3
      node2 = n2
      node1 = n1
      nodeOption = n3 >> Some
      nodeOption2 = n2 >> Some
      nodeOption1 = n1 >> Some }
  let Access : DU AccessRepo =
    { ``val`` = fun (Node(v, l, r)) -> v
      left = fun (Node(v, l, r)) -> l
      right = fun (Node(v, l, r)) -> r }

(**
### Record operations

Next are the implementations for the Record.
*)

/// Various operations for the Record model.
module R =
  open Model
  let Create : R CreateRepo =
    let n3 = fun (v, l, r) -> { v = v; l = l; r = r }
    let n2 = fun (v, l) -> { v = v; l = l; r = None }
    let n1 = fun v -> { v = v; l = None; r = None }
    { node = n3
      node2 = n2
      node1 = n1
      nodeOption = n3 >> Some
      nodeOption2 = n2 >> Some
      nodeOption1 = n1 >> Some }
  let Access : R AccessRepo =
    { ``val`` = fun r -> r.v
      left = fun r -> r.l
      right = fun r -> r.r }

(**
### Class operations

Finally, here are the implementations for the Class model.
*)

/// Various operations for the Class model.
module C =
  open Model
  let Create : C CreateRepo =
    let n3 = fun (v, l, r) -> C(v, l, r)
    let n2 = fun (v, l) -> C(v, l, None)
    let n1 = fun v -> C(v, None, None)
    { node = n3
      node2 = n2
      node1 = n1
      nodeOption = n3 >> Some
      nodeOption2 = n2 >> Some
      nodeOption1 = n1 >> Some }
  let Access : C AccessRepo =
    { ``val`` = fun c -> c.V
      left = fun c -> c.L
      right = fun c -> c.R }





(**
# Sample experiment

We start by referencing `Deedle` and `XPlot.GoogleCharts` libraries and then we
load the contents of *this* file:
*)

(*** define-output:loading ***)
open Deedle
open System.IO
open XPlot.GoogleCharts

let file = __SOURCE_DIRECTORY__ + "/P3a.fsx"
let contents = File.ReadAllText(file)

printfn "Loaded '%s' of length %d" file contents.Length

(*** include-output:loading ***)

(**
Now, we split the contents of the file into words, count the frequency of
words longer than 3 letters and turn the result into a Deedle series:
*)
let words =
  contents.Split(' ', '"', '\n', '\r', '*')
  |> Array.filter(fun s -> s.Length > 3)
  |> Array.map(fun s -> s.ToLower())
  |> Seq.countBy id
  |> series

(**
We can take the top 5 words occurring in this tutorial and see them in a chart:
*)
(*** define-output:grid ***)
words
|> Series.sort
|> Series.rev
|> Series.take 7
(*** include-it:grid ***)
(**
Finally, we can take the same 6 words and call `Chart.Column` to see them in a chart:
*)
(*** define-output:chart ***)
words
|> Series.sort
|> Series.rev
|> Series.take 7
|> Chart.Column
(*** include-it:chart ***)

(**
Summary
-------
An image is worth a thousand words:

![](http://imgs.xkcd.com/comics/hofstadter.png)
*)

