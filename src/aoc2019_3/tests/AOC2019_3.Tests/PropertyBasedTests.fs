module PropertyBasedTests

open AOC2019_3
open Expecto
open Hedgehog

[<Tests>]
let addTests =
  testList "Property-Based Add Tests"
    [ testCase "Add" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 10000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t = LLRBTree.ofArray g
          LLRBTree.count t < 10001 |> ignore
          ()
        }
        |> Property.check
      testCase "Add and Remove" <| fun _ ->
        property {
          let rnd = System.Random()
          let g = Array.create 5000 0 |> Array.map(fun _ -> rnd.Next())
          let h = Array.create 5000 0 |> Array.map(fun _ -> rnd.Next())
          let t =
            Array.fold (fun acc elt -> LLRBTree.add elt acc) LLRBTree.empty g
          Array.fold (fun acc elt -> LLRBTree.remove elt acc) t h |> ignore
        }
        |> Property.check
      testCase "Difference with itself" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 10000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t = LLRBTree.ofArray g
          LLRBTree.difference t t |> ignore
        }
        |> Property.check
      testCase "Union with itself" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 10000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t = LLRBTree.ofArray g
          LLRBTree.union t t |> ignore
        }
        |> Property.check
      testCase "Intersection with itself" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 10000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t = LLRBTree.ofArray g
          LLRBTree.intersect t t |> ignore
        }
        |> Property.check ]
