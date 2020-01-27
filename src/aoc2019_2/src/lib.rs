use gloo_timers::future::TimeoutFuture;
use seed::{prelude::*, *};

#[derive(Default)]
struct UiModel {
  input: String,
  operations_history: String,
  process_button_disabled: bool,
}

#[derive(Default)]
struct Model {
  position: usize,
  int_code: Vec<i64>,
  ui_model: UiModel,
}

impl Model {
  /// Get a value from the model at the given position.
  fn lookup(&self, position: usize) -> i64 { self.int_code[position] }

  /// Set a value on the model.
  fn set(&mut self, position: usize, value: i64) {
    self.int_code[position] = value
  }

  fn log(&mut self, s: &str) { self.ui_model.operations_history += s; }
}

#[derive(Clone)]
enum Msg {
  InputChanged(String),
  LogicMoveToNextOpCode,
  ProcessInputClicked,
  LogicInitialize,
  LogicProcessCurrentOpCode,
  LogicFinalize,
}

fn opcode_one(number1: i64, number2: i64) -> i64 { number1 + number2 }

fn opcode_two(number1: i64, number2: i64) -> i64 { number1 * number2 }

async fn delay_next_op_code(millis: u32) -> Result<Msg, Msg> {
  TimeoutFuture::new(millis).await;
  Ok(Msg::LogicProcessCurrentOpCode)
}

fn update(msg: Msg, model: &mut Model, orders: &mut impl Orders<Msg>) {
  match msg {
    Msg::InputChanged(str) =>
      model.ui_model.input = str
        .chars()
        .filter(|c| c.eq(&',') || c.is_ascii_digit())
        .collect(),
    Msg::LogicMoveToNextOpCode => {
      model.position += 4;
      model.log(format!("Moved to position {}\n", model.position).as_str());
      orders.perform_cmd(delay_next_op_code(1000));
    },
    Msg::ProcessInputClicked => {
      model.ui_model.process_button_disabled = true;
      orders.send_msg(Msg::LogicInitialize);
    },
    Msg::LogicInitialize => {
      model.int_code = model
        .ui_model
        .input
        .split(',')
        .filter(|s| !s.is_empty())
        .map(|s| s.parse::<i64>().unwrap())
        .collect();
      model.position = Default::default();
      model.ui_model.operations_history = Default::default();
      model.log("Initializing Logic\n");
      orders.perform_cmd(delay_next_op_code(1_000));
    },
    Msg::LogicProcessCurrentOpCode => {
      let op_code = model.int_code[model.position];
      match op_code {
        1 => {
          let left = model.lookup(model.lookup(model.position + 1) as usize);
          let right = model.lookup(model.lookup(model.position + 2) as usize);
          let result_position = model.lookup(model.position + 3) as usize;
          let result = opcode_one(left, right);

          model.log(
            format!(
              "Placing {} (OpCode 1 [{}, {}]) into position {}\n",
              result, left, right, result_position
            )
            .as_str(),
          );

          model.set(result_position, result);
          orders.send_msg(Msg::LogicMoveToNextOpCode);
        },
        2 => {
          let left = model.lookup(model.lookup(model.position + 1) as usize);
          let right = model.lookup(model.lookup(model.position + 2) as usize);
          let result_position = model.lookup(model.position + 3) as usize;
          let result = opcode_two(left, right);

          model.log(
            format!(
              "Placing {} (OpCode 2 [{}, {}]) into position {}\n",
              result, left, right, result_position
            )
            .as_str(),
          );

          model.set(result_position, result);
          orders.send_msg(Msg::LogicMoveToNextOpCode);
        },
        99 => {
          model.log(
            format!(
              "Program has ended based on code 99 at position {}\n",
              model.position
            )
            .as_str(),
          );
          orders.send_msg(Msg::LogicFinalize);
        },
        _ => {
          model.log(
            format!(
              "Error, unknown code {} in position {}\n",
              model.lookup(model.position),
              model.position
            )
            .as_str(),
          );
          orders.send_msg(Msg::LogicFinalize);
        },
      }
    },
    Msg::LogicFinalize => {
      model.ui_model.process_button_disabled = false;
    },
  }
}

/// The text area (disabled) that shows the history of actions that have
/// occurred on the Int Code program.
fn view_history(model: &Model) -> Node<Msg> {
  div![
    style! {
      St::Display => "flex";
      St::FlexDirection => "column";
    },
    label![
      attrs![
        At::For => "history-area";
      ],
      "History",
    ],
    textarea![
      attrs! {
        At::ReadOnly => true.as_at_value();
        At::Rows => 50;
        At::Value => model.ui_model.operations_history;
        At::Wrap => "soft";
      },
      id!("history-area"),
    ],
  ]
}

/// Text area to enter problem input.
fn view_input(model: &Model) -> Node<Msg> {
  div![
    style! {
      St::Display => "flex";
      St::FlexDirection => "column";
    },
    label![
      attrs![
        At::For => "input-area";
      ],
      "Problem Input",
    ],
    textarea![
      attrs! {
        At::AutoComplete => false.as_at_value();
        At::AutoFocus => true.as_at_value();
        At::ReadOnly => false.as_at_value();
        At::Wrap => "soft";
      },
      id!("input-area"),
      input_ev(Ev::Input, Msg::InputChanged),
      model.ui_model.input,
    ],
  ]
}

/// Text area showing the processing of the result.
fn view_process_area(model: &Model) -> Node<Msg> {
  let i64_visualization: Vec<Node<Msg>> = model
    .int_code
    .iter()
    .enumerate()
    .map(|(i, v)| view_process_area_helper(*v, model.position == i))
    .collect();
  div![
    style! {
      St::Display => "flex";
      St::FlexDirection => "column";
    },
    button![
      attrs! {
        At::Disabled =>
          (model.ui_model.process_button_disabled ||
            model.ui_model.input.is_empty())
            .as_at_value();
      },
      simple_ev(Ev::Click, Msg::ProcessInputClicked),
      "Process Input"
    ],
    label![
      attrs! {
        At::Value =>
          format!("Answer: {}",
            if model.int_code.len() > 0 { model.int_code[0].to_string() }
            else { "".into() });
      },
      format!(
        "Answer: {}",
        if model.int_code.len() > 0 {
          model.int_code[0].to_string()
        } else {
          "".into()
        }
      ),
    ],
    //    label![format!("{:?}", model.int_code)],
    div![
      style! {
        St::Display => "flex";
        St::FlexDirection => "row";
        St::FlexWrap => "wrap";
      },
      i64_visualization,
    ]
  ]
}

/// Convert an i64 to a node, with emphasis placed on "important" nodes.
fn view_process_area_helper(num: i64, important: bool) -> Node<Msg> {
  div![
    style! {
      St::Margin => "5px";
    },
    if important {
      label![strong![num.to_string()]]
    } else {
      label![num.to_string()]
    }
  ]
}

/// Main view function that brings together views for the input, process area,
/// and history.
fn view(model: &Model) -> impl View<Msg> {
  div![
    style! {
      St::Display => "flex";
      St::FlexDirection => "row";
      St::FlexWrap => "wrap";
    },
    div![
      style! {
        St::Display => "flex";
        St::FlexDirection => "column";
        St::Width => "50%";
        St::Height => "100%";
      },
      view_input(model),
      view_process_area(model),
    ],
    div![
      style! {
        St::Display => "flex";
        St::FlexDirection => "column";
        St::Width => "50%";
        St::Height => "100%";
      },
      view_history(model),
    ]
  ]
}

#[wasm_bindgen(start)]
pub fn render() { App::builder(update, view).build_and_start(); }
