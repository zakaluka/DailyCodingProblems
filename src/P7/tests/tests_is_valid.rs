#![feature(test)]

extern crate test;

#[cfg(test)]
mod tests_is_valid {
  /// Import for property-based testing.
  use proptest::prelude::*;

  /// Import the library code for testing.
  use p7::problem_7::*;

  /// Tests for the constants.
  #[test]
  fn test_constants() {
    assert_eq!(LOWER_LIMIT, 1);
    assert_eq!(UPPER_LIMIT, 26);
    assert_eq!(INVALID_INT, -1);
  }

  /// Test `is_valid()` for crashes, using property-based testing.
  #[test]
  fn test_is_valid_pb_all() {
    // Test for crashes
    proptest!(|(x in "[0-9]*")| {
      is_valid(x.as_bytes())
    });
  }

  /// Test `is_valid()` with invalid values starting with `0`, using
  /// property-based testing.
  #[test]
  fn test_is_valid_pb_start_eq_0() {
    // Test for invalid values starting with `0`.
    proptest!(|(x in "0[0-9]*")| {
      prop_assert_eq!(is_valid(x.as_bytes()), false)
    });
  }

  /// Test `is_valid()` with potentially valid values starting with `1-9`,
  /// using property-based testing.
  #[test]
  fn test_is_valid_pb_start_ne_0() {
    proptest!(|(x in "[1-9][0-9]*")| {
      let n = x.parse::<i32>().unwrap_or(INVALID_INT);
      prop_assert_eq!(is_valid(x.as_bytes()),
        n >= LOWER_LIMIT && n <= UPPER_LIMIT)
    });
  }

  /// Test `is_valid()` with any i32 value.  This is used for benchmarks as
  /// well.
  #[test]
  fn test_is_valid_pb_i32() {
    proptest!(|(x: i32)| {
      let y = format!("{}", x);
      is_valid(y.as_bytes());
      prop_assert_eq!(is_valid(y.as_bytes()),
        LOWER_LIMIT <= x && x <= UPPER_LIMIT)
    })
  }

  /// Test `is_valid()` with only valid values, using property-based
  /// testing.
  #[test]
  fn test_is_valid_pb_valid_only() {
    proptest!(|(y in 1..26 as i32)| {
      let x = format!("{}", y);
      prop_assert_eq!(is_valid(x.as_bytes()), true)
    })
  }

  /// Test `is_valid()` with only invalid values, using property-based
  /// testing.
  #[test]
  fn test_is_valid_pb_invalid_only() {
    proptest!(|(x: i32)| {
      prop_assume!(!(LOWER_LIMIT <= x && x <= UPPER_LIMIT));
      let y = format!("{}", x);
      is_valid(y.as_bytes());
      prop_assert_eq!(is_valid(y.as_bytes()), false)
    })
  }

  /// Specific test case that failed during property-based testing on the
  /// empty string.
  #[test]
  fn test_is_valid_empty_string() { is_valid("".as_bytes()); }
}
