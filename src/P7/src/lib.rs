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

  /// Initial solution, not tail recursive and not memoized.
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

  /// Tail-recursive-ish solution, not memoized. I don't believe this is tail
  /// recursive because I am not "passing the stack" manually through a function
  /// argument. Instead, since almost all nodes have two children (`one_letter`
  /// and `two_letter`), I am relying on the system to "spring back" to the node
  /// in the call stack to go down the other avenue. Really, it is only
  /// tail-recursive in the sense that once we go down the rightmost branch
  /// of a node, we can return the answer for that node without having to go
  /// back up the call stack.
  pub fn p7_tail_recursive_ish(input_str: &str) -> i32 {
    fn calc(s: &[u8], ans: i32) -> i32 {
      if s.len() == 0 {
        // Base case - when there is nothing more to analyze, it means the
        // entire input string was valid
        1 + ans
      } else {
        // Is the next integer a valid value? If yes, check the rest of that DFS
        // branch.  Otherwise, stop checking that avenue.
        let one_letter = if is_valid(&s[0..1]) {
          calc(&s[1..], ans)
        } else {
          ans
        };

        // Check if the combination of next 2 integers is a valid value.  If it
        // is, check the rest of that DFS branch. Otherwise, stop checking that
        // avenue. We pass `one_letter` to `calc()` because it already
        // incorporates `ans` from its calculation.
        let two_letter = if s.len() > 1 && is_valid(&s[0..2]) {
          calc(&s[2..], one_letter)
        } else {
          one_letter
        };

        two_letter
      }
    }

    calc(input_str.as_bytes(), 0)
  }

  /// Properly tail-recursive version of `p7_tail_recursive_ish`. The
  /// implementation has to maintain two values:
  ///
  /// * the stack of remaining investigations
  /// * the answer.
  ///
  /// That way, we don't have to rely on the program's call stack to investigate
  /// all branches through the string. Because we are using a `Vec<T>` to hold
  /// the stack, the algorithm is still a DFS (depth-first search) because the
  /// `push()` and `pop()` functions operate on the last element of the `Vec`.
  pub fn p7_tail_recursive(input_str: &str) -> i32 {
    fn calc(stack: &mut Vec<&[u8]>, ans: i32) -> i32 {
      if stack.len() == 0 {
        // We are out of branches to investigate.
        ans
      } else {
        // Get the last element (inappropriately called `hd` or head) to
        // investigate.
        let hd = stack.pop().unwrap();

        // Head has no remaining elements, so it was a valid string.
        if hd.len() == 0 {
          calc(stack, ans + 1)
        } else {
          // Head has items in the array, so they need to be investigated. We
          // only investigate branches where the next 1-2 characters represent a
          // valid integer.

          // If the next two characters are a valid integer, push the rest of
          // that branch onto the stack.
          if hd.len() > 1 && is_valid(&hd[0..2]) {
            stack.push(&hd[2..])
          }

          // If the next character is a valid integer, push the rest of that
          // branch onto the stack.
          if is_valid(&hd[0..1]) {
            stack.push(&hd[1..])
          }

          // Investigate the newly updated stack.
          calc(stack, ans)
        }
      }
    }

    let stk: &mut Vec<&[u8]> = &mut Vec::new();
    stk.push(input_str.as_bytes());
    calc(stk, 0)
  }

  /// Memoized solution, not tail-recursive.
  pub fn p7_memoized(input_str: &str) -> i32 {
    fn calc(s: &[u8], m: &mut HashMap<String, i32>) -> i32 {
      let s_to_string = str::from_utf8(s).unwrap().to_string();
      if m.contains_key(&s_to_string) {
        *m.get(&s_to_string).unwrap()
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

        m.insert(s_to_string, one_letter + two_letters);
        // Return the number of valid combinations
        one_letter + two_letters
      }
    }

    // `HashMap` that holds memoized values.
    let mut memo_map: HashMap<String, i32> = HashMap::new();

    calc(input_str.as_bytes(), &mut memo_map)
  }
}
