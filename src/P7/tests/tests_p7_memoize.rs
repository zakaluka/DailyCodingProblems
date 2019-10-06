#![feature(test)]

extern crate test;

#[cfg(test)]
mod test_p7_memoize {
  /// Import for property-based testing.
  use proptest::prelude::*;

  /// Import the library code for testing.
  use p7::problem_7::*;

  /// Tests the `p7_memoize` function for crashes.
  #[test]
  fn test_p7m_pb_crash() {
    // Test for crashes.
    proptest!(|(x in "[0-9]*")| {
      p7_memoize(&x);
    })
  }

  #[test]
   fn test_p7m_pb_valid_values() {
    proptest!(|(x in "[1-9][0-9]*")| {
      p7_memoize(&x);
    })
  }

  /// Given test in problem statement.
  #[test]
  fn test_p7m_given_111() { assert_eq!(p7_memoize("111"), 3) }

  /// Simpler version of test provided in problem statement.
  #[test]
  fn test_p7m_11() { assert_eq!(p7_memoize("11"), 2) }
}
