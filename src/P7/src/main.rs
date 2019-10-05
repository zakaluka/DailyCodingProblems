#![feature(test)]

extern crate test;

/// # Problem 7
///
/// The problem statement is as follows:
///
/// > Given the mapping a = 1, b = 2, ... z = 26, and an encoded message, count
/// > the number of ways it can be decoded. For example, the message '111'
/// > would give 3, since it could be decoded as 'aaa', 'ka', and 'ak'. You can
/// > assume that the messages are decodable. For example, '001' is not allowed.
///
/// # Solution
///
/// This solution seems like a relatively straight-forward variation on a graph
/// problem. It seemed to me that the easiest solution is along the lines of a
/// depth- or breadth-first traversal through the tree. Implementation-wise, a
/// recursive function can go through the possibilities without double-counting
/// any combinations.
///
/// In this instance, I chose a depth-first search where each successful search
/// that results in all valid combinations of characters counts as `1` valid
/// decode combination. The following is pseudo code representing the high-level
/// logic for the solution.
///
/// ```fsharp
/// let lower_limit = 1
/// let upper_limit = 26
///
/// let is_valid (s: string) : boolean ->
///   let n = string_to_int(s)
///   s.starts_with() != 0
///     AND n >= lower_limit
///     AND n <= upper_limit
///
/// let rec calc (s: string) : int ->
///   if length(s) == 0 then 1
///   else
///     let one_letter =
///       if is_valid(s.[0]) then calc(s.[1..])
///       else 0
///     let two_letters =
///       if is_valid(s.[0..1]) then calc(s.[2..])
///       else 0
///     one_letter + two_letters
/// ```
///
/// ## Variation 1
///
/// One optimization on the above algorithm is to memoize the number of results
/// produced by certain substrings. That way, the results do not have to
/// be re-calculated for every permutation in the earlier part of the string.
/// Memoization trades storage space for processing time and, with a large
/// enough input string, the processing savings can be enormous.
///
/// The following is pseudo code representing the high-level logic for a
/// memoization solution.
///
/// ```fsharp
/// let lower_limit = 1
/// let upper_limit = 26
/// let memo = Hashmap::new()
///
/// let is_valid (s: string) : boolean ->
///   let n = string_to_int(s)
///   s.starts_with() != 0
///     AND n >= lower_limit
///     AND n <= upper_limit
///
/// let rec calc_memo (s: string) : int ->
///   if memo.has_key(s) then memo.get_value(s)
///   elseif length(s) == 0 then 1
///   else
///     let one_letter =
///       if is_valid(s.[0]) then calc_memo(s.[1..])
///       else 0
///     let two_letters =
///       if is_valid(s.[0..1]) then calc_memo(s.[2..])
///       else 0
///     memo.add(s, one_letter + two_letters)
///     one_letter + two_letters
/// ```
mod problem_7 {
  use std::collections::HashMap;
  use std::str;

  /// The lower limit for valid integers.
  const LOWER_LIMIT: i32 = 1;

  /// The upper limit for valid integers.
  const UPPER_LIMIT: i32 = 26;

  /// Invalid character to use when converting string to int.
  const INVALID_INT: i32 = -1;

  /// Check the validity of a string against the LOWER_LIMIT and UPPER_LIMIT.
  fn is_valid(s: &[u8]) -> bool {
    // An empty string and a string starting with '0' are invalid.
    if s.len() > 0 && s[0] != b'0' {
      let i = str::from_utf8(s)
        .unwrap()
        .parse::<i32>()
        .unwrap_or(INVALID_INT);
      i >= LOWER_LIMIT && i <= UPPER_LIMIT
    } else {
      false
    }
  }

  /// Non-memoized solution.
  fn p7(input_str: &str) -> i32 {
    fn calc(s: &[u8]) -> i32 {
      if s.len() == 0 {
        // Base case - when there is nothing more to analyze, it means the
        // entire input string was valid
        1
      } else {
        // Is the next integer a valid value?
        let one_letter = if is_valid(&s[0..1]) { calc(&s[1..]) } else { 0 };

        // Is the combination of next 2 integers a valid value?
        let two_letters = if s.len() > 1 && is_valid(&s[0..2]) {
          calc(&s[2..])
        } else {
          0
        };

        // Return the number of valid combinations
        one_letter + two_letters
      }
    }

    calc(input_str.as_bytes())
  }

  fn p7_memoize(input_str: &str) -> i32 {
    // `HashMap` that holds memoized values.
    let mut memo_map: HashMap<&[u8], i32> = HashMap::new();

    fn calc(s: &[u8]) -> i32 {
      if memo_map.
      else if s.len() == 0 {
        // Base case - when there is nothing more to analyze, it means the
        // entire input string was valid
        1
      } else {
        // Is the next integer a valid value?
        let one_letter = if is_valid(&s[0..1]) { calc(&s[1..]) } else { 0 };

        // Is the combination of next 2 integers a valid value?
        let two_letters = if s.len() > 1 && is_valid(&s[0..2]) {
          calc(&s[2..])
        } else {
          0
        };

        // Return the number of valid combinations
        one_letter + two_letters
      }
    }

    calc(input_str.as_bytes())
  }

  /// Tests module
  #[cfg(test)]
  mod test_is_valid {
    /// Pull in all functions from the parent module, including private
    /// variables and functions.
    use super::*;

    /// Import for property-based testing.
    use proptest::prelude::*;

    /// Tests for the constants.
    #[test]
    fn test_constants() {
      assert_eq!(LOWER_LIMIT, 1);
      assert_eq!(UPPER_LIMIT, 26);
      assert_eq!(INVALID_INT, -1);
    }

    /// Test `is_valid()` for crashes, using property-based testing.
    #[test]
    fn test_is_valid_pb_crash() {
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
    pub fn test_is_valid_pb_i32() {
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

  #[cfg(test)]
  mod test_p7 {
    /// Pull in all functions from the parent module, including private
    /// variables and functions.
    use super::*;

    /// Import for property-based testing.
    use proptest::prelude::*;

    /// Tests for the `p7` function.
    #[test]
    fn test_p7_pb_crash() {
      // Test for crashes.
      proptest!(|(x in "[0-9]*")| {
        p7(&x);
      })
    }

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
  }

  /// Benchmarks module
  #[cfg(test)]
  mod benchmarks {
    use super::test_is_valid::*;
    use test::Bencher;

    #[bench]
    fn bench_is_valid_pb_i32(b: &mut Bencher) {
      b.iter(|| test_is_valid_pb_i32())
    }
  }
}

fn main() {
  println!("Hello, world!");

  let s = &"Golden Eagle";
  let slice = &s[..s.char_indices().nth(6).unwrap().0];
  println!("{}", slice);
}
