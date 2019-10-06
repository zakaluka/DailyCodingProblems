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
pub mod problem_7 {
  use std::collections::HashMap;
  use std::str;

  /// The lower limit for valid integers.
  pub const LOWER_LIMIT: i32 = 1;

  /// The upper limit for valid integers.
  pub const UPPER_LIMIT: i32 = 26;

  /// Invalid character to use when converting string to int.
  pub const INVALID_INT: i32 = -1;

  /// Check the validity of a string against the LOWER_LIMIT and UPPER_LIMIT.
  pub fn is_valid(s: &[u8]) -> bool {
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
  pub fn p7(input_str: &str) -> i32 {
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

  pub fn p7_memoize(input_str: &str) -> i32 {
    fn calc(s: &[u8], m: &mut HashMap<&[u8], i32>) -> i32 {
      if m.contains_key(s) {
        *m.get(s).unwrap()
      } else if s.len() == 0 {
        // Base case - when there is nothing more to analyze, it means the
        // entire input string was valid
        1
      } else {
        // Is the next integer a valid value?
        let one_letter = if is_valid(&s[0..1]) {
          calc(&s[1..], m)
        } else {
          0
        };

        // Is the combination of next 2 integers a valid value?
        let two_letters = if s.len() > 1 && is_valid(&s[0..2]) {
          calc(&s[2..], m)
        } else {
          0
        };

        // Return the number of valid combinations
        one_letter + two_letters
      }
    }

    // `HashMap` that holds memoized values.
    let mut memo_map: HashMap<&[u8], i32> = HashMap::new();

    calc(input_str.as_bytes(), &mut memo_map)
  }
}
