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

h(ADD => " M I  ->  N", @[
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
])

h(DIV => "I J  ->  K L", @[
])

h(SIGN => "N1  ->  N2", @[
])

h(NEG => "I  ->  J", @[
])

h(ORD => "C  ->  I", @[
])

h(CHR => "I  ->  C", @[
])

h(ABS => "N1  ->  N2", @[
])

h(ACOS => "F  ->  G", @[
])

h(ASIN => "F  ->  G", @[
])

h(ATAN => "F  ->  G", @[
])

h(CEIL => "F  ->  G", @[
])

h(COS => "F  ->  G", @[
])

h(COSH => "F  ->  G", @[
])

h(EXP => "F  ->  G", @[
])

h(FLOOR => "F  ->  G", @[
])

h(SIN => "F  ->  G", @[
])

h(SINH => "F  ->  G", @[
])

h(SQRT => "F  ->  G", @[
])

h(TAN => "F  ->  G", @[
])

h(TANH => "F  ->  G", @[
])

h(TRUNC => "F  ->  I", @[
])