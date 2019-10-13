#![feature(test)]

extern crate test;

#[cfg(test)]
mod tests_p7_compare {
  /// Import for property-based testing.
  use proptest::prelude::*;

  /// Import the library code for testing.
  use p7::problem_7::*;

  /// Tests for the `p7` function.
  #[test]
  fn test_p7_compare_pb_all() {
    // Test for crashes.
    proptest!(|(x in "[0-9]*")| {
      let naive = p7(&x);
      prop_assert!(naive == p7_memoized(&x)
        && naive == p7_tail_recursive_ish(&x)
        && naive == p7_tail_recursive(&x));
    })
  }

  #[test]
  fn test_p7_compare_pb_valid_values() {
    proptest!(|(x in "[1-9][0-9]*")| {
      let naive = p7(&x);
      prop_assert!(naive == p7_memoized(&x)
        && naive == p7_tail_recursive_ish(&x)
        && naive == p7_tail_recursive(&x));
    })
  }
}
