module DefaultTests

open AOC2019_3
open Expecto

[<Tests>]
let tests =
  testList "samples"
    [testCase "Say nothing" <| fun _ ->
      let subject = Say.nothing()
      Expect.equal subject () "Not an absolute unit"
     testCase "Say hello all" <| fun _ ->
       let subject = Say.hello "all"
       Expect.equal subject "Hello all" "You didn't say hello"]
