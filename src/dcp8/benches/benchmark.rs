#[macro_use]
extern crate criterion;

// Import the libraries for benchmarking.
use criterion::{Benchmark, Criterion};
use criterion::{BenchmarkId, Throughput};

// Import the library code for benchmarking.
use dcp8::problem_8::tree::Tree::{Branch, Leaf};
use dcp8::problem_8::tree::{count_unival_trees, Tree};

use itertools::Itertools;
use rand::prelude::*;
use std::collections::HashMap;

/// Simple function to create a tree with a random boolean in each node.
fn create_tree(levels: u64) -> Box<Tree> {
  if levels == 0 {
    Box::new(Leaf(rand::random()))
  } else {
    Box::new(Branch(
      rand::random(),
      create_tree(levels - 1),
      create_tree(levels - 1),
    ))
  }
}

fn bench_count_unival_trees(c: &mut Criterion) {
  // Create inputs as powers of 2.
  let base: u64 = 2;

  let inputs = [
    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
  ]
  .iter()
  .map(|s| (base.checked_pow(*s).unwrap(), create_tree(*s as u64)))
  .collect::<HashMap<u64, Box<Tree>>>();

  //  let sizes = [1, 2, 4, 8, 16, 32];
  //  let inputs =

  // Group for plotting results.
  let mut group = c.benchmark_group("Count Unival Trees");

  for size in inputs.keys().sorted() {
    // Report throughput correctly
    group.throughput(Throughput::Elements(*size));

    // Perform the benchmark. The `BenchmarkId` uses an underscore because
    // without it, during Valgrind memory profiling, the system runs all
    // benchmarks starting with the specified number (e.g. trying to run the
    // benchmark with 2 levels, runs benchmarks for levels 2, 256, 2048, and
    // 262144).  Providing an underscore as the terminating character fixes the
    // issue.
    group.bench_with_input(
      BenchmarkId::new("CUT", format!("{}_", *size)),
      size,
      |b, &size| {
        b.iter(|| count_unival_trees(inputs.get(&size).unwrap().clone()))
      },
    );
  }

  // Complete the entries for this benchmark group.
  group.finish();
}

criterion_group!(benches, bench_count_unival_trees);
criterion_main!(benches);
