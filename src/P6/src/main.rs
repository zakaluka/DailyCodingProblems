/// Represents the empty address, i.e. the `prev` of the `head` of the list.
const EMPTY: usize = 0;

/// The linked list is a series of Nodes.  This is modeled as a `struct` with
/// an `element` of type `i32` and a `both` pointer of type `usize` containing
/// an XOR of the addresses of the previous and next nodes.
///
/// If this were not a doubly linked list, then Node could contain a reference
/// to a strongly-typed `tail` instead of just an `usize`.
#[derive(PartialEq, Debug)]
struct XORLinkedListNode {
  element: i32,
  both: usize,
}

impl XORLinkedListNode {
  fn new_single(elt: i32) -> XORLinkedListNode {
    XORLinkedListNode {
      element: elt,
      both: EMPTY,
    }
  }

  fn new(elt: i32, address: usize) -> XORLinkedListNode {
    XORLinkedListNode {
      element: elt,
      both: address,
    }
  }

  //  fn address(&self) -> usize {
  //    usize::from_str_radix(format!("{:p}", self).trim_start_matches("0x"),
  // 16)      .unwrap()
  //  }
}

fn address<T>(elt: &T) -> usize {
  usize::from_str_radix(format!("{:p}", elt).trim_start_matches("0x"), 16)
    .unwrap()
}

/// This struct represents a collection of nodes, plus additional book-keeping
/// items needed to make the linked list work in Rust. This could be done with
/// an array but vectors provide certain features like automatic capacity
/// increases which we can take advantage of here.
///
/// From a memory management perspective, we must store the XORLinkedList
/// instances somewhere because Rust has a strong concept of ownership. When an
/// item has no owner and no references, its destructor will automatically be
/// invoked via the `Drop` trait.
#[derive(PartialEq, Debug)]
struct XORLinkedList {
  nodes: Vec<XORLinkedListNode>,
}

impl XORLinkedList {
  /// Constructor for an empty linked list.
  fn new() -> XORLinkedList { XORLinkedList { nodes: Vec::new() } }

  /// Adds an item to the end of the linked list.
  fn add(&mut self, v: i32) -> &XORLinkedList {
    let l = self.nodes.len();

    // Linked list is empty, so add the value with an `EMPTY` both
    // pointer.
    if l == 0 {
      // First, create the new node.
      let n = XORLinkedListNode::new_single(v);

      // Add node to the internal vector.
      self.nodes.push(n);

      self
    } else {
      // Get last node
      let tl = &mut self.nodes[l - 1];

      // Create new tail node
      let n = XORLinkedListNode::new(v, address(tl));

      // Set former tail's `both` = `prev` XOR `new tail`
      tl.both = tl.both ^ address(&n);

      self
    }
  }

  /// Returns the `XORLinkedListNode` at the given index. This will fail if the
  /// index is outside the length of the linked list.
  fn get(&self, index: usize) -> &XORLinkedListNode {
    fn get_helper(
      n: &XORLinkedListNode,
      idx: usize,
      prev_address: usize,
    ) -> &XORLinkedListNode {
      if idx == 0 {
        n
      } else {
        // Get the address of the next node.
        let next_address = n.both ^ prev_address;
        let next_n = next_address as *const XORLinkedListNode;
        get_helper(unsafe { &(*next_n) }, idx - 1, address(n))
      }
    }
    get_helper(&self.nodes[0], index, EMPTY)
  }
}

#[cfg(test)]
mod tests {
  use super::*;

  #[test]
  fn test_new_ll() {
    let lst = XORLinkedList::new();

    // Item was initialized and was not set to address '0x0'.
    assert_ne!(address(&lst), 0 as usize);
  }

  #[test]
  fn test_new_single_lln() {
    let node = XORLinkedListNode::new_single(5);

    assert_ne!(address(&node), 0 as usize);
    assert_eq!(node.element, 5);
  }

  #[test]
  fn test_new_multi_lln() {
    let node1 = XORLinkedListNode::new_single(2);
    let node2 = XORLinkedListNode::new(3, address(&node1));

    assert_ne!(address(&node1), 0 as usize);
    assert_ne!(address(&node2), 0 as usize);
    assert_eq!(node1.element, 2);
    assert_eq!(node2.element, 3);
    assert_eq!(node2.both, address(&node1));
  }

  #[test]
  fn test_add_single() {
    // Test add here
  }
}

fn main() {
  println!("Hello, world!");
}
