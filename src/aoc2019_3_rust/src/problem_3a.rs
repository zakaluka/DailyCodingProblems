use std::fmt::{Display, Formatter};
use std::str::FromStr;

use crate::problem_3a::MovementDirection::{D, L, R, U};
use crate::problem_3a::Orientation::{Horizontal, Vertical};

/// Stand-alone function to split a string into two: the first character (as a
/// string) and the remainder. Avoids errors caused due to empty strings.
fn car_cdr(s: &str) -> (&str, &str) {
  match s.chars().next() {
    Some(c) => s.split_at(c.len_utf8()),
    None => s.split_at(0),
  }
}

// =============================================================================
// Section for `Path`
// =============================================================================

/// The direction to move in to create the next `WireSection`
#[derive(Clone, Debug, PartialEq)]
pub enum MovementDirection {
  U,
  D,
  L,
  R,
}

impl Display for MovementDirection {
  fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
    let s = match self {
      U => "U",
      D => "D",
      L => "L",
      R => "R",
    };
    write!(f, "{}", s)
  }
}

impl FromStr for MovementDirection {
  type Err = String;

  fn from_str(s: &str) -> Result<Self, Self::Err> {
    if s.is_empty() || s.len() > 1 {
      Err(format!("Value is not a single character: {}", s))
    } else {
      match s {
        "u" | "U" => Ok(U),
        "d" | "D" => Ok(D),
        "l" | "L" => Ok(L),
        "r" | "R" => Ok(R),
        _ => Err(format!("Not one of U, D, L, R: {}", s)),
      }
    }
  }
}

/// Represents one entry in the `Path`
#[derive(Clone, Debug, PartialEq)]
pub struct PathEntry {
  direction: MovementDirection,
  distance: i32,
}

impl PathEntry {
  #[must_use]
  pub const fn new(direction: MovementDirection, distance: i32) -> Self {
    Self { direction, distance }
  }

  #[must_use]
  pub fn get_direction(&self) -> MovementDirection { self.direction.clone() }

  #[must_use]
  pub const fn get_distance(&self) -> i32 { self.distance }
}

impl FromStr for PathEntry {
  type Err = String;

  fn from_str(s: &str) -> Result<Self, Self::Err> {
    if s.len() < 2 {
      Err(format!("String not of type '[UDLR][0-9]+': {}", s))
    } else {
      let (md_str, dist_str) = car_cdr(s);
      let direction = md_str.parse::<MovementDirection>();
      let distance = dist_str.parse::<i32>();

      match (direction, distance) {
        (Err(_), _) => Err(format!("Unable to parse direction from {}", s)),
        (_, Err(_)) => Err(format!("Unable to parse distance from {}", s)),
        (Ok(dir), Ok(dist)) =>
          if dist > 0 {
            Ok(Self::new(dir, dist))
          } else {
            Err(format!("Zero distance not allowed for PathEntry: {}", s))
          },
      }
    }
  }
}

/// Path of a wire, given in a relative style.
#[derive(Debug, PartialEq)]
pub struct Path {
  pub moves: Vec<PathEntry>,
}

impl Path {
  #[must_use]
  pub fn new_internal(moves: Vec<PathEntry>) -> Self { Self { moves } }

  #[must_use]
  pub fn new(s: &str) -> Self { s.parse::<Self>().unwrap() }
}

impl Display for Path {
  fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
    // NOTE: Requires 2 lines instead of chaining the entire express because
    // of a lifetime issue E0716
    let s = self.moves.iter().fold(String::new(), |acc, e| {
      format!("{},{}{}", acc, e.direction, e.distance)
    });

    // Splits the string and gets the 2nd half. Per the API, this will not
    // error out even if the string being split is empty.
    let s = car_cdr(s.as_str()).1;

    write!(f, "{}", s)
  }
}

impl FromStr for Path {
  type Err = String;

  fn from_str(s: &str) -> Result<Self, Self::Err> {
    let moves: Vec<Result<PathEntry, _>> = s
      .chars()
      .filter(|c: &char| !c.is_whitespace())
      .collect::<String>()
      .split(',')
      .filter_map(|e: &str| {
        if e.is_empty() { None } else { Some(e.parse::<PathEntry>()) }
      })
      .collect();

    let any_errors = moves.iter().any(std::result::Result::is_err);
    if any_errors {
      Err(format!(
        "Unable to create Path from {}, {:?}",
        s,
        moves.iter().find(|e| e.is_err())
      ))
    } else {
      let clean_moves: Vec<PathEntry> =
        moves.iter().map(|e| e.as_ref().unwrap()).cloned().collect();

      Ok(Self::new_internal(clean_moves))
    }
  }
}

// =============================================================================
// Section for `Wire`
// =============================================================================

/// The orientation of a section of wire
#[derive(Clone, Debug, PartialEq)]
pub enum Orientation {
  Vertical,
  Horizontal,
}

/// A point in a 2D plane
#[derive(Debug, Eq, PartialEq)]
pub struct Point {
  x: i32,
  y: i32,
}

impl Point {
  pub const ORIGIN: Point = Point { x: std::i32::MIN, y: std::i32::MIN };

  /// Calculates the Manhattan distance of a point to the center
  #[must_use]
  pub const fn manhattan_distance(&self) -> i64 {
    ((self.x as i64) - (Point::ORIGIN.x as i64)).abs()
      + ((self.y as i64) - (Point::ORIGIN.y as i64)).abs()
  }

  #[must_use]
  /// TODO understand if this lifetime parameter is correct.
  pub fn closer_to_zero<'a>(&'a self, other_point: &'a Point) -> &'a Point {
    let md_self = self.manhattan_distance();
    let md_other = other_point.manhattan_distance();

    if md_self <= md_other { self } else { other_point }
  }
}

#[derive(Debug, PartialEq)]
pub struct WireSection {
  orientation: Orientation,
  start: Point,
  end: Point,
}

impl WireSection {
  /// Create a `WireSection` from two `Point`s.
  ///
  /// Test for post-invariant:
  /// - start point < end point
  #[must_use]
  pub fn create(point1: Point, point2: Point) -> Self {
    let md1 = point1.manhattan_distance();
    let md2 = point2.manhattan_distance();
    if point1 == point2 {
      panic!("A WireSection cannot be created with 2 identical Points")
    } else if point1.x != point2.x && point1.y != point2.y {
      panic!("A WireSection must be horizontal or vertical, not diagonal")
    }

    // One point is closer than the other to the origin or, if equidistant, then
    // one has "lower" `x` or `y` values than the other point
    let (sp, ep) = if md1 < md2 {
      (point1, point2)
    } else if md1 > md2 {
      (point2, point1)
    } else if point1.x < point2.x {
      // Horizontal line
      (point1, point2)
    } else {
      // Vertical line, point1.y < point2.y
      (point2, point1)
    };

    let direction = if sp.x == ep.x {
      Orientation::Vertical
    } else {
      Orientation::Horizontal
    };

    Self { orientation: direction, start: sp, end: ep }
  }

  /// Calculate the intersection of two `Point`s. As context, my mental plane
  /// under considerations has `(0,0)` at the top-left corner, `x` increases
  /// to the right, and `y` increases as you go down. There are multiple
  /// scenarios to consider:
  /// - Setup for all scenarios:
  ///   - Line 1 is `x1, y1` to `x2, y2`
  ///   - Line 2 is `x3, y3` to `x4, y4`
  ///   - General invariants:
  ///     - Line orientation - both lines are oriented so that the start points
  ///       are less than or equal to the end points.
  ///       - `x1 <= x2 && y1 <= y2`
  ///         - or `manhattan_distance(x1,y1) < manhattan_distance(x2,y2)`
  ///         - in case of a tie, start point should be the lower `x` or `y`
  ///           value.
  ///       - `x3 <= x4 && y3 <= y4`
  ///         - or `manhattan_distance(x3,y3) < manhattan_distance(x4,y4)`
  ///     - Distance to zero - Line 1 is closer to the origin than line 2.
  ///       - `manhattan_distance(x1,y1) <= manhattan_distance(x3,y3)`
  /// - 2 `Horizontal` lines
  ///   - Invariants:
  ///     - `y1 == y2 && y3 == y4` (on horizontal lines, the `y` never changes)
  ///     - `x1 <= x3`
  ///       - if not, swap them so Line 1 is to the left of Line 2 and/or they
  ///         start at the same point.
  ///   - Scenarios:
  ///     - The lines have no overlap at all.
  ///       - `y1 != y3` (i.e. they are vertically offset from each other).
  ///       - `x2 < x3` (i.e. Line 1 ends before Line 2 starts).
  ///     - One line is completely within another one.
  ///       - `x1 <= x3 && x2 >= x4` (Line 2 is shorter or they are the same
  ///         length)
  ///       - `x1 == x3 && x2 < x4` (Line 1 is shorter)
  ///         - In both scenarios, the closest intersection point is at: `(x3,
  ///           y1)`.  If Line 2 is shorter, the other intersection point is
  ///           `(x4, y1)`.  If Line 1 is shorter, the other intersection point
  ///           is `(x2, y1)`.  However, both of these are guaranteed to be
  ///           farther away from the origin than `(x3, y1)` if the
  ///           pre-conditions / invariants are true.
  ///     - There is a partial overlap of the two lines
  ///       - `x1 <= x3 && x2 < x4`
  ///       - `x1 < x3 && x2 <= x4`
  ///         - Intersect at: `(x3, y1)`.  All other intersection points, such
  ///           as `(x2, y1)`, are guaranteed to be farther away from the origin
  ///           point.
  /// - 2 Vertical lines.
  ///   - Similar to 2 Horizontal lines.
  ///   - Invariants:
  ///     - `x1 == x2 && x3 == x4` (on vertical lines, the `x` doesn't change
  ///       for each line)
  ///     - `y1 <= y3`
  ///       - if not, swap them so Line 1 is above Line 2 and/or they start at
  ///         the same point.
  ///   - Scenarios:
  ///     - The lines have no overlap at all.
  ///       - `x1 != x3` (i.e. they are horizontally offset from each other).
  ///       - `y2 < y3` (i.e. Line 1 ends before Line 2 starts)
  ///     - One line is completely within another one.
  ///       - `y1 <= y3 && y2>= y4` (Line 2 is shorter or they are the same
  ///         length)
  ///       - `y1 == y3 && y2 < y4` (Line 1 is shorter)
  ///         - Both scenarios intersect at: `(x1, y3)`. Other intersection
  ///           points, such as `(x1, y4)` or `(x1, y2)` are guaranteed to be
  ///           farther away from the origin if the pre-conditions / invariants
  ///           are true.
  ///     - Partial overlap of the lines:
  ///       - `y1 <= y3 && y2 < y4`
  ///       - `y1 < y3 && y2 <= y4`
  ///       - Intersect at: `min()`
  /// - 1 Horizontal and 1 Vertical line
  ///   - Line 1 is the horizontal line.
  ///   - Line 2 is the vertical line.
  ///   - Invariants:
  ///     - `y1 == y2` (Line 1 is horizontal)
  ///     - `x3 == x4` (Line 2 is vertical)
  ///       - If these invariants are not true, swap the lines.
  ///   - Scenarios:
  ///     - There is no overlap.
  ///       - Vertical offset.
  ///         - `y1 < y3` (Line 1 is above Line 2)
  ///         - `y1 > y4` (Line 1 is below Line 2)
  ///       - Horizontal offset.
  ///         - `x2 < x3` (Line 1 is to left of Line 2)
  ///         - `x1 > x4` (Line 1 is to the right of Line 2)
  ///     - There is one point of overlap.
  ///       - Intersect at: `x3, y1`
  ///
  /// *NOTE:* Don't compare `x` and `y` values directly because if the numbers
  /// are negative, then the operations are reversed.  We want distance to
  /// origin `(0,0)` to determine which point is "smaller", not a pure
  /// mathematical comparison.  E.g. mathematically, `-4 < -2`.  However, in
  /// our problem, `-2` is considered the smaller value because it is closer
  /// to `0`.
  pub fn intersection(&self, other_point: &Point) -> Result<Point, &str> {
    let (line1, line2) = (1, 2);
    unimplemented!("intersection")
  }
}

#[derive(Debug)]
pub struct Wire {
  sections: Vec<WireSection>,
}
