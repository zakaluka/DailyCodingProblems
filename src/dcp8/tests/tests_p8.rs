#![feature(test)]

/// Import for property-based testing.
use proptest::prelude::*;

/// Import the library code for testing.
use dcp8::problem_8::tree::Tree::{Branch, Leaf};
use dcp8::problem_8::tree::{count_unival_trees, Tree};

/// A method to generate a random `Tree` for property-based testing.  The
/// strategy is broken up into two parts.  The first portion has the
/// non-recursive cases, i.e. `Leaf`.  The second portion has the recursive
/// cases, i.e. `Branch`.
fn generate_tree() -> impl Strategy<Value = Tree> {
  // Non-recursive cases.
  let leaf = prop_oneof![any::<bool>().prop_map(Tree::Leaf),];
  leaf.prop_recursive(
    14,    // depth
    16385, // desired_size
    10,    /* expected_branch_size (this parameter doesn't make as much
            * sense since we don't use collections within the Tree
            * structure. So I used the value that the documentation
            * example uses. */
    // Recursive cases.
    |inner| {
      prop_oneof![
        // Takes the inner strategy and makes the recursive case. Per the
        // documentation, `inner` is an `Arc` and, since we need to
        // reference it twice, we clone it the first time.
        (any::<bool>(), inner.clone(), inner)
          .prop_map(|(a, b, c)| Tree::Branch(a, Box::new(b), Box::new(c))),
      ]
    },
  )
}

/// Count the number of leaves in a given tree, using a basic non-tail-recursive
/// depth first search.
fn count_leaves(t: Box<Tree>) -> i32 {
  match *t {
    Tree::Leaf(_) => 1,
    Tree::Branch(_, l, r) => count_leaves(l) + count_leaves(r),
  }
}

// Property-based tests are located within this macro.
proptest! {
  /// Test to ensure that number of unival trees is always above zero, since
  /// every leaf (and there will be at least one) is a unival tree.
  #[test]
  fn test_basic(t in generate_tree()) {
    assert!(count_unival_trees(Box::new(t)) > 0);
  }

  /// Test to ensure that number of unival trees is at least equal to the
  /// number of leaves in the tree.
  #[test]
  fn test_leaves(t in generate_tree()) {
    let btree = Box::new(t);
    let num_leaves = count_leaves(btree.clone());
    assert!(count_unival_trees(btree) >= num_leaves);
  }
}

/// Runs the test that is provided in the problem statement.
#[test]
fn test_problem_public() {
  let t = Box::new(Branch(
    false,
    Box::new(Leaf(true)),
    Box::new(Branch(
      false,
      Box::new(Branch(true, Box::new(Leaf(true)), Box::new(Leaf(true)))),
      Box::new(Leaf(false)),
    )),
  ));

  assert_eq!(count_unival_trees(t), 5);
}
