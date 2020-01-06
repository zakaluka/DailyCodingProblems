pub mod problem_8 {
  pub mod tree {
    /// A simple tree structure modeled as an algebraic data type. This data
    /// structure assumes that the tree is a "complete" binary tree. It does not
    /// allow `Branch`es with a single child.
    #[derive(Debug, PartialEq, Hash, Clone)]
    pub enum Tree {
      Leaf(bool),
      Branch(bool, Box<Tree>, Box<Tree>),
    }

    /// This function performs a depth-first search to find all unival trees
    /// within the structure.
    ///
    /// Assumption: This is a "complete" binary tree, i.e. all nodes are either
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
            // respective sub-trees, check if the two sub-tress have the same
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
    /// / file.
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
