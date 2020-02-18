use gloo_timers::future::TimeoutFuture;
use seed::{prelude::*, *};
use wasm_bindgen::__rt::std::io::Error;

#[derive(Default)]
struct LogString {
  log: String,
}

impl std::io::Write for LogString {
  fn write(&mut self, buf: &[u8]) -> Result<usize, Error> {
    log += str::from_utf8(buf).to_owned();
    Ok(buf.len())
  }

  fn flush(&mut self) -> Result<(), Error> { Ok(()) }
}

#[derive(Default)]
struct UiModel {
  input: String,
  log: LogString,
  process_button_disabled: bool,
  visual_delay_ms: u32,
}

/// Program model.
#[derive(Default)]
struct Model {
  position: usize,
  int_code: Vec<i64>,
  ui_model: UiModel,
}

impl Model {
  /// Get a value from the model at the given position.
  fn code_lookup(&self, position: usize) -> i64 { self.int_code[position] }

  /// Set a value on the model.
  fn set(&mut self, position: usize, value: i64) {
    self.int_code[position] = value
  }

  fn log(&mut self, s: &str) { self.ui_model.operations_history += s; }
}
