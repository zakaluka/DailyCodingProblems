/// # Problem 7
///
/// The problem statement is as follows:
///
/// > A unival tree (which stands for "universal value") is a tree where all
/// nodes under it have the same value. >
/// > Given the root to a binary tree, count the number of unival subtrees.
/// >
/// > For example, the following tree has 5 unival subtrees:
/// >
/// > ```text
/// >   0
/// >  / \
/// > 1   0
/// >    / \
/// >   1   0
/// >  / \
/// > 1   1
/// > ```
///
/// _NOTE: All the code in this post, including this write-up itself, can be
/// found or generated from the
/// [GitHub repository](https://github.com/zakaluka/DailyCodingProblems)._
///
/// # Solution
///
/// The solution has two major parts:
///
/// - The data model, which uses a "classical" representation of a tree.
/// - The algorithms, which uses a depth-first search.
///
/// ## Assumptions
///
/// The solution makes the following assumptions:
///
/// - Each tree is a full binary tree.
/// - The tree is not deep enough that it will cause a stack overflow (as part
///   of a [depth-first search][2] algorithm).
/// - Instead of making the data structure generic, `Branch`es and `Leaf`s of a
///   `Tree` hold a boolean as their value.  Making this value an integer or
///   even a generic type adds complexity without adding any value to the final
///   solution.
///
/// According to Wikipedia, a [full binary tree][1] is defined as follows:
///
/// > A full binary tree (sometimes referred to as a proper or plane binary
/// > tree) is a tree in which every node has either 0 or 2 children. Another
/// > way of defining a full binary tree is a recursive definition. A full
/// > binary tree is either:
/// >
/// > - A single vertex.
/// > - A tree whose root node has two subtrees, both of which are full binary
/// > trees.
///
/// ## Data model
///
/// Here is the data structure used to hold the `Tree`:
///
/// ```text
/// pub enum Tree {
///   Leaf(bool),
///   Branch(bool, Box<Tree>, Box<Tree>),
/// }
/// ```
///
/// The sub-trees are `Box`ed so that the compiler knows the size of a `Branch`
/// when one is created.
///
/// ## Algorithm
///
/// The solution performs a depth-first search to find unival trees, going down
/// the "left" side of the tree first.  Each function call takes a tree,
/// specifically a `Box<Tree>`, as input.  The output is a tuple consisting of
/// an `Option` and an integer, `(Option<bool>, i32)`.  If the `Option` is
/// holding a value, it indicates that the sub-tree which was analyzed is a
/// unival tree and the value represents the boolean value of that unival tree.
/// The integer represents the number of unival trees found so far as part of
/// the search.
///
/// ```text
/// pub(in crate) fn depth_first_search_helper(
///   tree: Box<Tree>,
/// ) -> (Option<bool>, i32) {
///   match *tree {
///     Tree::Leaf(x) => (Option::from(x), 1),
///     Tree::Branch(x, lt, rt) => {
///       // Count the number of univals along the left child.
///       let (l_unival, lcount) = depth_first_search_helper(lt);
///
///       // Count the number of univals along the right child.
///       let (r_unival, rcount) = depth_first_search_helper(rt);
///
///       match (l_unival, r_unival) {
///         // If the left or right sub-trees don't have the same value, then
///         // just send over any counts from both sub-trees.
///         (None, _) => (None, lcount + rcount),
///         (_, None) => (None, lcount + rcount),
///
///         // If the left and sub-trees both have the same values within their
///         // respective sub-trees, check if the two sub-trees have the same
///         // value and if it matches the current node.
///         (Some(lv), Some(rv)) =>
///           if lv == rv && lv == x {
///             (Some(x), lcount + rcount + 1)
///           } else {
///             (None, lcount + rcount)
///           },
///       }
///     },
///   }
/// }
/// ```
///
/// # Testing
///
/// This code was tested using the example provided by the original problem and by using property-based testing.  Unfortunately, since there is only one implementation of the algorithm, it is difficult to independently verify the results.  The property-based testing checked for two main properties:
///
/// - Number of unival tress > 0
/// - Number of unival tress >= number of leaves in the tree.
///
/// # Benchmarking
///
/// The solution was benchmarked using [Criterion.rs][3].  Input values for the benchmark used numbers from 1 to 20 to indicate the number of levels in the input sample.  Values for the nodes were picked randomly using the [`rand`][4] package.
///
/// For benchmarking purposes, the system produced "perfect" binary trees, i.e. binary trees where all interior nodes are `Branch`es and all `Leaf`s have the same depth.
///
/// The following line chart CONTINUE FROM HERE, NEED TO BENCHMARK MEMORY USAGE.
///
/// [1]: https://en.wikipedia.org/wiki/Binary_tree
/// [2]: https://en.wikipedia.org/wiki/Depth-first_search
/// [3]: https://bheisler.github.io/criterion.rs/book/
/// [4]: https://rust-random.github.io/book/
pub mod problem_8 {
  pub mod tree {
    /// A simple tree structure modeled as an algebraic data type. This data
    /// structure assumes that the tree is a "full" binary tree. It does not
    /// allow `Branch`es with a single child.
    #[derive(Debug, PartialEq, Hash, Clone)]
    pub enum Tree {
      Leaf(bool),
      Branch(bool, Box<Tree>, Box<Tree>),
    }

    /// This function performs a depth-first search to find all unival trees
    /// within the structure.
    ///
    /// Assumption: This is a "full" binary tree, i.e. all nodes are either
    /// branches with two children or leaves with no children.
    pub(in crate) fn depth_first_search_helper(
      tree: Box<Tree>,
    ) -> (Option<bool>, i32) {
      match *tree {
        Tree::Leaf(x) => (Option::from(x), 1),
        Tree::Branch(x, lt, rt) => {
          // Count the number of univals along the left child.
          let (l_unival, lcount) = depth_first_search_helper(lt);

          // Count the number of univals along the right child.
          let (r_unival, rcount) = depth_first_search_helper(rt);

          match (l_unival, r_unival) {
            // If the left or right sub-trees don't have the same value, then
            // just send over any counts from both sub-trees.
            (None, _) => (None, lcount + rcount),
            (_, None) => (None, lcount + rcount),

            // If the left and sub-trees both have the same values within their
            // respective sub-trees, check if the two sub-trees have the same
            // value and if it matches the current node.
            (Some(lv), Some(rv)) =>
              if lv == rv && lv == x {
                (Some(x), lcount + rcount + 1)
              } else {
                (None, lcount + rcount)
              },
          }
        },
      }
    }

    /// The public function to count unival trees given a binary tree.
    pub fn count_unival_trees(t: Box<Tree>) -> i32 {
      let (_, count) = depth_first_search_helper(t);
      count
    }

    /// Unit tests of private functions must be contained within the same module
    /// file.
    #[cfg(test)]
    mod tests {
      use crate::problem_8::tree::depth_first_search_helper;
      use crate::problem_8::tree::Tree::{Branch, Leaf};

      #[test]
      fn test_problem_private() {
        let t = Box::new(Branch(
          false,
          Box::new(Leaf(true)),
          Box::new(Branch(
            false,
            Box::new(Branch(true, Box::new(Leaf(true)), Box::new(Leaf(true)))),
            Box::new(Leaf(false)),
          )),
        ));

        assert_eq!(depth_first_search_helper(t), (None, 5))
      }
    }
  }
}
