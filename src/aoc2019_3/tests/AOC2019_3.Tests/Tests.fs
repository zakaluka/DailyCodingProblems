module Tests

open AOC2019_3
open Expecto
open System

/// https://github.com/Skinney/core/blob/master/tests/tests/Test/Dict.elm
/// Build Tests with Collections
[<Tests>]
let buildTestsFromCollections =
  testList "Build Tests from Collections"
    [ testCase "Empty 1" <| fun _ -> Expect.equal LLRBTree.empty E "Empty 1"
      testCase "Empty 2"
      <| fun _ ->
        Expect.allEqual
          [ LLRBTree.ofList []
            LLRBTree.ofArray [||]
            LLRBTree.ofSeq Seq.empty
            LLRBTree.ofSet Set.empty ] LLRBTree.empty "Empty 2"
      testCase "Singleton"
      <| fun _ ->
        Expect.allEqual
          [ LLRBTree.ofList [ "a" ]
            LLRBTree.ofArray [| "a" |]
            LLRBTree.ofSeq(seq { "a" })
            LLRBTree.ofSet(Set.singleton "a") ] (LLRBTree.singleton "a")
          "Singleton"
      testCase "Insert"
      <| fun _ ->
        Expect.allEqual
          [ LLRBTree.ofList [ "a" ]
            LLRBTree.ofArray [| "a" |]
            LLRBTree.ofSeq(seq { "a" })
            LLRBTree.ofSet(Set.singleton "a") ]
          (LLRBTree.insert "a" LLRBTree.empty) "Insert"
      testCase "Remove"
      <| fun _ ->
        Expect.allEqual
          [ LLRBTree.ofList [ "a" ] |> LLRBTree.remove "a"
            LLRBTree.ofArray [| "a" |] |> LLRBTree.remove "a"
            LLRBTree.ofSeq(seq { "a" }) |> LLRBTree.remove "a"
            LLRBTree.ofSet(Set.singleton "a") |> LLRBTree.remove "a"
            LLRBTree.singleton "a" |> LLRBTree.remove "a"
            LLRBTree.insert "a" LLRBTree.empty |> LLRBTree.remove "a" ]
          LLRBTree.empty "Remove"
      testCase "Remove Not Found"
      <| fun _ ->
        Expect.allEqual
          [ LLRBTree.ofList [ "a" ] |> LLRBTree.remove "b"
            LLRBTree.ofArray [| "a" |] |> LLRBTree.remove "b"
            LLRBTree.ofSeq(seq { "a" }) |> LLRBTree.remove "b"
            LLRBTree.ofSet(Set.singleton "a") |> LLRBTree.remove "b"
            LLRBTree.singleton "a" |> LLRBTree.remove "b"
            LLRBTree.insert "a" LLRBTree.empty |> LLRBTree.remove "b" ]
          (LLRBTree.singleton "a") "Remove Not Found"
      testCase "Transition between collections"
      <| fun _ ->
        Expect.equal
          ([ "a";"b";"c" ]
           |> LLRBTree.ofList
           |> LLRBTree.toList
           |> LLRBTree.ofList
           |> LLRBTree.toArray
           |> LLRBTree.ofArray
           |> LLRBTree.toSeq
           |> LLRBTree.ofSeq
           |> LLRBTree.toSet
           |> LLRBTree.ofSet
           |> LLRBTree.toList) [ "a";"b";"c" ] "Transition between collections" ]

/// Query Tests
[<Tests>]
let queryTests =
  let animals = [ "Dog";"Cat";"Mongoose";"Rat" ] |> LLRBTree.ofList
  testList "Query tests"
    [ testCase "Member 1" <| fun _ ->
        Expect.allEqual
          [ LLRBTree.contains "Dog" animals
            LLRBTree.contains "Cat" animals
            LLRBTree.contains "Mongoose" animals
            LLRBTree.contains "Rat" animals ] true "Member 1"
      testCase "Member 2"
      <| fun _ ->
        Expect.allEqual
          [ LLRBTree.contains "Donkey" animals
            LLRBTree.contains "Monkey" animals
            LLRBTree.contains "Elephant" animals
            LLRBTree.contains "Giraffe" animals
            LLRBTree.contains "Zebra" animals ] false "Member 2"
      testCase "Get 1" <| fun _ ->
        Expect.allEqual
          [ LLRBTree.get "Cat" animals
            |> Option.get
            |> LLRBTree.getValue ] "Cat" "Get 1"
      testCase "Get 2" <| fun _ ->
        Expect.allEqual
          [ LLRBTree.get "Dog" animals
            |> Option.get
            |> LLRBTree.getValue ] "Dog" "Get 2"
      testCase "Get 3" <| fun _ ->
        Expect.allEqual
          [ LLRBTree.get "Mongoose" animals
            |> Option.get
            |> LLRBTree.getValue ] "Mongoose" "Get 3"
      testCase "Get 4" <| fun _ ->
        Expect.allEqual
          [ LLRBTree.get "Rat" animals
            |> Option.get
            |> LLRBTree.getValue ] "Rat" "Get 4"
      testCase "Get 5" <| fun _ ->
        Expect.allEqual
          [ LLRBTree.get "Aardvark" animals
            LLRBTree.get "Chicken" animals
            LLRBTree.get "Zebu" animals
            LLRBTree.get "Sailfish" animals
            LLRBTree.get "Narwhal" animals
            LLRBTree.get "Eagle" animals ] None "Get 5" ]
