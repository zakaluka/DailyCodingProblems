#![feature(test)]

use aoc2019_3::problem_3a::MovementDirection::{D, L, R, U};
use aoc2019_3::problem_3a::{MovementDirection, PathEntry};
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

#[test]
fn path_entry_simple_up() {
  assert_eq!("U42".parse::<PathEntry>().unwrap(), PathEntry::new(U, 42));
  assert_eq!("u42".parse::<PathEntry>().unwrap(), PathEntry::new(U, 42));
}

#[test]
fn path_entry_simple_down() {
  assert_eq!("D42".parse::<PathEntry>().unwrap(), PathEntry::new(D, 42));
  assert_eq!("d42".parse::<PathEntry>().unwrap(), PathEntry::new(D, 42));
}

#[test]
fn path_entry_simple_left() {
  assert_eq!("L42".parse::<PathEntry>().unwrap(), PathEntry::new(L, 42));
  assert_eq!("l42".parse::<PathEntry>().unwrap(), PathEntry::new(L, 42));
}

#[test]
fn path_entry_simple_right() {
  assert_eq!("R42".parse::<PathEntry>().unwrap(), PathEntry::new(R, 42));
  assert_eq!("r42".parse::<PathEntry>().unwrap(), PathEntry::new(R, 42));
}

#[test]
#[should_panic(expected = "Zero distance not allowed for PathEntry: U0")]
fn path_entry_simple_0_distance() { "U0".parse::<PathEntry>().unwrap(); }

#[test]
fn path_entry_simple_empty_string() {
  match "".parse::<PathEntry>() {
    Err(msg) => assert_eq!(msg, "String not of type '[UDLR][0-9]+': "),
    _ => panic!("Should never happen"),
  }
}

#[test]
fn path_entry_simple_invalid_direction() {
  match "X42".parse::<PathEntry>() {
    Err(msg) => assert_eq!(msg, "Unable to parse direction from X42"),
    _ => panic!("Should never happen"),
  }
}

#[test]
fn path_entry_simple_invalid_distance() {
  match "UYZ".parse::<PathEntry>() {
    Err(msg) => assert_eq!(msg, "Unable to parse distance from UYZ"),
    _ => panic!("Should never happen"),
  }
}

#[test]
fn path_entry_simple_negative_distance() {
  match "U-42".parse::<PathEntry>() {
    Err(msg) =>
      assert_eq!(msg, "Zero distance not allowed for PathEntry: U-42"),
    _ => panic!("Should never happen"),
  }
}

proptest! {
 #[test]
 fn path_entry_pb_strategy_is_valid(pe in arb_pathentry()) {
   let md = pe.get_direction();
   let dist = pe.get_distance();
   prop_assert!(md == U || md == D || md == L || md == R);
   prop_assert!(dist > 0);
 }

 #[test]
 fn path_entry_pb_valid_input(md in "[uUdDlLrR]", dist in 1..i32::MAX) {
   let s = format!("{}{}", md, dist);
   prop_assert!(s.parse::<PathEntry>().is_ok());
 }

 #[test]
 fn path_entry_pb_invalid_direction(s in "[^uUdDlLrR][1-9][0-9]*") {
   prop_assert!(s.parse::<PathEntry>().is_err());
 }

 #[test]
 fn path_entry_pb_invalid_distance(s in "[uUdDlLrR]-[0-9]+") {
   prop_assert!(s.parse::<PathEntry>().is_err());
 }

 #[test]
 fn path_entry_pb_invalid_string(s in ".*") {
   prop_assert!(s.parse::<PathEntry>().is_err());
 }
}
