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

/// Generate `Orientation` values
fn strategy_orientation() -> impl Strategy<Value = Orientation> {
  prop_oneof![Just(Orientation::Horizontal), Just(Orientation::Vertical),]
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
