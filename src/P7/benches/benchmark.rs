#[macro_use]
extern crate criterion;

use criterion::black_box;
use criterion::Criterion;

fn fibonacci(n: u64) -> u64 {
  match n {
    0 => 1,
    1 => 1,
    n => fibonacci(n - 1) + fibonacci(n - 2),
  }
}

fn criterion_benchmark(c: &mut Criterion) {
  c.bench_function("fib 20", |b| b.iter(|| fibonacci(black_box(20))));
}

criterion_group!(benches, criterion_benchmark);
criterion_main!(benches);

/*
#![feature(test)]

extern crate test;

/// Benchmarks module
#[cfg(test)]
mod benchmarks {
  use super::*;
  use test::Bencher;

  const NUMBER_OF_ITERATIONS: i32 = 100000;

  #[bench]
  fn bench_is_valid_pb_i32(b: &mut Bencher) {
    b.bench(|_| {
      let n = test::black_box(NUMBER_OF_ITERATIONS);
      (0..n).map(|_| test_is_valid::test_is_valid_pb_i32());
    });
  }

  #[bench]
  fn bench_p7_pb_valid_values(b: &mut Bencher) {
    b.iter(|| {
      let n = test::black_box(NUMBER_OF_ITERATIONS);
      (0..n).map(|_| test_p7::test_p7_pb_valid_values())
    })
  }

  #[bench]
  fn bench_p7m_pb_valid_values(b: &mut Bencher) {
    b.iter(|| {
      let n = test::black_box(NUMBER_OF_ITERATIONS);
      (0..n).map(|_| test_p7_memoize::test_p7m_pb_valid_values())
    })
  }
}

*/
