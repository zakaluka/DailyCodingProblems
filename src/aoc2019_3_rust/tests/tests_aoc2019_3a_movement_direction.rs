#![feature(test)]

use aoc2019_3::problem_3a::MovementDirection;
use aoc2019_3::problem_3a::MovementDirection::{D, L, R, U};
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

#[test]
fn movement_direction_simple_up() {
  assert_eq!("U".parse::<MovementDirection>().unwrap(), U, "Up");
  assert_eq!("u".parse::<MovementDirection>().unwrap(), U, "Up2");
  assert_eq!(format!("{}", U), "U", "Up3");
}

#[test]
fn movement_direction_simple_down() {
  assert_eq!("D".parse::<MovementDirection>().unwrap(), D, "Down");
  assert_eq!("d".parse::<MovementDirection>().unwrap(), D, "Down2");
  assert_eq!(format!("{}", D), "D", "Down3");
}

#[test]
fn movement_direction_simple_left() {
  assert_eq!("L".parse::<MovementDirection>().unwrap(), L, "Left");
  assert_eq!("l".parse::<MovementDirection>().unwrap(), L, "Left2");
  assert_eq!(format!("{}", L), "L", "Left3");
}

#[test]
fn movement_direction_simple_right() {
  assert_eq!("R".parse::<MovementDirection>().unwrap(), R, "Right");
  assert_eq!("r".parse::<MovementDirection>().unwrap(), R, "Right2");
  assert_eq!(format!("{}", R), "R", "Right3");
}

#[test]
#[should_panic(expected = "Not one of U, D, L, R: X")]
fn movement_direction_simple_x() { "X".parse::<MovementDirection>().unwrap(); }

#[test]
#[should_panic(expected = "Value is not a single character: ")]
fn movement_direction_simple_empty_string() {
  "".parse::<MovementDirection>().unwrap();
}

#[test]
#[should_panic(expected = "Value is not a single character: uldr")]
fn movement_direction_simple_long_string() {
  "uldr".parse::<MovementDirection>().unwrap();
}

proptest! {
 #[test]
 fn movement_direction_pb_strategy_is_valid
  (md in strategy_movement_direction()) {
   prop_assert!(md == U || md == D || md == L || md == R);
 }

 #[test]
 fn movement_direction_pb_valid_input(s in "[uUdDlLrR]") {
   prop_assert!(s.parse::<MovementDirection>().is_ok());
 }

 #[test]
 fn movement_direction_pb_invalid_input_single_char(s in "[^uUdDlLrR]") {
   prop_assert!(s.parse::<MovementDirection>().is_err());
 }

 #[test]
 fn movement_direction_pb_invalid_input_multichar(s in "..+") {
   prop_assert!(s.parse::<MovementDirection>().is_err());
 }
}
