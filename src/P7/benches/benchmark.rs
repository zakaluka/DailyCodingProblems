#[macro_use]
extern crate criterion;

// Import the libraries for performance testing.
use criterion::Criterion;
use criterion::{black_box, BenchmarkId};

// Import the library code for testing.
use p7::problem_7::*;

fn bench_is_valid(c: &mut Criterion) {
  c.bench_function("is_valid", |b| {
    b.iter(|| {
      for i in 0..100 {
        is_valid(black_box(format!("{}", i).as_bytes()));
      }
    })
  });
}

fn bench_p7_all(c: &mut Criterion) {
  let inputs = [
    "1".repeat(20),
    "2".repeat(20),
    "12".repeat(10),
    "123".repeat(10),
    // non-repeating value 1, 18
    "124782193651078432562974".to_string(),
    // non-repeating value 2, 245760
    "12131415161718191010918171615145141313121".to_string(),
  ];

  let mut group = c.benchmark_group("Problem 7");

  for elt in inputs.iter() {
    // Naive function.
    group.bench_with_input(BenchmarkId::new("p7", elt), elt, |b, i| {
      b.iter(|| {
        p7(black_box(i));
      })
    });

    // Memoized function.
    group.bench_with_input(BenchmarkId::new("p7_memoize", elt), elt, |b, i| {
      b.iter(|| {
        p7_memoized(black_box(i));
      })
    });

    // Tail-recursive-ish function.
    group.bench_with_input(
      BenchmarkId::new("p7_tail_recursive_ish", elt),
      elt,
      |b, i| {
        b.iter(|| {
          p7_tail_recursive_ish(black_box(i));
        })
      },
    );

    // Tail-recursive function.
    group.bench_with_input(
      BenchmarkId::new("p7_tail_recursive", elt),
      elt,
      |b, i| {
        b.iter(|| {
          p7_tail_recursive(black_box(i));
        })
      },
    );
  }
}

criterion_group!(benches, bench_is_valid, bench_p7_all);
criterion_main!(benches);
