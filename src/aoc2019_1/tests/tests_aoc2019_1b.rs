/// Integration / public tests for the logic, primarily based on examples
/// provided in the problem statement.
#[cfg(test)]
mod tests_aoc2019_1b {
  use aoc2019_1::aoc2019_1b::{calculate_fuel, calculate_fuels};

  /// Sample #1 provided by problem, mass of 12 converts to 2.
  #[test]
  fn sample_1() {
    assert_eq!(calculate_fuel(12), 2);
  }

  /// Sample #2 provided by problem, mass of 14 converts to 2.
  #[test]
  fn sample_2() {
    assert_eq!(calculate_fuel(14), 2);
  }

  /// Sample #3 provided by problem, mass of 1_969 converts to 966.
  #[test]
  fn sample_3() {
    assert_eq!(calculate_fuel(1_969), 966);
  }

  /// Sample #4 provided by problem, mass of 100_756 converts to 50_346.
  #[test]
  fn sample_4() {
    assert_eq!(calculate_fuel(100_756), 50_346);
  }

  /// Combine samples into 1 call, mass of `[12, 14, 1_969, 100_756]` converts
  /// to 51_316.
  #[test]
  fn sample_combined() {
    assert_eq!(
      calculate_fuels([12u64, 14, 1_969, 100_756].to_vec()),
      51_316
    );
  }
}
