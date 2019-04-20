(*** hide ***)
#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"
#I @"packages/Hedgehog/lib/netstandard1.6"
#r "Hedgehog.dll"

(**
# Daily Coding Problem 4

This problem was asked by Stripe.

Given an array of integers, find the first missing positive integer in linear time and constant space. In other words, find the lowest positive integer that does not exist in the array. The array can contain duplicates and negative numbers as well.

For example, the input `[3, 4, -1, 1]` should give `2`. The input `[1, 2, 0]` should give `3`.

You can modify the input array in-place.

# Solution overview

## Constraints and assumptions

1. `0` is not considered a positive integer.  In the first example, the answer is expected to be `2`.  If `0` were considered a positive integer, the answer would have been `0`.

1. Linear time complexity, also known as `O(n)`.  To quote [Wikipedia](https://en.wikipedia.org/wiki/Time_complexity#Linear_time)
> ... there is a constant `c` such that the running time is at most `cn` for every input of size `n`.

1. Constant space complexity, also known as `O(1)`. To quote [StackOverflow](https://stackoverflow.com/questions/43260889/what-is-o1-space-complexity)
> ... `O(1)` denotes constant (10, 100 and 1000 are constant) space and DOES NOT vary depending on the input size (say `N`).

## What do these constraints mean?

### Linear time complexity

Linear time complexity limits the algorithms that can be used to solve this problem.  For example, a [comparison-based sort](https://en.wikipedia.org/wiki/Sorting_algorithm#Comparison_sorts) cannot perform better than `O(n log n)` in the worst case, which means that we cannot use any comparison-based sorting algorithm to solve this problem.

Having said that, there are [non-comparison sorting algorithms](https://en.wikipedia.org/wiki/Sorting_algorithm#Non-comparison_sorts) that, at least on paper, can have a linear worst-case time complexity.

### Constant space complexity

This means that we cannot change the space used by the algorithm as a function of the input size.  Or, in other words, I can't simply create a min-heap, for example, and ask for the minimum value because the heap's size will change each time depending on the size of the input array.  In case someone is thinking about creating a massive data structure to use for each execution of the algorithm (e.g. always create an Array of size MAX_INT for execution, regardless of the size of the input array) - while I guess that this is theoretically Constant space, we run into the following problems:

1. This is a hugely wasteful approach to solving the problem.
1. This solution will fail if we can use a data structure larger than addressable memory, for example a data structure that can be stored on disk.
1. I don't believe this is in the spirit of the problem.

On a side note, the final solution will not be pretty because:
* Most of F#'s data structures, by default, are immutable.  Luckily, Arrays are not.
* F# encourages a functional programming style and, I believe, the final code will have to be more procedural.

## So, how do we solve it?

I thought of a lot of solutions to solve this problem that can meet the given constraints.

1. Construct a min-heap / binary tree-style data structure and find the root (while filtering out negative values). **_Violates space complexity_**
    1.  After creating the heap / tree, filter the values to remove those entries where `value + 1` is also in the structure.

1. Use 2 sets and perform a set difference. **_Violates space complexity_**
    1. Create a set using the original values (call it `Set(orig)`)
    1. Create a set of original values + 1 (call it `Set(plusone)`)
    1. Do `Set(plusone) - Set(orig)` and find the smallest positive integer

1. Sort the array and find the first entry where the following entry's value is not `current entry + 1`.  **_Violates space and time complexity_**
    1. In other words, using a 0-indexed array:

    ```
        for i in 0 .. array.length - 2 do
            if array[i] + 1 <> array[i + 1] then
                return array[i] + 1
    ```
    1. This can work _if_ I can use a linear time and constant space sort.  As far as I know, there is no such sort.
        1. Comparison sorts violate time constraint
        1. Non-comparison sorts violate space constraint

1. Recognize that if an array has length `n`, the answer can be, at most, `n + 1`.
    1. Consider an array that is 1-indexed.
        1. If all the values in the array are `0` or negative, the answer is `1`.
        1. If all the values in the array are `>= 2`, the answer is `1`.
        1. If the value at each index of the array is the same as the index, the answer is `n + 1`.
    1. While I haven't done anything close to a formal proof for this, I believe I am right about this.
    1. It took me a **long** time to arrive at this.  I had been working on this problem in my head for a couple of weeks (mostly while commuting to and from work) before I came to this realization.  I also had to force myself to think procedurally, like in C/C++, to get to this answer.

# Code

The following is the solution I came up with.

## Strategy for the code

At a high level, the code adopts the following strategy:

1. First pass.  Places values in their corresponding indices.

    ```
        for each entry do
            while the entry's value is not 0 and is not index + 1 do
                swap values with the index represented by the value in the entry
    ```

1. Second pass.  Find the first entry where the value is `0`, return `index + 1`.

    ```
        Scan the array for the first entry with value of 0
        If found, return index + 1
        Else, return array length
    ```

When reading the code, there are two items to remember:

1. `value = index + 1`
1. `index = value - 1`
*)

/// The main function.
let solver arr =
  /// swap the value in between 2 indices.
  let swap (arr: int[]) i =
    let origVal = arr.[i]
    arr.[i] <- arr.[origVal - 1]
    arr.[origVal - 1] <- origVal

  // Process a single index.
  let processOneIndex (arr: int[]) i =
    // Invalid, negative or zero value (doesn't influence end result)
    if arr.[i] <= 0 then arr.[i] <- 0
    // Value is greater than the array length
    elif arr.[i] > Array.length arr then arr.[i] <- 0
    // Value is already set to the array index, no action required
    elif arr.[i] = i + 1 then ()
    // Requires swapping but the target has the same value already
    elif arr.[i] = arr.[arr.[i] - 1] then arr.[i] <- 0
    // Value is valid and requires swapping
    else swap arr i |> ignore

  /// First pass, put each value in its corresponding index
  for i in 0 .. Array.length arr - 1 do
    while not (arr.[i] = i + 1 || arr.[i] = 0) do
      processOneIndex arr i

  /// Second pass, find the first `0` or return `array.length + 1` as the result
  let zeroIdx = Array.tryFindIndex ((=) 0) arr
  match zeroIdx with
  | Some(i) -> i + 1
  | None -> Array.length arr + 1

(**
## Alternate solutions

In order to test this solution, we need to write a few alternate solutions to ensure that the results are correct.

I suggested three solutions above, and have implemented two of them below.

### Set-based solution

The first alternate solution solves the problem by creating two sets and performing set subtraction.  Sets will automatically remove duplicate values, so I do not need to handle that situation explicitly.
*)

let setSolver arr =
  // The set of original values, without any negative numbers.
  let origSet =
    arr
    |> Array.filter (fun e -> e >= 0)
    |> Set.ofArray

  // The set of "solution" values, with the minimum possible solution
  let plusOneSet =
    origSet
    |> Set.map (fun e -> e + 1)
    |> Set.add 1

  // Return the minimum value from the solution set that wasn't in the original set
  Set.difference plusOneSet origSet
  |> Set.minElement

(**
### Sort-based solution

In the second alternate solution, I solve the problem by sorting the array and finding the first pair of values that is not consecutive.  Duplicate values are explicitly removed to avoid issues with comparison later.
*)

let sortSolver arr =
  // Filter, sort, and de-duplicate the array
  let sortedArray =
    arr
    |> Array.filter (fun e -> e > 0)
    |> Array.sort
    |> Array.distinct

  if Array.contains 1 sortedArray then
    if Array.length sortedArray > 1 then
      // Find the first pair of numbers in the array that is not consecutive.
      sortedArray
      |> Seq.windowed 2
      |> Seq.filter (fun a -> a.[1] - a.[0] <> 1)
      |> Seq.tryHead
      |> function
        | Some(a) -> a.[0] + 1
        | None -> Array.length sortedArray + 1
    elif Array.length sortedArray = 1 then sortedArray.[0] + 1
    else 1
  else 1

(**
# Testing

## Base tests

Considering the "index math" happening in these solutions, we need to thoroughly test them to find any issues.  The first tests are to ensure that the two given test cases work correctly for each of the algorithms.

Having switched to [FSharp.Literate](https://fsprojects.github.io/FSharp.Formatting/literate.html), I'm happy to start using a property-based testing tool like [Hedgehog](https://github.com/hedgehogqa/fsharp-hedgehog).
*)

open Hedgehog

(** First, let's test the main solver. *)

(*** define-output:basesolver1 ***)
property {
  let l = [|3; 4; -1; 1|]
  return solver l = 2
}
|> Property.print' 1<tests>
(*** include-output:basesolver1 ***)

(*** define-output:basesolver2 ***)
property {
  let l = [|1; 2; 0|]
  return solver l = 3
}
|> Property.print' 1<tests>
(*** include-output:basesolver2 ***)

(** Next, we test the set-based solver. *)

(*** define-output:baseset1 ***)
property {
  let l = [|3; 4; -1; 1|]
  return setSolver l = 2
}
|> Property.print' 1<tests>
(*** include-output:baseset1 ***)

(*** define-output:baseset2 ***)
property {
  let l = [|1; 2; 0|]
  return setSolver l = 3
}
|> Property.print' 1<tests>
(*** include-output:baseset2 ***)

(** Finally, we can test the sort-based solver *)

(*** define-output:basesort1 ***)
property {
  let l = [|3; 4; -1; 1|]
  return sortSolver l = 2
}
|> Property.print' 1<tests>
(*** include-output:basesort1 ***)

(*** define-output:basesort2 ***)
property {
  let l = [|1; 2; 0|]
  return sortSolver l = 3
}
|> Property.print' 1<tests>
(*** include-output:basesort2 ***)

(**
## Additional property-based tests between the algorithms.

Now that the basic smoke tests are out of the way, we can move on to some more serious property-based testing.
*)

property {
  let! g = Gen.list (Range.)
}











(**

Welcome to FsLab journal
========================

FsLab journal is a simple Visual Studio template that makes it easy to do
interactive data analysis using F# Interactive and produce HTML or PDF
to document your research.

Next steps
----------

 * To see how things work, run `build run` from the terminal to start the journal
   runner in the background (or hit **F5** in Visual Studio). Executing this
   project will turn this F# script into a report.

 * To generate PDF from your experiments, you need to install `pdflatex` and
   have it accessible in the system `PATH` variable. Then you can run
   `build pdf` in the folder with this script (then check out `output` folder).

Sample experiment
-----------------

We start by referencing `Deedle` and `XPlot.GoogleCharts` libraries and then we
load the contents of *this* file:
*)

(*** define-output:loading ***)
open Deedle
open System.IO
open XPlot.GoogleCharts

let file = __SOURCE_DIRECTORY__ + "/P4.fsx"
let contents = File.ReadAllText(file)
printfn "Loaded '%s' of length %d" file contents.Length
(*** include-output:loading ***)

(**
Now, we split the contents of the file into words, count the frequency of
words longer than 3 letters and turn the result into a Deedle series:
*)
let words =
  contents.Split(' ', '"', '\n', '\r', '*')
  |> Array.filter (fun s -> s.Length > 3)
  |> Array.map (fun s -> s.ToLower())
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

