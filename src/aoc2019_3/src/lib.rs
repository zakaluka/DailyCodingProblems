#![deny(clippy::all, clippy::nursery, clippy::pedantic)]
#![forbid(unsafe_code)]
#![feature(try_find)]

/// Strategy:
///
/// - Input string is of the form `D15,U25,L30,R40`.
///   - This is represented in its entirety by a `Path`.
///     - Each entry, separated by commas, is a `PathEntry`, which consists of 2
///       items:
///       - `MovementDirection` that represents up, down, left, or right.
///       - Distance represented by an `i32`.
/// - A `Path` can be turned into a `Wire`.
/// - A `Wire` is a series of segments that represents the main item in the
///   problem.
///     - A `Wire` is composed of one or more `WireSection`s.  Each
///       `WireSection` is composed of 3 parts:
///         - An `Orientation`, which is either vertical or horizontal.
///         - A start `Point` (x and y `i32` coordinates).
///         - An end `Point` (x and y `i32` coordinates).
pub mod problem_3a;
