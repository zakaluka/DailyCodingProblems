//! # Problem 6 - XOR Linked List
//!
//! The problem statement is as follows:
//!
//! > An XOR linked list is a more memory efficient doubly linked list. Instead
//! > of each node holding `next` and `prev` fields, it holds a field named
//! > `both`, which is an XOR of the next node and the previous node. Implement
//! > an XOR linked list; it has an `add(element)` which adds the element to the
//! > end, and a `get(index)` which returns the node at index.
//!
//! > If using a language that has no pointers (such as Python) you can assume
//! > you have access to `get_pointer` and `dereference_pointer` functions that
//! > converts between nodes and memory addresses.
//!
//! _NOTE: All the code in this post, including this write-up itself, can be
//! found or generated from the [GitHub repository][5]._
//!
//! # Preface to Solution
//!
//! This problem represents a large gap in my recent blog posts because, when I
//! encountered this problem, I saw it as an opportunity to learn [Rust][3].
//!
//! For those who are not familiar, here is a short description from
//! [Mozilla Research][4]:
//!
//! > Rust is an open-source systems programming language that focuses on speed,
//! > memory safety and parallelism. Developers are using Rust to create a wide
//! > range of new software applications, such as game engines, operating
//! > systems, file systems, browser components and simulation engines for
//! > virtual reality.
//!
//! Rust is a "low-level" language whose syntax is quite close to C/C++.
//! However, it also provides a number of higher-level concepts from other
//! object-oriented and functional programming languages.
//!
//! In particular, Rust has a very strong concept of memory safety through
//! concepts of [ownership][1] and [borrowing][2]. While there are lots of other
//! articles about this feature and its implementation, here is how it finally
//! made sense to me: Rust's memory safety is somewhat similar to a pessimistic
//! database lock. Because mutability is very explicit in Rust, there can be as
//! many `readers` as desired for a single location in memory. However, when
//! a `writer` wants to mutate that memory, it receives and keeps exclusive
//! access to that memory until the work is complete.
//!
//! As you can imagine, doubly linked lists are particularly annoying in Rust
//! because a common question that comes up is 'Who owns the current node, the
//! previous or the next node'. There is an excellent [article][6] about linked
//! lists in Rust for those wanting a correct and much better introduction to
//! linked lists in Rust.
//!
//! It took me the better part of the last few months to learn enough about Rust
//! to get this exercise working. I gave up pretty much once a month, wrote this
//! solution partially in [Go][7] and in [F#][8], and did a lot of
//! experimentation to get to this point. In the end, I was not able to make my
//! code completely 'safe' in Rust, but I am not sure that is possible with an
//! XOR linked list. In any case, all feedback is welcome and greatly
//! appreciated.
//!
//! # Solution
//!
//! ## Data model
//!
//! The key to this problem, in my mind, is to design the correct data structure
//! that can work with an XOR linked list while also ensuring that objects are
//! not prematurely destroyed (a problem in Rust since the language calls the
//! [destructor][9] as soon as the last reference to an object goes out of
//! scope).
//!
//! Going with the classic model, a linked list is made of a series of nodes.
//! My choice was to create two data structures that, together, represent an XOR
//! linked list:
//!
//! 1. An `XORLinkedListNode` is a [struct][10] (similar to a F# record, a Java
//! class without methods, or a data transfer object) that holds the following
//! information:
//!     1. `element` holds an integer, representing the value of the node.
//!     1. `both` holds the XOR'ed memory address.
//! 1. An `XORLinkedList` is a struct that holds the following information:
//!     1. `nodes` keeps a reference to all `XORLinkedListNode` objects so
//! that Rust does not `drop` while they are still part of the list.
//!
//! In addition to the above, there are two additional considerations to keep in
//! mind:
//!
//! - The Rust compiler is free to move objects around in memory for efficiency
//!   purposes, whether on the stack or the heap. Because of the strong memory
//!   safety that Rust provides, this is considered a relatively safe action.
//!   However, if the compiler chooses to move items around, that would
//!   invalidate the `both` pointer. To prevent this, each node has a 3rd
//!   element in its struct called `_pin`.
//!     - `_pin` forces Rust to pin an object to a memory address. More
//!       information can be found [here][11].
//! - By default, Rust creates objects on the stack. To create objects on the
//!   heap so that they outlive the execution of a function, Rust provides
//!   [boxed][12] values.
//!     - The types of `nodes` in the `XORLinkedList` is
//! `Vec<Pin<Box<XORLinkedListNode>>>`, i.e. a vector of boxed, pinned
//! `XORLinkedListNode` values.
//!
//! ## XORLinkedListNode
//!
//! `XORLinkedListNode` provides two methods.  THe first is the equivalent of a
//! simple constructor that creates new nodes. The second is a `set_both`
//! function that changes the value of `both` for a boxed, pinned node.
//!
//! ```text
//! unsafe fn set_both(new_both: usize, node: &mut Pin<Box<XORLinkedListNode>>)
//! {
//!   let x = node.as_mut();
//!   Pin::get_unchecked_mut(x).both = new_both;
//! }
//! ```
//!
//! ## XORLinkedList
//!
//! `XORLinkedList` provides the two methods requested in the original problem
//! statement, `add` and `get`.
//!
//! `add` is responsible for two key actions:
//!
//! 1. Creating the new node.
//! 1. Fixing the `both` attribute of the last node in a non-empty linked list.
//!
//! ```text
//! // Adds an item to the end of the linked list.
//! fn add(&mut self, v: i32) -> &XORLinkedList {
//!   // Linked list is empty, so add the value with an `EMPTY` both
//!   // pointer.
//!   if self.nodes.is_empty() {
//!     // First, create the new node.
//!     let n = XORLinkedListNode::new(v, None);
//!
//!     // Add node to the internal vector.
//!     self.nodes.push(n);
//!
//!     self
//!   } else {
//!     // Convenience variable for current length.
//!     let l = self.nodes.len();
//!
//!     // Create new tail node with `both` set to the address of the old tail.
//!     // Then, adds the new tail to the internal vector.
//!     self.nodes.push(XORLinkedListNode::new(
//!       v,
//!       Option::from(self.nodes[l - 1].deref()),
//!     ));
//!
//!     // Fixes the old tail's `both` to point to the address of the new tail.
//!     unsafe {
//!       XORLinkedListNode::set_both(
//!         self.nodes[l - 1].both ^ address(self.nodes[l].deref()),
//!         &mut self.nodes[l - 1],
//!       );
//!     }
//!
//!     self
//!   }
//! }
//! ```
//!
//! `get` uses a recursive helper function to walk the list and retrieve the
//! desired index. In its current implementation, `get` will `panic` if the
//! requested index is outside the length of the linked list.
//!
//! ```text
//! // Returns the `XORLinkedListNode` at the given index. This will panic if
//! // the index is outside the length of the linked list.
//! fn get(&self, index: usize) -> &XORLinkedListNode {
//!   // (Tail-)Recursive function that returns the node at the given index. `n`
//!   // represents the current node under investigation. `idx` represents the
//!   // index being counted down (0-based). `prev_address` represents the
//!   // memory address of the previous node, so that it can be XORed with
//!   // `n.both` to get the next node's address.
//!   fn get_helper(
//!     n: &XORLinkedListNode,
//!     idx: usize,
//!     prev_address: usize,
//!   ) -> &XORLinkedListNode {
//!     if idx == 0 {
//!       n
//!     } else {
//!       // Get the address of the next node.
//!       let next_address = n.both ^ prev_address;
//!       let next_n = next_address as *const XORLinkedListNode;
//!       get_helper(unsafe { &(*next_n) }, idx - 1, address(n))
//!     }
//!   }
//!
//!   // Call `get_helper`. The previous node of the first entry in the list is
//!   // always `EMPTY`.
//!   get_helper(&self.nodes[0], index, EMPTY)
//! }
//! ```
//!
//! ## Utility methods
//!
//! The code uses one utility constant and one utility function to make coding
//! easier.
//!
//! The first is a constant called `EMPTY` which represents the so-called empty
//! address, i.e. `0x0`.
//!
//! The second is a function that returns the address of an object.
//!
//! ```text
//! // Gets the memory address of an object.
//! fn address<T>(elt: &T) -> usize {
//!   usize::from_str_radix(format!("{:p}", elt).trim_start_matches("0x"), 16)
//!     .unwrap()
//! }
//! ```
//!
//! # Testing
//!
//! Testing in Rust was extremely easy, thanks to built-in support using the
//! `cargo test` command.
//!
//! For this exercise, I did not perform any property-based testing. By the end,
//! I just wanted to get my code working. However, this exercise really
//! highlighted the value of unit testing. It was through testing that I
//! realized that Rust is free to move objects around in memory, even if they
//! were allocated on the heap. I don't know if other languages also have this
//! behavior, but it was very surprising to me. That is what led me down the
//! rabbit hole of both pinned and boxed values.
//!
//! # Conclusion
//!
//! Even though this was my first coding challenge in Rust, I don't think I
//! could have picked a better problem to help me learn about some key concepts
//! in Rust:
//!
//! 1. Heap allocations
//! 1. Memory safety
//! 1. Unsafe functions
//! 1. Immutability vs. mutability
//!
//! I am planning to write the next few solutions in Rust to build up my
//! knowledge of the language.
//!
//! See you in the next one!
//!
//! [1]: https://doc.rust-lang.org/book/ch04-01-what-is-ownership.html
//! [2]: https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html
//! [3]: https://www.rust-lang.org/
//! [4]: https://research.mozilla.org/rust/
//! [5]: https://github.com/zakaluka/DailyCodingProblems
//! [6]: https://rust-unofficial.github.io/too-many-lists/
//! [7]: https://golang.org/
//! [8]: https://fsharp.org/
//! [9]: https://doc.rust-lang.org/nomicon/destructors.html
//! [10]: https://doc.rust-lang.org/book/ch05-01-defining-structs.html
//! [11]: https://doc.rust-lang.org/nightly/std/pin/index.html
//! [12]: https://doc.rust-lang.org/std/boxed/index.html

use std::marker::PhantomPinned;
use std::ops::Deref;
use std::pin::Pin;

/// Represents the empty address, i.e. the `prev` of the `head` of the list.
const EMPTY: usize = 0;

/// The linked list is a series of Nodes.  This is modeled as a `struct` with
/// an `element` of type `i32` and a `both` pointer of type `usize` containing
/// an XOR of the addresses of the previous and next nodes.
///
/// If this were not a doubly linked list, then Node could contain a reference
/// to a strongly-typed `tail` instead of just an `usize`.
#[derive(PartialEq, Eq, Debug)]
struct XORLinkedListNode {
  element: i32,
  both: usize,
  _pin: PhantomPinned,
}

impl XORLinkedListNode {
  fn new(elt: i32, prev: Option<&XORLinkedListNode>) -> Pin<Box<Self>> {
    let node = XORLinkedListNode {
      element: elt,
      both: match prev {
        Some(p) => address(p),
        None => EMPTY,
      },
      _pin: PhantomPinned,
    };

    // Return a pinned version of the node, saved on the heap.
    Box::pin(node)
  }

  /// Centralizes the logic to change the value of a node's `both` attribute.
  unsafe fn set_both(new_both: usize, node: &mut Pin<Box<XORLinkedListNode>>) {
    let x = node.as_mut();
    Pin::get_unchecked_mut(x).both = new_both;
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
struct XORLinkedList {
  nodes: Vec<Pin<Box<XORLinkedListNode>>>,
}

impl XORLinkedList {
  /// Adds an item to the end of the linked list.
  fn add(&mut self, v: i32) -> &XORLinkedList {
    // Linked list is empty, so add the value with an `EMPTY` both
    // pointer.
    if self.nodes.is_empty() {
      // First, create the new node.
      let n = XORLinkedListNode::new(v, None);

      // Add node to the internal vector.
      self.nodes.push(n);

      self
    } else {
      // Convenience variable for current length.
      let l = self.nodes.len();

      // Create new tail node with `both` set to the address of the old tail.
      // Then, adds the new tail to the internal vector.
      self.nodes.push(XORLinkedListNode::new(
        v,
        Option::from(self.nodes[l - 1].deref()),
      ));

      // Fixes the old tail's `both` to point to the address of the new tail.
      unsafe {
        XORLinkedListNode::set_both(
          self.nodes[l - 1].both ^ address(self.nodes[l].deref()),
          &mut self.nodes[l - 1],
        );
      }

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

    // Call `get_helper`. The previous node of the first entry in the list is
    // always `EMPTY`.
    get_helper(&self.nodes[0], index, EMPTY)
  }
}

#[cfg(test)]
mod tests {
  use super::*;

  #[test]
  /// Tests basic node creation.
  fn test_new_single_lln() {
    let node = XORLinkedListNode::new(5, None);

    assert_ne!(address(node.deref()), 0, "node was allocated");
    assert_eq!(node.element, 5, "node value is correct");
  }

  #[test]
  /// Tests 2-node creation with one of the `both` parameters being set.
  fn test_new_two_lln() {
    let node1 = XORLinkedListNode::new(2, None);
    let node2 = XORLinkedListNode::new(3, Option::from(node1.deref()));

    assert_ne!(address(node1.deref()), 0, "node1 was allocated");
    assert_ne!(address(node2.deref()), 0, "node2 was allocated");
    assert_eq!(node1.element, 2, "node1 value is correct");
    assert_eq!(node2.element, 3, "node2 value is correct");
    assert_eq!(
      node2.both,
      address(node1.deref()),
      "node2.both point to node1"
    );
  }

  #[test]
  /// Tests that the `set_both` method within `XORLinkedListNode` works.
  fn test_new_three_lln() {
    let mut node2 = XORLinkedListNode::new(3, None);
    let node1 = XORLinkedListNode::new(2, Option::from(node2.deref()));
    let node3 = XORLinkedListNode::new(4, Option::from(node2.deref()));

    let mut node2b = XORLinkedListNode::new(3, None);

    // Set the `both` attributes
    unsafe {
      // Set `both` manually.
      Pin::get_unchecked_mut(node2.as_mut()).both =
        address(node1.deref()) ^ address(node3.deref());

      // Set `both` using convenience function.
      XORLinkedListNode::set_both(
        address(node1.deref()) ^ address(node3.deref()),
        &mut node2b,
      );
    }

    assert_ne!(address(node1.deref()), 0, "node1 was allocated");
    assert_ne!(address(node2.deref()), 0, "node2 was allocated");
    assert_ne!(address(node2b.deref()), 0, "node2b was allocated");
    assert_ne!(address(node3.deref()), 0, "node3 was allocated");
    assert_eq!(node1.element, 2, "node1 value is correct");
    assert_eq!(node2.element, 3, "node2 value is correct");
    assert_eq!(node2b.element, 3, "node2b value is correct");
    assert_eq!(node3.element, 4, "node3 value is correct");
    assert_eq!(
      node2.both, node2b.both,
      "node2 and node2b have the same .both value"
    );
    assert_eq!(node2, node2b, "node2 and node2b are equal");
  }

  #[test]
  /// Tests basic list creation.
  fn test_new_ll() {
    let lst: XORLinkedList = XORLinkedList::default();

    // Item was initialized and was not set to address '0x0'.
    assert_ne!(address(&lst), 0, "list is not at 0 address");
    assert_eq!(lst.nodes.len(), 0, "list is empty");
  }

  #[test]
  /// Test list with 1 node.
  fn test_single_ll() {
    // Test add here
    let mut lst = XORLinkedList::default();

    lst.add(15);

    assert_eq!(lst.nodes.len(), 1, "list has 1 node");
    assert_eq!(lst.get(0).element, 15, "element is 15");
    assert_eq!(lst.get(0).both, EMPTY, "both is EMPTY");
  }

  #[test]
  /// Tests that more than 1 item can be added to a list.
  fn test_adds_ll() {
    let mut lst = XORLinkedList::default();

    lst.add(10);
    lst.add(11);
    lst.add(12);
    lst.add(13);
    lst.add(14);
    lst.add(15);
  }

  #[test]
  /// Tests the internals of `add` on a list.
  fn test_adds_ll_internals() {
    let mut lst2 = XORLinkedList::default();

    lst2.add(10);
    lst2.add(11);
    lst2.add(12);
    lst2.add(13);
    lst2.add(14);
    lst2.add(15);

    assert_eq!(lst2.nodes.len(), 6, "v.len");
    assert_eq!(
      lst2.nodes[0].both,
      address(lst2.nodes[1].deref()),
      "lst2[0].both = lst2[1]"
    );
    assert_eq!(
      lst2.nodes[1].both,
      address(lst2.nodes[0].deref()) ^ address(lst2.nodes[2].deref()),
      "lst2[1].both = lst2[0] ^ lst2[2]"
    );
  }

  #[test]
  /// Test of `get` for linked lists.
  fn test_gets_ll() {
    let mut lst3 = XORLinkedList::default();

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
