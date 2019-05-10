(*** hide ***)
#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"
#I @"packages/Hedgehog/lib/netstandard1.6"
#I @"packages/XPlot.GoogleCharts/lib/net45"
#r "Hedgehog.dll"
#r "XPlot.GoogleCharts.dll"

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

This means that we cannot change the space used by the algorithm as a function of the input size.  Or, in other words, I can't simply create a min-heap, for example, and ask for the minimum value because the heap's size will change each time depending on the size of the input array.  In case someone is thinking about creating a massive data structure to use for each execution of the algorithm (e.g. always create an Array of size MAX_INT for execution, regardless of the size of the input array) - while I guess that this is theoretically constant space, we run into the following problems:

1. This is a hugely wasteful approach to solving the problem.
1. This solution will fail if we can use a data structure larger than addressable memory, for example a data structure that can be stored on disk.
1. I don't believe this is in the spirit of the problem.

On a side note, the final solution will not be pretty because:

* Most of F#'s data structures, by default, are immutable.  _Luckily, Arrays are not._
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

    ```unk
        for each entry do
            while the entry's value is not 0 and is not index + 1 do
                swap values with the index represented by the value in the entry
    ```

1. Second pass.  Find the first entry where the value is `0`, return `index + 1`.

    ```unk
        Scan the array for the first entry with value of 0
        If found, return index + 1
        Else, return array length
    ```

When reading the code, there are two items to remember.  These may seem obvious, but I had to keep reminding myself courtesy of 0-based arrays in a situation where `0` has special meaning:

1. Given an `index`, the value there should be `index + 1`, i.e.`value = index + 1`
1. Given a `value`, the index it belongs to should be `value - 1`, i.e. `index = value - 1`
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
    else swap arr i

  /// First pass, put each value in its corresponding index
  for i in 0 .. Array.length arr - 1 do
    while not (arr.[i] = i + 1 || arr.[i] = 0) do
      processOneIndex arr i

  /// Second pass, find the first 0 or return (array.length + 1) as the result
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

  // Return the minimum value from the solution set that wasn't in the original
  // set
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
## Utility functions

Some utility functions to work with 3-tuples and to collect performance data using Windows Performance Counters.
*)

/// Get first value from 3-tuple.
let mfst (x,_,_) = x
let msnd (_,y,_) = y
let mtrd (_,_,z) = z

open System
open System.Diagnostics
open System.Threading

/// A list of all counter categories and names we want to track
let counterNames = [
  ".NET CLR LocksAndThreads", "# of current logical Threads"
  ".NET CLR LocksAndThreads", "# of current physical Threads"
  ".NET CLR Memory", "# Gen 0 Collections"
  ".NET CLR Memory", "# Gen 1 Collections"
  ".NET CLR Memory", "# Gen 2 Collections"
  ".NET CLR Memory", "Gen 0 heap size"
  ".NET CLR Memory", "Gen 1 heap size"
  ".NET CLR Memory", "Gen 2 heap size"
  ".NET CLR Memory", "Large Object Heap size"
  "Process", "Private Bytes"
  "Process", "% Processor Time"
]

/// Store readings from the counters
let mutable counterResults : ResizeArray<float32> [] =
  Array.init (List.length counterNames) (fun _ -> ResizeArray())

/// Cancellation token so that we can stop collection
let cancelToken = new CancellationTokenSource()

/// Async process to continuously collect the data, with 1 second sleep
let counters = async {
  let ctrs =
    counterNames
    |> List.map (fun (x, y) -> new PerformanceCounter(x, y,
                                Process.GetCurrentProcess().ProcessName))
  while (true) do
    ctrs
    |> List.iteri
      (fun i pc -> counterResults.[i].Add(pc.NextValue()))
    Thread.Sleep(1000)
}

// Kick off the collecton process
Async.Start (counters, cancelToken.Token)

/// A stopwatch to measure the overall program's run-time.
let overallStopwatch = Stopwatch()
overallStopwatch.Start()

(**
# Testing

## Base tests

Considering the "index math" happening in these solutions, we need to thoroughly test them to find any issues.  The first tests are to ensure that the two given test cases work correctly for each of the algorithms.

After much struggling with [FsCheck](https://github.com/fscheck/FsCheck), I'm happy to start using a property-based testing tool that works with Jupyter and FSharp.Literate, [Hedgehog](https://github.com/hedgehogqa/fsharp-hedgehog).

I am going to use a stopwatch to help measure runtimes since I am using FSharp.Literate instead of Jupyter for this post, and I can't figure out how to enable the `#time` directive through FSharp.Literate.
*)

open Hedgehog

/// The stopwatch that will measure run-times for the rest of the code.
let stopWatch = Stopwatch()

stopWatch.Start()

(**
First, let's test the main solver.
*)

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

(**
Next, we test the set-based solver.
*)

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

(**
Finally, we can test the sort-based solver.
*)

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

The first test will exercise all three algorithms over the full range of integers.
*)

(*** define-output:proptest1 ***)
property {
  let! g =
    Gen.array
    <| Range.exponential 0 10000
    <| Gen.int (Range.constant System.Int32.MinValue System.Int32.MaxValue)

  let result = solver g
  let setResult = setSolver g
  let sortResult = sortSolver g

  return result = setResult && result = sortResult
}
|> Property.print' 15000<tests>
(*** include-output:proptest1 ***)

(**
The second test tries various combinations of positive integers over larger arrays.
*)

(*** define-output:proptest2 ***)
property {
  let! g =
    Gen.array
    <| Range.exponential 0 20000
    <| Gen.int (Range.constant 1 1000)

  let result = solver g
  let setResult = setSolver g
  let sortResult = sortSolver g

  return result = setResult && result = sortResult
}
|> Property.print' 15001<tests>
(*** include-output:proptest2 ***)

(**
The third test ensures that 1-element arrays are handled correctly.
*)

(*** define-output:proptest3 ***)
property {
  let! g =
    Gen.array
    <| Range.constant 1 1
    <| Gen.int (Range.constant 1 2)

  let result = solver g
  let setResult = setSolver g
  let sortResult = sortSolver g

  return result = setResult && result = sortResult &&
    (if g.[0] = 1 then result = 2 else result = 1)
}
|> Property.print' 15002<tests>
(*** include-output:proptest3 ***)

(**
And a final edge-case test with empty arrays to ensure that the algorithms are working correctly.
*)

(*** define-output:proptest4 ***)
property {
  let! g =
    Gen.array
    <| Range.constant 0 0
    <| Gen.int (Range.constant 1 2)

  let result = solver g
  let setResult = setSolver g
  let sortResult = sortSolver g

  return result = setResult && result = sortResult && result = 1
}
|> Property.print' 15003<tests>
(*** include-output:proptest4 ***)

(**
And the runtime for the base tests is given below.
*)

stopWatch.Stop()

(*** define-output:basetestRuntime ***)
TimeSpan.FromTicks(stopWatch.ElapsedTicks).ToString("G")
|> printfn "%s"

stopWatch.Reset()

(**
Base Test Runtime was:
*)

(*** include-output:basetestRuntime ***)

(**
# Performance Testing

Now that the algorithms seem to be working correctly, the next step is to capture some performance metrics.  For this portion, I will be using `System.Diagnostics.Stopwatch` to measure each algorithm's speed.  I believe my last post proved that, as of now, I cannot benchmark memory usage reliably.

First, we can setup a generator to generate random values for testing.  Similar to the base testing, let's benchmark the runtime of the test data generation.
*)

let arrayGen : Gen<int []> =
  gen {
    let! g =
      Gen.array
      <| Range.exponential 1000 10000
      <| Gen.int (Range.constant 1 1000)
    return g
  }

let numX = 100
let numY = 100

// Begin measuring data generation runtime
stopWatch.Start()

let tc =
  [|
    for _ in 1 .. numX do
      yield arrayGen |> Gen.sample Size.MaxValue numY |> List.toArray
  |]
  |> Array.fold Array.append [||]

stopWatch.Stop()

(*** define-output:dataGenRuntime ***)
TimeSpan.FromTicks(stopWatch.ElapsedTicks).ToString("G")
|> printfn "%s"

stopWatch.Reset()

(**
Data Generation Runtime was:
*)

(*** include-output:dataGenRuntime ***)

(**
Now that we have our profiling data, the next step is to measure performance.

First, let's create a bare minimum set of utility 'tools' to enable the performance monitoring.
*)

/// The number of runs to perform, so average the performance.
let numRuns = 500

/// Function that helps measure the average runtime based on the number of runs
let measurement numRuns alg lst =
  // Measures actual passage of time, not just time spent in the algorithm
  let swRT = Stopwatch()

  // Measures GC runtime
  let swGC = Stopwatch()

  swRT.Start()
  // Reset the stopwatch when starting a new measurement.
  stopWatch.Reset()
  for _ in 1 .. numRuns do
    // Perform a forced, blocking garbage collection with compaction, but don't
    // penalize the algorithm.
    swGC.Start()
    System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced,
      true)
    swGC.Stop()

    // Start the stopwatch.
    stopWatch.Start()

    // Run the algorithm on the entire input list.
    for i in lst do alg i |> ignore

    // Stop the stopwatch
    stopWatch.Stop()

  // Stop the measurement of "real passage" of time.
  swRT.Stop()

  // Return algorithm, GC, and total runtimes
  System.TimeSpan.FromTicks(stopWatch.ElapsedTicks / (int64 numRuns)).Ticks,
  swGC.ElapsedTicks,
  swRT.ElapsedTicks

(**
## Solver

Let's start by benchmarking the base `solver` algorithm.
*)

let solverRuntime = measurement numRuns solver tc

(*** define-output:solverRuntime ***)
printfn "Solver algorithm time %s, gc time %s, and total time %s"
  (TimeSpan.FromTicks(mfst solverRuntime).ToString("G"))
  (TimeSpan.FromTicks(msnd solverRuntime).ToString("G"))
  (TimeSpan.FromTicks(mtrd solverRuntime).ToString("G"))
(*** include-output:solverRuntime ***)

(**
## Set Solver

The next benchmark is with the `setSolver` algorithm.
*)

let setSolverRuntime = measurement numRuns setSolver tc

(*** define-output:setSolverRuntime ***)
printfn "Set Solver algorithm time %s, gc time %s, and total time %s"
  (TimeSpan.FromTicks(mfst setSolverRuntime).ToString("G"))
  (TimeSpan.FromTicks(msnd setSolverRuntime).ToString("G"))
  (TimeSpan.FromTicks(mtrd setSolverRuntime).ToString("G"))
(*** include-output:setSolverRuntime ***)

(**
## Sort Solver

And finally, the benchmark for the `sortSolver` algorithm.
*)

let sortSolverRuntime = measurement numRuns sortSolver tc

(*** define-output:sortSolverRuntime ***)
printfn "Sort Solver algorithm time %s, gc time %s, and total time %s"
  (TimeSpan.FromTicks(mfst sortSolverRuntime).ToString("G"))
  (TimeSpan.FromTicks(msnd sortSolverRuntime).ToString("G"))
  (TimeSpan.FromTicks(mtrd sortSolverRuntime).ToString("G"))
(*** include-output:sortSolverRuntime ***)

(**
# Results

So, what were the results?  First, let's chart the runtimes as measured by the stopwatches (algorithm `Alg`, garbage collection `GC`, and total time `TT`) and some comparative percentages.
*)

let rows = [
  "Solver"
  "Set Solver"
  "Sort Solver"
]

let columns = [
  "Algorithm"
  "Alg relative"
  "Runtime (Alg)"
  "Runtime (GC)"
  "Runtime (TT)"
  "GC/Alg"
  "TT/Alg"
]

let algRes = [mfst solverRuntime; mfst setSolverRuntime; mfst sortSolverRuntime]
let gcRes = [msnd solverRuntime; msnd setSolverRuntime; msnd sortSolverRuntime]
let ttRes = [mtrd solverRuntime; mtrd setSolverRuntime; mtrd sortSolverRuntime]

open XPlot.GoogleCharts

(*** define-output:table1 ***)
[ // Compare stopwatch runtimes to minimum stopwatch runtime
  yield
    algRes
    |> List.map (fun e -> 100L * e / (List.min algRes)
                          |> fun x -> (string x) + "%")
    |> List.zip rows
  // Raw runtimes for algorithm, GC, and total time
  for i in [algRes; gcRes; ttRes] do
    yield
      List.map (fun r -> System.TimeSpan.FromTicks(r).ToString("G")) i
      |> List.zip rows
  // Compare GC runtimes to stopwatch runtimes
  yield
    List.fold2 (fun acc x y -> acc @ [100L * y / x])
      [] algRes gcRes
    |> List.map (fun x -> (string x) + "%")
    |> List.zip rows
  // Compare total runtimes to algorithm runtimes
  yield
    List.fold2 (fun acc x y -> acc @ [100L * y / x])
      [] algRes ttRes
    |> List.map (fun x -> (string x) + "%")
    |> List.zip rows
]
|> Chart.Table
|> Chart.WithOptions(Options(title="Runtime comparisons",showRowNumber=true))
|> Chart.WithLabels columns
(*** include-it:table1 ***)

(*** define-output:algBar1 ***)
algRes
|> List.map (fun e -> TimeSpan.FromTicks(e).TotalSeconds)
|> List.zip rows
|> Chart.Bar
|> Chart.WithOptions(Options(title="Algorithm runtime comparisons (seconds)"))
(*** include-it:algBar1 ***)

(**
## Performance Counter statistics

While executing the workbook, I collected a number of values using Performance Counters on Windows.  These were collected throughout the life of the book's execution.  For descriptions of these counters, please refer to the [.NET Performance Counters page](https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/performance-counters).

- .NET CLR LocksAndThreads
    - # of current logical Threads
    - # of current physical Threads
- .NET CLR Memory
    - # Gen 0 Collections
    - # Gen 1 Collections
    - # Gen 2 Collections
    - Gen 0 heap size
    - Gen 1 heap size
    - Gen 2 heap size
    - Large Object Heap size
- Process
    - Private Bytes
    - % Processor Time

I thought that it would be interesting to chart these values out.

However, the first thing we need to do is to stop collecting the data.
*)

cancelToken.Cancel()

(**
### .NET CLR LocksAndThreads, # of Current Threads
*)

let minThreadSize =
  [counterResults.[0].Count;counterResults.[1].Count]
  |> List.min

(*** define-output:lat ***)
[ yield List.init
    minThreadSize
    (fun i -> i, counterResults.[0].Item i)
  yield List.init
    minThreadSize
    (fun i -> i, counterResults.[1].Item i)
]
|> Chart.Line
|> Chart.WithOptions
  (Options(title = ".NET CLR LocksAndThreads, # of current Threads",
    curveType = "function",
    legend = Legend(position = "bottom")))
|> Chart.WithLabels
  ["# of current logical Threads"; "# of current physical Threads"]
(*** include-it:lat ***)

(**
### .NET CLR Memory, # of Collections
*)

let minCollectionSize =
  [counterResults.[2].Count;counterResults.[3].Count;counterResults.[4].Count]
  |> List.min

(*** define-output:mGC ***)
[ yield List.init
    minCollectionSize
    (fun i -> i, counterResults.[2].Item i)
  yield List.init
    minCollectionSize
    (fun i -> i, counterResults.[3].Item i)
  yield List.init
    minCollectionSize
    (fun i -> i, counterResults.[4].Item i)
]
|> Chart.Line
|> Chart.WithOptions
  (Options(title = ".NET CLR Memory, # of Collections",
    curveType = "function",
    legend = Legend(position = "bottom")))
|> Chart.WithLabels
  ["# Gen 0 Collections"; "# Gen 1 Collections"; "# Gen 2 Collections"]
(*** include-it:mGC ***)

(**
### .NET CLR Memory, Heap size
*)

let minHeapSize =
  [counterResults.[5].Count;counterResults.[6].Count;counterResults.[7].Count;
  counterResults.[8].Count]
  |> List.min

(*** define-output:mGS ***)
[ yield List.init
    minHeapSize
    (fun i -> i, counterResults.[5].Item i)
  yield List.init
    minHeapSize
    (fun i -> i, counterResults.[6].Item i)
  yield List.init
    minHeapSize
    (fun i -> i, counterResults.[7].Item i)
  yield List.init
    minHeapSize
    (fun i -> i, counterResults.[8].Item i)
]
|> Chart.Line
|> Chart.WithOptions
  (Options(title = ".NET CLR Memory, Heap size",
    curveType = "function",
    legend = Legend(position = "bottom")))
|> Chart.WithLabels
  [ "Gen 0 heap size"; "Gen 1 heap size"; "Gen 2 heap size";
  "Large Object Heap size"]
(*** include-it:mGS ***)

(**
### Process, Private Bytes
*)

(*** define-output:pPB ***)
List.init
  (counterResults.[9].Count)
  (fun i -> i, counterResults.[9].Item i)
|> Chart.Line
|> Chart.WithOptions
  (Options(title = "Process, Private Bytes",
    curveType = "function",
    legend = Legend(position = "bottom")))
|> Chart.WithLabel "Private Bytes"
(*** include-it:pPB ***)

(**
### Process, % Processor Time
*)

(*** define-output:pPT ***)
List.init
  (counterResults.[10].Count)
  (fun i -> i, counterResults.[10].Item i)
|> Chart.Line
|> Chart.WithOptions
  (Options(title = "Process, % Processor Time",
    curveType = "function",
    legend = Legend(position = "bottom")))
|> Chart.WithLabel "% Processor Time"
(*** include-it:pPT ***)

(**
# Conclusion

_NOTE: This section was written after the notebook ran once.  To re-generate the output with this paragraph, the entire notebook ran again.  So, if I refer to any values below, they may not exactly match the measurements shown above._

TODO: fill this in.
*)

overallStopwatch.Stop()

(*** define-output:overallStopwatch ***)
TimeSpan.FromTicks(overallStopwatch.ElapsedTicks).ToString("G")
|> printfn "%s"

(**
And finally, the overall run-time of this notebook is...
*)

(*** include-output:overallStopwatch ***)
