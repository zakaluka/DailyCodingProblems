#![feature(test)]

use aoc2019_3::problem_3a::MovementDirection::{D, L, R, U};
use aoc2019_3::problem_3a::*;
use proptest::prelude::*;

/// Generate `MovementDirection` values
fn strategy_movement_direction() -> impl Strategy<Value = MovementDirection> {
  prop_oneof![
    Just(MovementDirection::U),
    Just(MovementDirection::D),
    Just(MovementDirection::L),
    Just(MovementDirection::R),
  ]
}

prop_compose! {
  /// Generate `PathEntry` values
  fn arb_pathentry()
                  (direction in strategy_movement_direction(),
                   distance in 1..10_000)
                  -> PathEntry {
    PathEntry::new(direction, distance)
  }
}

prop_compose! {
  /// Generate `Path` values
  fn arb_path(max_segments: usize)
             (vec in prop::collection::vec(arb_pathentry(), 1..max_segments))
             -> Path {
    Path::new_internal(vec)
  }
}

/// R8,U5,L5,D3
#[test]
fn path_simple_problem1() {
  let p_expected = Path::new_internal(vec![
    PathEntry::new(R, 8),
    PathEntry::new(U, 5),
    PathEntry::new(L, 5),
    PathEntry::new(D, 3),
  ]);
  assert_eq!(Path::new("R8,U5,L5,D3"), p_expected);
}

/// R75,D30,R83,U83,L12,D49,R71,U7,L72
#[test]
fn path_simple_problem2() {
  let p_expected = Path::new_internal(vec![
    PathEntry::new(R, 75),
    PathEntry::new(D, 30),
    PathEntry::new(R, 83),
    PathEntry::new(U, 83),
    PathEntry::new(L, 12),
    PathEntry::new(D, 49),
    PathEntry::new(R, 71),
    PathEntry::new(U, 7),
    PathEntry::new(L, 72),
  ]);
  assert_eq!(Path::new("R75,D30,R83,U83,L12,D49,R71,U7,L72"), p_expected);
}

/// U62,R66,U55,R34,D71,R55,D58,R83
#[test]
fn path_simple_problem3() {
  let p_expected = Path::new_internal(vec![
    PathEntry::new(U, 62),
    PathEntry::new(R, 66),
    PathEntry::new(U, 55),
    PathEntry::new(R, 34),
    PathEntry::new(D, 71),
    PathEntry::new(R, 55),
    PathEntry::new(D, 58),
    PathEntry::new(R, 83),
  ]);
  assert_eq!(Path::new("U62,R66,U55,R34,D71,R55,D58,R83"), p_expected);
}

/// R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51
#[test]
fn path_simple_problem4() {
  let p_expected = Path::new_internal(vec![
    PathEntry::new(R, 98),
    PathEntry::new(U, 47),
    PathEntry::new(R, 26),
    PathEntry::new(D, 63),
    PathEntry::new(R, 33),
    PathEntry::new(U, 87),
    PathEntry::new(L, 62),
    PathEntry::new(D, 20),
    PathEntry::new(R, 33),
    PathEntry::new(U, 53),
    PathEntry::new(R, 51),
  ]);
  assert_eq!(
    Path::new("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51"),
    p_expected
  );
}

/// U98,R91,D20,R16,D67,R40,U7,R15,U6,R7
#[test]
fn path_simple_problem5() {
  let p_expected = Path::new_internal(vec![
    PathEntry::new(U, 98),
    PathEntry::new(R, 91),
    PathEntry::new(D, 20),
    PathEntry::new(R, 16),
    PathEntry::new(D, 67),
    PathEntry::new(R, 40),
    PathEntry::new(U, 7),
    PathEntry::new(R, 15),
    PathEntry::new(U, 6),
    PathEntry::new(R, 7),
  ]);
  assert_eq!(Path::new("U98,R91,D20,R16,D67,R40,U7,R15,U6,R7"), p_expected);
}

#[test]
fn path_simple_empty_string() {
  let p_expected = Path::new_internal(Vec::new());
  let p_actual = Path::new("");
  assert_eq!(p_actual, p_expected);
  assert_eq!(p_expected.moves.len(), 0);
  assert_eq!(p_actual.moves.len(), 0);
}

proptest! {
  #[test]
  fn path_pb_valid_input(s in "([ULDRuldr][1-9][0-9]{0,4},)*") {
    prop_assert!(s.parse::<Path>().is_ok());
  }

  #[test]
  fn path_pb_invalid_direction(s in "([^ULDRuldr][0-9]{1,5},)+") {
    prop_assert!(s.parse::<Path>().is_err());
  }

  #[test]
  fn path_pb_invalid_distance(s in "([ULDRuldr]-[0-9]{1,5},)+") {
    prop_assert!(s.parse::<Path>().is_err());
  }
}
