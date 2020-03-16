module RBTree =
  /// Possible colors of nodes, Red and Black.
  type Color =
    | R
    | B

  type BlackHeight = int32

  type Tree<'a when 'a :> System.IComparable> =
    /// Represents an empty tree.
    | E
    | T of color: Color * left: 'a Tree * elt: 'a * right: 'a Tree

  /// Create an empty tree.
  let empty = E

  /// Creates a singleton Tree, aka a tree with no children.
  let singleton elt = T(B,E,elt,E)

  /// Check if a tree is empty.
  let isEmpty =
    function
    | E -> true
    | T(_,_,_,_) -> false

  /// Returns the color of a node, fails for an empty Tree.
  let internal getColor =
    function
    | E -> failwith "Empty tree"
    | T(c,_,_,_) -> c

  /// Returns the left sub-tree of a node, fails for an empty Tree.
  let internal getLeftTree =
    function
    | E -> failwith "Empty tree"
    | T(_,l,_,_) -> l

  /// Returns the right sub-tree of a node, fails for an empty Tree.
  let internal getRightTree =
    function
    | E -> failwith "Empty tree"
    | T(_,_,_,r) -> r

  /// Returns an element if it is present in the Tree.
  let rec internal get elt =
    function
    | E -> None
    | T(_,l,y,r) as node ->
        if elt = y then Some(node)
        else if elt < y then get elt l
        else get elt r

  /// Checks whether an element exists in the Tree.
  let rec public exists elt t =
    match get elt t with
    | None -> false
    | Some(_) -> true

  /// Balance function for the tree
  let balance color left elt right =
    match getColor right,right with
    | R,T(rC,rL,rV,rR) ->
        match getColor left,left with
        | R,T(lC,lL,lV,lR) -> T(R,T(B,lL,lV,lR),elt,T(B,rL,rV,rR))
        | _,_ -> T(color,T(R,left,elt,rL),rV,rR)
    | _,_ ->
        if not(isEmpty left) && not(isEmpty(getLeftTree left))
           && getColor left = R && getColor(getLeftTree left) = R then
          let (T(lC,lL,lV,lR)) = left
          let (T(llC,llL,llV,llR)) = getLeftTree left
          T(R,T(B,llL,llV,llR),lV,T(B,lR,elt,right))
        else
          T(color,left,elt,right)

  /// Insert helper.
  let rec internal insertHelper elt =
    function
    | E -> T(R,E,elt,E)
    | T(nC,nL,nV,nR) as node ->
        let comparison = elt.CompareTo(nV)
        if comparison < 0 then balance nC (insertHelper elt nL) nV nR
        else if comparison = 0 then node
        else balance nC nL nV (insertHelper elt nR)

  /// Insert a value into a Tree
  let insert elt t =
    let insertedTree = insertHelper elt t
    match getColor insertedTree,insertedTree with
    | R,T(_,l,v,r) -> T(B,l,v,r)
    | _,_ -> insertedTree

  do printfn "Wut?"

do printfn "Wut 2?"
