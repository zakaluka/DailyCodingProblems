namespace AOC2019_3

module RBTree =
  /// Possible colors of nodes, Red and Black.
  type Color =
    | R
    | B

  /// The Tree that all functions in this module operate on.
  type Tree<'a when 'a :> System.IComparable> =
    /// Represents an empty Tree.
    | E
    /// A node in a Tree, with possible children.
    | T of color: Color * left: 'a Tree * 'a * right: 'a Tree

  /// Create an empty tree.
  let public empty = E

  /// Returns the color of a node, fails for an empty Tree.
  let internal getColor =
    function
    | E -> failwith "Empty tree"
    | T(c,_,_,_) -> c

  /// Returns the value of a node, fails for an empty Tree.
  let internal getValue =
    function
    | E -> failwith "Empty tree"
    | T(_,_,v,_) -> v

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

  /// Returns an element's sub-tree if that element is present in the Tree.
  let rec public get elt =
    function
    | E -> None
    | T(_,l,y,r) as node ->
        if elt = y then Some(node)
        else if elt < y then get elt l
        else get elt r

  /// Checks whether an element exists in the Tree.
  let public exists elt t = get elt t |> Option.isSome

  /// Determine the number of values in the Tree.
  let public size t =
    let rec sizeHelp n t =
      match t with
      | E -> n
      | T(_,l,_,r) -> sizeHelp (sizeHelp (n + 1) r) l
    sizeHelp 0 t

  /// Check if a tree is empty.
  let public isEmpty =
    function
    | E -> true
    | T(_) -> false

  /// Balance function for the tree
  let internal balance color left elt right =
    match right with
    | T(rC,rL,rV,rR) when rC = R ->
        match left with
        | T(lC,lL,lV,lR) when lC = R -> T(R,T(B,lL,lV,lR),elt,T(B,rL,rV,rR))
        | _ -> T(color,T(R,left,elt,rL),rV,rR)
    | _ ->
        match left with
        | T(lC,T(llC,llL,llV,llR),lV,lR) when lC = R && llC = R ->
            T(R,T(B,llL,llV,llR),lV,T(B,lR,elt,right))
        | _ -> T(color,left,elt,right)

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
  let public insert elt t =
    match insertHelper elt t with
    | T(c,l,v,r) when c = R -> T(B,l,v,r)
    | insertedTree -> insertedTree

  let internal moveRedLeft t =
    match t with
    | T(_,T(_,lL,lV,lR),v,T(_,T(rlC,rlL,rlV,rlR),rV,rR)) when rlC = R ->
        T(R,T(B,T(R,lL,lV,lR),v,rlL),rlV,T(B,rlR,rV,rR))
    | T(c,T(_,lL,lV,lR),v,T(_,rL,rV,rR)) ->
        match c with
        | B -> T(B,T(R,lL,lV,lR),v,T(R,rL,rV,rR))
        | R -> T(B,T(R,lL,lV,lR),v,T(R,rL,rV,rR))
    | _ -> t

  let internal moveRedRight t =
    match t with
    | T(_,T(_,T(llC,llL,llV,llR),lV,lR),v,T(_,rL,rV,rR)) when llC = R ->
        T(R,T(B,llL,llV,llR),lV,T(B,lR,v,T(R,rL,rV,rR)))
    | T(c,T(_,lL,lV,lR),v,T(_,rL,rV,rR)) ->
        match c with
        | B -> T(B,T(R,lL,lV,lR),v,T(R,rL,rV,rR))
        | R -> T(B,T(R,lL,lV,lR),v,T(R,rL,rV,rR))
    | _ -> t

  let rec internal getMin t =
    match t with
    | T(_,T(_),_,_) -> getLeftTree t |> getMin
    | _ -> t

  let rec internal removeMin t =
    match t with
    | T(c,T(lC,lL,_,_),v,r) ->
        let l = getLeftTree t
        match lC with
        | B ->
            match lL with
            | T(llC,_,_,_) when llC = R -> T(c,(removeMin l),v,r)
            | _ ->
                match moveRedLeft t with
                | T(nC,nL,nV,nR) -> balance nC (removeMin nL) nV nR
                | E -> E
        | R -> T(c,(removeMin l),v,r)
    | _ -> E

  let internal removeHelpPrepEQGT _elt t c l v r =
    match l with
    | T(lC,lL,lV,lR) when lC = R -> T(c,lL,lV,T(R,lR,v,r))
    | _ ->
        match r with
        | T(rC,T(rlC,_,_,_),_,_) when rC = B && rlC = B -> moveRedRight t
        | T(rC,rL,_,_) when rC = B && rL = E -> moveRedRight t
        | _ -> t

  /// When we find the node we are looking for, we can remove by replacing the
  /// key-value pair with the key-value pair of the left-most node on the right
  /// side (the closest pair).
  let rec internal removeHelpEQGT elt t =
    match t with
    | E -> E
    | T(c,l,v,r) ->
        if elt = v then
          match getMin r with
          | T(_,_,minV,_) -> balance c l minV (removeMin r)
          | E -> E
        else
          balance c l v (removeHelp elt r)

  /// The easiest thing to remove from the tree is a red node. However, when
  /// searching for the node to remove, we have no way of knowing if it will
  /// be red or not. This remove implementation makes sure that the bottom
  /// node is red by moving red colors down the tree through rotation and
  /// color flips. Any violations this will cause, can easily be fixed by
  /// balancing on the way up again.
  /// https://github.com/Skinney/core/blob/master/src/Dict.elm
  and internal removeHelp elt t =
    match t with
    | E -> E
    | T(c,l,v,r) ->
        if elt < v then
          match l with
          | T(lC,lL,_,_) when lC = B ->
              match lL with
              | T(llC,_,_,_) when llC = R -> T(c,(removeHelp elt l),v,r)
              | _ ->
                  match moveRedLeft t with
                  | E -> E
                  | T(nC,nL,nV,nR) -> balance nC (removeHelp elt nL) nV nR
          | _ -> T(c,(removeHelp elt l),v,r)
        else
          removeHelpPrepEQGT elt t c l v r |> removeHelpEQGT elt

  /// Removes a value from the Tree, if it exists.  Otherwise, no action is
  /// taken.
  let public remove elt t =
    match removeHelp elt t with
    | T(c,l,v,r) when c = R -> T(B,l,v,r)
    | x -> x

  /// Creates a singleton Tree, aka a tree with no children.
  let public singleton elt = T(B,E,elt,E)

  /// Fold over the elements in a Tree from lowest to highest.
  let rec public fold func acc t =
    match t with
    | E -> acc
    | T(_,l,v,r) -> fold func (func (fold func acc l) v) r

  /// Fold over the elements in a Tree from highest to lowest.
  let rec public foldBack func t acc =
    match t with
    | E -> acc
    | T(_,l,v,r) -> foldBack func l (func v (foldBack func r acc))

  /// Keep only the elements that pass the given test.
  let public filter isGood t =
    fold (fun acc elt ->
      if isGood elt then insert elt acc else acc) empty t

  /// Partition a Tree according to some test. The first Tree contains all
  /// elements which passed the test, and the second contains the elements that
  /// did not.
  let public partition isGood t =
    let add (t1,t2) elt =
      if isGood elt then (insert elt t1),t2 else t1,insert elt t2
    fold add (empty,empty) t

  /// Convert an Array to a Tree
  let public ofArray(a: 'a array) =
    Array.fold (fun acc elt -> insert elt acc) empty a

  /// Convert a List to a Tree
  let public ofList(l: 'a list) =
    List.fold (fun acc elt -> insert elt acc) empty l

  /// Convert a Sequence to a Tree
  let public ofSeq(s: 'a seq) =
    Seq.fold (fun acc elt -> insert elt acc) empty s

  /// Convert a Set to a Tree
  let public ofSet(s: Set<'a>) =
    Set.fold (fun acc elt -> insert elt acc) empty s

  /// Convert a Tree to an Array
  let public toArray t =
    fold (fun acc elt -> Array.singleton elt |> Array.append acc) Array.empty t

  /// Convert a Tree to a List
  let public toList t = foldBack (fun elt acc -> elt :: acc) t List.empty

  /// Convert a Tree to a Sequence
  let public toSeq t =
    fold (fun acc elt -> Seq.singleton elt |> Seq.append acc) Seq.empty t

  /// Convert a Tree to a Set
  let public toSet t = fold (fun acc elt -> Set.add elt acc) Set.empty t

  /// Apply a function to all values in a Tree
  let public map func t =
    fold (fun acc elt -> insert (func elt) acc) empty t

  /// Combine two Trees, similar to set union.  Inserts the values from t2 into t1.
  let public union t1 t2 = fold (fun acc elt -> insert elt acc) t1 t2

  /// Keep values that appear in both Trees, similar to set intersection.
  let public intersect t1 t2 = filter (fun elt -> exists elt t2) t1

  /// Keep an element when it does not appear in the second Tree (t2).
  let difference t1 t2 = fold (fun acc elt -> remove elt acc) t1 t2
