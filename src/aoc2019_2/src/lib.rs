/// Program model.
#[derive(Default)]
pub struct Model {
  pub int_code: Vec<i64>,
}

impl Model {
  /// Get a value from the model at the given position.
  pub fn get(&self, position: usize) -> i64 { self.int_code[position] }

  /// Set a value on the model.
  pub fn set(&mut self, position: usize, value: i64) {
    self.int_code[position] = value
  }
}

impl From<String> for Model {
  /// Converts a String to a Model by deferring to another implementation.
  fn from(s: String) -> Self { s.as_str().into() }
}

impl From<&str> for Model {
  /// Converts a &str to a Model using the following steps:
  /// - Remove all non-digits and non-commas from the input string
  /// - Remove empty inputs (i.e. no ',,'-style scenarios allowed)
  /// - Convert the remaining inputs to `i64` values
  /// - Collect as a `Vec<i64>` for `Model.int_code`
  fn from(s: &str) -> Self {
    Model {
      int_code: s
        .chars()
        .filter(|c| c.eq(&',') || c.is_ascii_digit())
        .collect::<String>()
        .split(",")
        .filter(|s| !s.is_empty())
        .map(|s| s.parse::<i64>().unwrap())
        .collect(),
    }
  }
}

/// Executes the operation for Opcode 1.
fn opcode_one(number1: i64, number2: i64) -> i64 { number1 + number2 }

/// Executes the operation for Opcode 2.
fn opcode_two(number1: i64, number2: i64) -> i64 { number1 * number2 }

/// The executor for Problem 2a.
pub fn problem_2a(mut m: Model) -> Model {
  let mut position: usize = 0;

  while m.get(position) != 99 {
    let opcode = m.get(position);
    let num1 = m.get(m.get(position + 1) as usize);
    let num2 = m.get(m.get(position + 2) as usize);
    let target_slot = m.get(position + 3) as usize;

    let result = match opcode {
      1 => opcode_one(num1, num2),
      2 => opcode_two(num1, num2),
      _ => panic!(format!(
        "Bad input string, for line opcode:{} num1:{} num2:{} target_slot:{} \
         at position:{}",
        opcode, num1, num2, target_slot, position
      )),
    };

    // Save the result in the target location.
    m.set(target_slot, result);

    // Move the position forward by 4 slots.
    position += 4;
  }

  // Return the updated model.
  m
}
