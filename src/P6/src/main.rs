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
  fn new(elt: i32, prev: Option<&XORLinkedListNode>) -> XORLinkedListNode {
    XORLinkedListNode {
      element: elt,
      both: match prev {
        Some(p) => address(p),
        None => EMPTY,
      },
    }
  }
}

/// Gets the memory address of an object.
fn address<T>(elt: &T) -> usize {
  usize::from_str_radix(format!("{:p}", elt).trim_start_matches("0x"), 16)
    .unwrap()
}

/// This struct represents a collection of nodes, plus additional book-keeping
/// items needed to make the linked list work in Rust. This could be done with
/// an array but vectors provide certain features like automatic capacity
/// increases which we can take advantage of here.
///
/// From a memory management perspective, we must store / refer to
/// XORLinkedListNode instances somewhere because Rust has a strong concept of
/// ownership. When an item has no owner and no references, its destructor will
/// automatically be invoked via the `Drop` trait. Thus, all instances are
/// stored in an internal vector.
#[derive(PartialEq, Debug, Default)]
struct XORLinkedList<'a> {
  nodes: Vec<&'a XORLinkedListNode>,
}

impl XORLinkedList {
  /// Adds an item to the end of the linked list.
  fn add(&mut self, v: i32) -> &XORLinkedList {
    let l = self.nodes.len();

    // Linked list is empty, so add the value with an `EMPTY` both
    // pointer.
    if l == 0 {
      // First, create the new node .
      let n = XORLinkedListNode::new(v, None);

      // Add node to the internal vector.
      self.nodes.push(&n);

      self
    } else {
      // Create new tail node with `both` set to the address of the old tail.
      // Then, adds the new tail to the internal vector.
      self
        .nodes
        //        .push(XORLinkedListNode::new(v, address(&self.nodes[l - 1])));
        .push(XORLinkedListNode::new_single(v));

      self.nodes[l].both = address(&self.nodes[l - 1]);

      println!(
        "Adding node at index {}, addr of l-1: {}, addr of l: {}",
        l,
        address(&self.nodes[l - 1]),
        address(&self.nodes[l])
      );

      print!("idx: {}, Before: {}, ", l - 1, self.nodes[l - 1].both);

      // Fixes the old tail's `both` to point to the address of the new tail.
      self.nodes[l - 1].both = self.nodes[l - 1].both ^ address(&self.nodes[l]);

      println!("After : {}", self.nodes[l - 1].both);

      // Get last node
      //      let  tl =  &mut self.nodes[l - 1];

      // Create new tail node
      //      let n = XORLinkedListNode::new(v, address(tl));

      // Add the new tail node to the internal vector
      //      self.nodes.push(n);

      // Set former tail's `both` = `prev` XOR `new tail`
      //      tl.both = tl.both ^ address(&self.nodes[l]);

      self
    }
  }

  /// Returns the `XORLinkedListNode` at the given index. This will panic if the
  /// index is outside the length of the linked list.
  fn get(&self, index: usize) -> &XORLinkedListNode {
    /// (Tail-)Recursive function that returns the node at the given index. `n`
    /// represents the current node under investigation. `idx` represents the
    /// index being counted down (0-based). `prev_address` represents the memory
    /// address of the previous node, so that it can be XORed with `n.both` to
    /// get the next node's address.
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
  fn test_new_single_lln() {
    let node = XORLinkedListNode::new_single(5);

    assert_ne!(address(&node), 0 as usize);
    assert_eq!(node.element, 5);
  }

  #[test]
  fn test_new_two_lln() {
    let node1 = XORLinkedListNode::new_single(2);
    let node2 = XORLinkedListNode::new(3, address(&node1));

    assert_ne!(address(&node1), 0 as usize);
    assert_ne!(address(&node2), 0 as usize);
    assert_eq!(node1.element, 2);
    assert_eq!(node2.element, 3);
    assert_eq!(node2.both, address(&node1));
  }

  #[test]
  fn test_new_three_lln() {
    let mut node1 = XORLinkedListNode::new_single(2);
    let mut node2 = XORLinkedListNode::new_single(3);
    let mut node3 = XORLinkedListNode::new_single(4);
    let node2b = XORLinkedListNode::new(3, address(&node1) ^ address(&node3));

    // Set the `both` attributes
    node1.both = address(&node2);
    node2.both = address(&node1) ^ address(&node3);
    node3.both = address(&node2);

    assert_ne!(address(&node1), 0 as usize);
    assert_ne!(address(&node2), 0 as usize);
    assert_ne!(address(&node2b), 0 as usize);
    assert_ne!(address(&node3), 0 as usize);
    assert_eq!(node1.element, 2);
    assert_eq!(node2.element, 3);
    assert_eq!(node2b.element, 3);
    assert_eq!(node3.element, 4);
    assert_eq!(node2.both, node2b.both);
    assert_eq!(node2, node2b);
  }

  #[test]
  fn test_new_ll() {
    let lst = XORLinkedList::new();

    // Item was initialized and was not set to address '0x0'.
    assert_ne!(address(&lst), 0 as usize);
  }

  #[test]
  fn test_single_ll() {
    // Test add here
    let mut lst = XORLinkedList::new();

    lst.add(15);

    assert_eq!(lst.nodes.len(), 1);
    assert_eq!(lst.get(0).element, 15);
  }

  #[test]
  fn test_adds_ll() {
    let mut lst2 = XORLinkedList::new();

    lst2.add(10);
    lst2.add(11);
    lst2.add(12);
    lst2.add(13);
    lst2.add(14);
    lst2.add(15);
  }

  #[test]
  fn test_adds_ll_internals() {
    let mut lst2 = XORLinkedList::new();

    lst2.add(10);
    lst2.add(11);
    lst2.add(12);
    lst2.add(13);
    lst2.add(14);
    lst2.add(15);

    assert_eq!(lst2.nodes.len(), 6, "v.len");
    println!(
      "v0: {}, v1 is at {}",
      lst2.nodes[0].both,
      address(&lst2.nodes[1])
    );

    assert_eq!(lst2.nodes[0].both, address(&lst2.nodes[1]), "v[0]");
    //    assert_eq!(v[1].both, address(&v[0]) ^ address(&v[2]), "v[1]");
  }

  //  #[test]
  fn test_gets_ll() {
    let mut lst3 = XORLinkedList::new();

    lst3.add(10);
    lst3.add(11);
    lst3.add(12);
    lst3.add(13);
    lst3.add(14);
    lst3.add(15);

    assert_eq!(lst3.get(0).element, 10);
    assert_eq!(lst3.get(1).element, 11);
    assert_eq!(lst3.get(2).element, 12);
    assert_eq!(lst3.get(3).element, 13);
    assert_eq!(lst3.get(4).element, 14);
    assert_eq!(lst3.get(5).element, 15);
  }
}

fn main() {
  println!("Hello, world!");
}
