import tables, opnames, macros

type
  Help = object
    effect*: string
    info*: seq[string]

var helptable* = initTable[string, Help]()

macro h(arg: untyped, info: seq[string]): untyped =
  arg.expectKind nnkInfix
  let key = arg[1]
  let eff = arg[2]
  quote do:
    helptable.add($`key`, Help(effect: $`eff`, info: `info`))

h(NEWSTACK => "..  ->", @[
])

h(STACK => ".. X Y Z  ->  .. X Y Z [Z Y X ..]", @[
  "Pushes the stack as a list."
])

h(UNSTACK => "[X Y ..]  ->  ..Y X", @[
  "The list [X Y ..] becomes the new stack."
])

h(ID := "->", @[
  "Identity function, does nothing.",
  "Any program of the form  P id Q  is equivalent to just  P Q."
])

h(DUP => "X  ->  X X", @[
  "Pushes an extra copy of X onto stack."
])

h(SWAP => "X Y  ->  Y X", @[
  "Interchanges X and Y on top of the stack."
])

h(ROLLUP => "X Y Z  ->  Z X Y", @[
  "Moves X and Y up, moves Z down."
])

h(ROLLDOWN => "X Y Z  ->  Y Z X", @[
  "Moves Y and Z down, moves X up."
])

h(ROTATE => "X Y Z  ->  Z Y X", @[
  "Interchanges X and Z."
])

h(POPD => "Y Z  ->  Z", @[
  "As if defined by:   popd  ==  [pop] dip"
])

h(DUPD => "Y Z  ->  Y Y Z", @[
  "As if defined by:   dupd  ==  [dup] dip"
])

h(SWAPD => "X Y Z  ->  Y X Z", @[
  "As if defined by:   swapd  ==  [swap] dip"
])

h(ROLLUPD => "X Y Z W  ->  Z X Y W", @[
  "As if defined by:   rollupd  ==  [rollup] dip"
])

h(ROLLDOWND => "X Y Z W  ->  Y Z X W", @[
  "As if defined by:   rolldownd  ==  [rolldown] dip"
])

h(ROTATED => "X Y Z W  ->  Z Y X W", @[
  "As if defined by:   rotated  ==  [rotate] dip"
])

h(POP => "X  ->", @[
  "Removes X from top of the stack."
])

h(CHOICE => "B T F  ->  X", @[
  "If B is true, then X = T else X = F."
])

h(OR => "X Y  ->  Z", @[
  "Z is the union of sets X and Y,",
  "logical disjunction for truth values."
])

h(XOR => "X Y  ->  Z", @[
  "Z is the symmetric difference of sets X and Y,",
  "logical exclusive disjunction for truth values."
])

h(AND => "X Y  ->  Z", @[
  "Z is the intersection of sets X and Y,",
  "logical conjunction for truth values."
])

h(NOT => "X  ->  Y", @[
  "Y is the complement of set X,",
  "logical negation for truth values."
])

h(ADD => "M I  ->  N", @[
  "Numeric N is the result of adding integer I to numeric M.",
  "Also supports float."
])

h(SUB => "M I  ->  N", @[
  "Numeric N is the result of subtracting integer I from numeric M.",
  "Also supports float."
])

h(MUL => "I J  ->  K", @[
  "Integer K is the product of integers I and J.",
  "Also supports float."
])

h(DIVIDE => "I J  ->  K", @[
  "Integer K is the (rounded) ratio of integers I and J.",
  "Also supports float."
])

h(REM => "I J  ->  K", @[
  "Integer K is the remainder of dividing I by J.",
  "Also supports float."
])

h(DIV => "I J  ->  K L", @[
  "Integers K and L are the quotient and remainder of dividing I by J."
])

h(SIGN => "N1  ->  N2", @[
  "Integer N2 is the sign (-1 or 0 or +1) of integer N1,",
  "or float N2 is the sign (-1.0 or 0.0 or 1.0) of float N1."
])

h(NEG => "I  ->  J", @[
  "Integer J is the negative of integer I.",
  "Also supports float."
])

h(ORD => "C  ->  I", @[
  "Integer I is the Ascii value of character C (or logical or integer)."
])

h(CHR => "I  ->  C", @[
  "C is the character whose Ascii value is integer I (or logical or character)."
])

h(ABS => "N1  ->  N2", @[
  "Integer N2 is the absolute value (0,1,2..) of integer N1,",
  "or float N2 is the absolute value (0.0 ..) of float N1"
])

h(ACOS => "F  ->  G", @[
  "G is the arc cosine of F."
])

h(ASIN => "F  ->  G", @[
  "G is the arc sine of F."
])

h(ATAN => "F  ->  G", @[
  "G is the arc tangent of F."
])

h(CEIL => "F  ->  G", @[
  "G is the float ceiling of F."
])

h(COS => "F  ->  G", @[
  "G is the cosine of F."
])

h(COSH => "F  ->  G", @[
  "G is the hyperbolic cosine of F."
])

h(EXP => "F  ->  G", @[
  "G is e (2.718281828...) raised to the Fth power."
])

h(FLOOR => "F  ->  G", @[
  "G is the floor of F."
])

h(SIN => "F  ->  G", @[
  "G is the sine of F."
])

h(SINH => "F  ->  G", @[
  "G is the hyperbolic sine of F."
])

h(SQRT => "F  ->  G", @[
  "G is the square root of F."
])

h(TAN => "F  ->  G", @[
  "G is the tangent of F."
])

h(TANH => "F  ->  G", @[
  "G is the hyperbolic tangent of F."
])

h(TRUNC => "F  ->  I", @[
  "I is an integer equal to the float F truncated toward zero."
])

h(PRED => "M  ->  N", @[
  "Numeric N is the predecessor of numeric M."
])

h(SUCC => "M  ->  N", @[
  "Numeric N is the successor of numeric M."
])

h(MAX => "N1 N2  ->  N", @[
  "N is the maximum of numeric values N1 and N2.",
  "Also supports float."
])

h(MIN => "N1 N2  ->  N", @[
  "N is the minimum of numeric values N1 and N2.",
  "Also supports float."
])

h(CONS => "X A  ->  B", @[
  "Aggregate B is A with a new member X (first member for sequences)."
])

h(SWONS => "A X  ->  B", @[
  "Aggregate B is A with a new member X (first member for sequences)."
])

h(FIRST => "A  ->  F", @[
  "F is the first member of the non-empty aggregate A."
])

h(REST => "A  ->  R", @[
  "R is the non-empty aggregate A with its first member removed."
])

h(AT => "A I  ->  X", @[
  "X (= A[I]) is the member of A at position I."
])

h(OF => "I A  ->  X", @[
  "X (= A[I]) is the I-th member of aggregate A."
])

h(SIZE => "A  ->  I", @[
  "Integer I is the number of elements of aggregate A."
])

h(OPCASE => "X [..[X Xs]..]  ->  [Xs]", @[
  "Indexing on type of X, returns the list [Xs]."
])

h(CASE => "X [..[X Y]..]  ->  Y i", @[
  "Indexing on the value of X, execute the matching Y."
])

h(UNCONS => "A  ->  F R", @[
  "F and R are the first and the rest of non-empty aggregate A."
])

h(UNSWONS => "A  ->  R F", @[
  "R and F are the rest and the first of non-empty aggregate A."
])

h(DROP => "A N  ->  B", @[
  "Aggregate B is the result of deleting the first N elements of A."
])

h(TAKE => "A N  ->  B", @[
  "Aggregate B is the result of retaining just the first N elements of A."
])

h(CONCAT => "S T  ->  U", @[
  "Sequence U is the concatenation of sequences S and T."
])

h(ENCONCAT => "X S T  ->  U", @[
  "Sequence U is the concatenation of sequences S and T",
  "with X inserted between S and T (== swapd cons concat)"
])

h(NAME => "sym  ->  \"sym\"", @[
  "For operators and combinators, the string \"sym\" is the name of item sym,",
  "for literals sym the result string is its type."
])

h(INTERN => "\"sym\"  -> sym", @[
  "Pushes the item whose name is \"sym\"."
])

h(BODY => "U  ->  [P]", @[
  "Quotation [P] is the body of user-defined symbol U."
])

h(NULL => "X  ->  B", @[
  "Tests for empty aggregate X or zero numeric."
])

h(SMALL => "X  ->  B", @[
  "Tests whether aggregate X has 0 or 1 members, or numeric 0 or 1."
])

h(GTE => "X Y  ->  B", @[
  "Tests whether X greater than or equal to Y.",
])

h(GT => "X Y  ->  B", @[
  "Tests whether X greater than Y.",
])

h(LTE => "X Y  ->  B", @[
  "Tests whether X less than or equal to Y.",
])

h(LT => "X Y  ->  B", @[
  "Tests whether X less than Y.",
])

h(NEQ => "X Y  ->  B", @[
  "Tests whether X not equal to Y.",
])

h(EQ => "X Y  ->  B", @[
  "Tests whether X equal to Y."
])

h(CMP => "X Y  ->  I", @[
  "I (=-1,0,+1) is the comparison of X and Y.",
  "The values correspond to the predicates <=, =, >=."
])

h(I => "[P]  ->  ...", @[
  "Executes P. So, [P] i  ==  P."
])

h(X => "[P] I  ->  ...", @[
  "Executes P without popping [P]. So, [P] x  ==  [P] P."
])

h(DIP => "X [P]  ->  ... X", @[
  "Saves X, executes P, pushes X back."
])

h(QUIT => "->", @[
  "Exit from Xil."
])
