pub mod aoc2019_1a {
  /// Calculate fuel usage as follows:
  ///
  /// 1. Take mass
  /// 1. Divide by 3
  /// 1. Round down
  /// 1. Subtract 2
  ///
  /// Assumptions:
  ///
  /// 1. Mass is an integer
  /// 1. Integer division in Rust automatically drops the fractional portion
  /// (aka, round-down is built into integer division)
  pub fn calculate_fuel(mass: u64) -> u64 {
    let denominator = 3u64;
    let subtract_from = 2u64;

    (mass.div_euclid(denominator)) - subtract_from
  }

  /// Calculates fuel usage for multiple modules at once.
  pub fn calculate_fuels(mass: Vec<u64>) -> u64 {
    mass
      .iter()
      .map(|e| calculate_fuel(*e))
      .fold(0, |acc, e| acc + e)
  }
}

pub mod aoc2019_1b {
  /// Calculate fuel usage as follows.  Recursively calculates fuel requirements
  /// for the fuel added to the mass until no additional fuel is needed
  /// (`calculate_fuel` returns a `0` as the result).
  ///
  /// 1. Take mass
  /// 1. Divide by 3
  /// 1. Round down
  /// 1. Subtract 2
  ///
  /// Assumptions:
  ///
  /// 1. Mass is an integer.
  /// 1. Integer division in Rust automatically drops the fractional portion
  /// (aka, round-down is built into integer division).
  /// 1. `u64` will panic if attempting to reduce the value below `0`.
  pub fn calculate_fuel(mass: u64) -> u64 {
    // Safe helper to calculate fuel needed for a single mass while avoiding a
    // panic with `u64` subtraction.
    fn calculate_single_fuel(mass: u64) -> u64 {
      let denominator = 3u64;
      let subtract_from = 2u64;

      // Ensure that there is no overflow for `u64`.
      let div_result: u64 = mass.div_euclid(denominator);
      if div_result < 2 {
        0
      } else {
        div_result - subtract_from
      }
    }

    // Tail-recursive helper to calculate total fuel needed.
    fn cf_helper(acc: u64, mass: u64) -> u64 {
      if mass == 0 {
        acc
      } else {
        let fuel = calculate_single_fuel(mass);
        cf_helper(acc + fuel, fuel)
      }
    }

    cf_helper(0, mass)
  }

  /// Calculates fuel usage for multiple modules at once.
  pub fn calculate_fuels(mass: Vec<u64>) -> u64 {
    mass
      .iter()
      .map(|e| calculate_fuel(*e))
      .fold(0, |acc, e| acc + e)
  }
}
