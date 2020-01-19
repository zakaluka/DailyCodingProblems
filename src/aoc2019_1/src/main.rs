use aoc2019_1::{aoc2019_1a, aoc2019_1b};
use std::fs::File;
use std::io::{BufRead, BufReader};

/// Main function for problem 1a.
fn aoc2019_1a() {
  // Define the filename.
  let filename = "src/input_1a";

  // Open the file in read-only mode, ignoring errors.
  let file = File::open(filename).unwrap();

  // Open reader for the file.
  let reader = BufReader::new(file);

  // Read each line, try to parse it to a u64, and convert the iterator to a
  // vector.
  let masses = reader
    .lines()
    .map(|line_result| line_result.unwrap())
    .map(|line| line.parse::<u64>().unwrap())
    .collect::<Vec<u64>>();

  // Calculate the fuel for the input mass.
  let fuel = aoc2019_1a::calculate_fuels(masses);

  // Print the calculated fuel.
  println!("Fuel for masses (1a) = {}", fuel);

  // Re-assert that logic hasn't broken for problem 1a.
  assert_eq!(fuel, 3_317_100);
}

/// Main function for problem 1b.
fn aoc2019_1b() {
  // Define the filename - uses the same input as problem 1a.
  let filename = "src/input_1a";

  // Open the file in read-only mode, ignoring errors.
  let file = File::open(filename).unwrap();

  // Open reader for the file.
  let reader = BufReader::new(file);

  // Read each line, try to parse it to a u64, and convert the iterator to a
  // vector.
  let masses = reader
    .lines()
    .map(|line_result| line_result.unwrap())
    .map(|line| line.parse::<u64>().unwrap())
    .collect::<Vec<u64>>();

  // Calculate the fuel for the input mass.
  let fuel = aoc2019_1b::calculate_fuels(masses);

  // Print the calculated fuel.
  println!("Fuel for masses (1b) = {}", fuel);

  // Re-assert that logic hasn't broken for problem 1b.
  assert_eq!(fuel, 4_972_784);
}

fn main() {
  aoc2019_1a();
  aoc2019_1b();
}
