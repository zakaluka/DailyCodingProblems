use aoc2019_3::*;
use std::fs::File;
use std::io::{BufRead, BufReader};

fn aoc2019_3a() {
  // Define the filename.
  let filename = "src/input_3a";

  // Open the file in read-only mode, ignoring errors.
  let file = File::open(filename).unwrap();

  // Open reader for the file.
  let reader = BufReader::new(file);

  // Read lines 1 and 2
  let line1 = reader;

  // Continue from here.
  unimplemented!();
}

/// Main function for problem 3.
fn main() { aoc2019_3a(); }
