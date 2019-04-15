(*** hide ***)
#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"

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

    class Node:
        def __init__(self, val, left=None, right=None):
            self.val = val
            self.left = left
            self.right = right

The following test should pass:

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

open System

module Model =
  //
  type 'a AccessRepo =
    { ``val``: 'a -> string
      l: 'a -> string
      r: 'a -> string }

  type 'a CreateRepository =
    { node: string * 'a option * 'a option -> 'a
      node2: string * 'a option -> 'a
      nodeOption: string * 'a option * 'a option -> 'a option
      nodeOption2: string * 'a option -> 'a option
      nodeOption1: string -> 'a option }

  type DUNode =
    | Node of string * DUNode option * DUNode option

    member x.``val`` =
      match x with
      | Node(v, l, r) -> v

    member x.left =
      match x with
      | Node(_, l, _) -> l

    member x.right =
      match x with
      | Node(_, _, r) -> r

    // Convenience methods to construct the DU
    static member create(v, l, r) = Node(v, l, r)
    static member create(v, l) = DUNode.create(v, l, None)
    static member createOpt(v, l, r) = DUNode.create(v, l, r) |> Some
    static member createOpt(v, l) = DUNode.create(v, l, None) |> Some
    static member createOpt(v) = DUNode.create(v, None, None) |> Some

  type RNode =
    {``val``: string
     left: RNode option
     right: RNode option}

    // Convenience methods to construct the record
    static member create(v, l, r) =
      {``val`` = v
       left = l
       right = r}

    static member createOpt(v, l, r) = RNode.create(v, l, r) |> Some
    static member createOpt(v, l) = RNode.create(v, l, None) |> Some
    static member createOpt(v) = RNode.create(v, None, None) |> Some

  type CNode(``val``: string, left: CNode option, right: CNode option) =
    // Save the values from the constructor
    member this.``val`` = ``val``
    member this.left = left
    member this.right = right
    // Convenience methods to construct the record
    static member create(v, l, r) = CNode(v, l, r)
    static member createOpt(v, l, r) = CNode.create(v, l, r) |> Some
    static member createOpt(v, l) = CNode.create(v, l, None) |> Some
    static member createOpt(v) = CNode.create(v, None, None) |> Some

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

