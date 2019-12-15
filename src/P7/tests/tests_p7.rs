#![feature(test)]

extern crate test;

#[cfg(test)]
mod tests_p7 {
  /// Import for property-based testing.
  use proptest::prelude::*;

  /// Import the library code for testing.
  use p7::problem_7::*;

  /// Tests using values starting with any possible integer.
  #[test]
  fn test_p7_pb_all() {
    // Test for crashes.
    proptest!(|(x in "[0-9]*")| {
      p7(&x);
    })
  }

  /// Tests using values not starting with `0`.
  #[test]
  fn test_p7_pb_valid_values() {
    proptest!(|(x in "[1-9][0-9]*")| {
      p7(&x);
    })
  }

  /// Given test in problem statement.
  #[test]
  fn test_p7_given_111() { assert_eq!(p7("111"), 3) }

  /// Simpler version of test provided in problem statement.
  #[test]
  fn test_p7_11() { assert_eq!(p7("11"), 2) }

  #[test]
  fn test_p7_for_benchmark() {
    for i in 0..100 {
      assert_eq!(p7("12131415161718191010918171"), 1280)
    }
  }
}
