module PropertyBasedTests

open Expecto
open Hedgehog
open ZN.DataStructures

[<Tests>]
let addTests =
  testList "Property-Based Add Tests"
    [ testCase "Add" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 5000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t = LLRBTree.ofArray g
          Expect.equal (LLRBTree.count t) (Array.length g) "Add equal"
          ()
        }
        |> Property.check
      testCase "Remove" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 5000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t =
            Array.fold (fun acc elt -> LLRBTree.add elt acc) LLRBTree.empty g
          let t2 = Array.fold (fun acc elt -> LLRBTree.remove elt acc) t g
          Expect.equal (LLRBTree.count t2) 0 "Remove count"
          Expect.equal t2 LLRBTree.empty "Remove tree"
        }
        |> Property.check
      testCase "Remove 2" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 5000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t =
            Array.fold (fun acc elt -> LLRBTree.add elt acc) LLRBTree.empty g
          let t2 = Array.foldBack LLRBTree.remove g t
          Expect.equal (LLRBTree.count t2) 0 "Remove 2 count"
          Expect.equal t2 LLRBTree.empty "Remove 2 tree"
        }
        |> Property.check
      testCase "Add and Remove" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 5000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let! h = Gen.array <| Range.exponential 0 5000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t =
            Array.fold (fun acc elt -> LLRBTree.add elt acc) LLRBTree.empty g
          let t2 = Array.fold (fun acc elt -> LLRBTree.remove elt acc) t h
          Expect.isLessThanOrEqual (LLRBTree.count t2) (LLRBTree.count t)
            "Add and Remove count"
          Expect.equal (LLRBTree.count t2)
            (Set.difference (Set.ofArray g) (Set.ofArray h) |> Set.count)
            "Add and Remove set count"
          Expect.equal (Set.difference (Set.ofArray g) (Set.ofArray h))
            (LLRBTree.toSet t2) "Add and Remove set"
        }
        |> Property.check
      testCase "Difference with itself" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 5000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t = LLRBTree.ofArray g
          let t2 = LLRBTree.difference t t
          Expect.equal (LLRBTree.count t2) 0 "Difference with itself count"
          Expect.equal t2 LLRBTree.empty "Difference with itself tree"
        }
        |> Property.check
      testCase "Union with itself" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 5000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t = LLRBTree.ofArray g
          let t2 = LLRBTree.union t t
          Expect.equal t t2 "Union with itself"
          Expect.equal (LLRBTree.count t) (LLRBTree.count t2)
            "Union with itself count"
        }
        |> Property.check
      testCase "Intersection with itself" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 5000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t = LLRBTree.ofArray g
          let t2 = LLRBTree.intersect t t
          Expect.equal (LLRBTree.toList t) (LLRBTree.toList t2)
            "Intersection with itself"
          Expect.equal (LLRBTree.count t) (LLRBTree.count t2)
            "Intersection with itself count"
        }
        |> Property.check
      testCase "tryPick and Pick" <| fun _ ->
        property {
          let! g = Gen.array <| Range.exponential 0 5000
                   <| Gen.int
                        (Range.constant System.Int32.MinValue
                           System.Int32.MaxValue)
          let t = LLRBTree.ofArray g

          let picker =
            fun e ->
              if e < 0 then Some e else None

          let tp = LLRBTree.tryPick picker t
          match tp with
          | None ->
              Expect.throwsT<System.Collections.Generic.KeyNotFoundException> (fun () ->
                LLRBTree.pick picker t |> ignore) "tryPick and Pick"
          | Some(x) ->
              Expect.equal x (LLRBTree.pick picker t) "tryPick and Pick"
        }
        |> Property.check ]
