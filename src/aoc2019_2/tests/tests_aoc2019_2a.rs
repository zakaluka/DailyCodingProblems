/// Integration / public tests for the logic, primarily based on examples
/// provided in the problem statement.
#[cfg(test)]
mod tests_aoc2019_2a {
  use aoc2019_2::*;
  use std::fs::File;
  use std::io::{BufRead, BufReader};

  #[test]
  fn sample_1() {
    let m = problem_2a("1,0,0,0,99".into());
    assert_eq!(m.get(0 as usize), 2);
    assert_eq!(m.get(1 as usize), 0);
    assert_eq!(m.get(2 as usize), 0);
    assert_eq!(m.get(3 as usize), 0);
    assert_eq!(m.get(4 as usize), 99);
  }

  #[test]
  fn sample_2() {
    let m = problem_2a("2,3,0,3,99".into());
    assert_eq!(m.get(0 as usize), 2);
    assert_eq!(m.get(1 as usize), 3);
    assert_eq!(m.get(2 as usize), 0);
    assert_eq!(m.get(3 as usize), 6);
    assert_eq!(m.get(4 as usize), 99);
  }

  #[test]
  fn sample_3() {
    let m = problem_2a("2,4,4,5,99,0".into());
    assert_eq!(m.get(0 as usize), 2);
    assert_eq!(m.get(1 as usize), 4);
    assert_eq!(m.get(2 as usize), 4);
    assert_eq!(m.get(3 as usize), 5);
    assert_eq!(m.get(4 as usize), 99);
    assert_eq!(m.get(5 as usize), 9801);
  }

  #[test]
  fn sample_4() {
    let m = problem_2a("1,1,1,4,99,5,6,0,99".into());
    assert_eq!(m.get(0 as usize), 30);
    assert_eq!(m.get(1 as usize), 1);
    assert_eq!(m.get(2 as usize), 1);
    assert_eq!(m.get(3 as usize), 4);
    assert_eq!(m.get(4 as usize), 2);
    assert_eq!(m.get(5 as usize), 5);
    assert_eq!(m.get(6 as usize), 6);
    assert_eq!(m.get(7 as usize), 0);
    assert_eq!(m.get(8 as usize), 99);
  }

  #[test]
  fn solution_2a() {
    // Define the filename.
    let filename = "src/input_2a";

    // Open the file in read-only mode, ignoring errors.
    let file = File::open(filename).unwrap();

    // Open reader for the file.
    let mut reader = BufReader::new(file);

    // Read the first and only line.
    let mut buffer: String = "".into();
    let _ = reader.read_line(&mut buffer);
    let mut m: Model = buffer.into();

    // Problem 2a instructions
    m.set(1, 12);
    m.set(2, 2);

    // Execute the Intcode program.
    let m = problem_2a(m);

    assert_eq!(m.get(0), 4930687);
  }
}
