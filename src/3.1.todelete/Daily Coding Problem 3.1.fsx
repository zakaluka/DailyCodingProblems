(**
# First-level heading
Some more documentation using `Markdown`.
*)

let helloWorld() = printfn "Hello world!"

(**
## Second-level heading

With some more documentation

### Evaluation demo

The following is a simple calculation:

#### How low

test 4

##### can we

test 5

###### go?

test 6
*)

let test = 40 + 2

(** We can print it as follows: *)
(*** define-output:test ***)
printf "Result is: %d" test

(** The result of the previous snippet is: *)
(*** include-output:test ***)

(** And the variable `test` has the following value: *)
(*** include-value: test ***)
