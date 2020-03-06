use aoc2019_2::*;
use std::fs::File;
use std::io::{BufRead, BufReader};

/// Read the input file and turn into a `Model`.
fn read_input() -> Model {
  // Define the filename.
  let filename = "src/input_2a";

  // Open the file in read-only mode, ignoring errors.
  let file = File::open(filename).unwrap();

  // Open reader for the file.
  let mut reader = BufReader::new(file);

  // Read the first and only line.
  let mut buffer: String = "".into();
  let _ = reader.read_line(&mut buffer);
  buffer.into()
}

/// Execute Problem 2a
fn aoc2019_2a() -> () {
  let mut m = read_input();

  // Problem 2a instructions
  m.set(1, 12);
  m.set(2, 2);

  // Execute the Intcode program.
  let m = problem_2a(m);

  // Print the answer for Problem 2a.
  println!("Problem 2a: {}", m.get(0));
}

/// Execute Problem 2b
fn aoc2019_2b() -> () {
  // Problem 2b instructions
  // Loop parameter
  let mut position_1_value: i64 = 0;
  while position_1_value < 100 {
    let mut position_2_value: i64 = 0;
    while position_2_value < 100 {
      // Reset input Intcode for each execution
      let mut m = read_input();

      // Change values in Intcode.
      m.set(1, position_1_value);
      m.set(2, position_2_value);

      // Execute the Intcode.
      let m = problem_2a(m);

      // Output if the answer is found.
      if m.get(0) == 19690720 {
        println!(
          "Problem 2b: noun: {}, verb: {}, answer: {}",
          position_1_value,
          position_2_value,
          100 * position_1_value + position_2_value
        );
      }

      // Move forward with Position 2 value.
      position_2_value += 1;
    }

    // Move forward with Position 1 value.
    position_1_value += 1;
  }
}

fn main() {
  aoc2019_2a();
  aoc2019_2b();
}
