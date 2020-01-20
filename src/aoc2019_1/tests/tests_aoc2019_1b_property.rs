#![feature(test)]

/// Import for property-based testing.
use proptest::prelude::*;

/// Import the library code for testing.
use aoc2019_1::aoc2019_1b::{calculate_fuel, calculate_fuels};

// Property-based tests are located within this macro.
proptest! {
  /// Property-based tests for `calculate_fuel`.
  #[test]
  fn test_calculate_fuel(t: u64) {
    assert!(calculate_fuel(t) >= 0);
  }

  /// Property-based tests for `calculate_fuels`.
  #[test]
  fn test_calculate_fuels(t: Vec<u64>) {
    assert!(calculate_fuels(t) >= 0);
  }
}
