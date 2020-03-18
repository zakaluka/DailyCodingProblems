module RBTreeTests

open AOC2019_3
open Expecto

/// https://github.com/Skinney/core/blob/master/tests/tests/Test/Dict.elm
[<Tests>]
let buildTests =
  testList "RBTree Build Tests"
    [testCase "empty"
     <| fun _ -> Expect.equal (RBTree.empty) RBTree.E "empty"
     testCase "singleton" <| fun _ -> Expect.equal () () ""
     testCase "Say nothing" <| fun _ ->
       let subject = Say.nothing()
       Expect.equal subject () "Not an absolute unit"
     testCase "Say hello all" <| fun _ ->
       let subject = Say.hello "all"
       Expect.equal subject "Hello all" "You didn't say hello"]
