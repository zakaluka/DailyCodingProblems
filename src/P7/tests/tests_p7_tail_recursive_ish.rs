#![feature(test)]

extern crate test;

#[cfg(test)]
mod tests_p7_tail_recursive_ish {
  /// Import for property-based testing.
  use proptest::prelude::*;

  /// Import the library code for testing.
  use p7::problem_7::*;

  /// Tests using values starting with any possible integer.
  #[test]
  fn test_p7tri_pb_all() {
    // Test for crashes.
    proptest!(|(x in "[0-9]*")| {
      p7_tail_recursive_ish(&x);
    })
  }

  /// Tests using values not starting with `0`.
  #[test]
  fn test_p7tri_pb_valid_values() {
    proptest!(|(x in "[1-9][0-9]*")| {
      p7_tail_recursive_ish(&x);
    })
  }

  /// Given test in problem statement.
  #[test]
  fn test_p7tri_given_111() { assert_eq!(p7_tail_recursive_ish("111"), 3) }

  /// Simpler version of test provided in problem statement.
  #[test]
  fn test_p7tri_11() { assert_eq!(p7_tail_recursive_ish("11"), 2) }

  /// Failing test case from property-based testing.
  #[test]
  fn test_p7tri_1111111101() { p7_tail_recursive_ish("1111111101"); }

  /// Failing test case from property-based testing.
  #[test]
  fn test_p7tri_11111111010() { p7_tail_recursive_ish("11111111010"); }

  #[test]
  fn test_p7tri_for_benchmark() {
    assert_eq!(p7_tail_recursive_ish("12131415161718191010918171"), 1280)
  }
}
