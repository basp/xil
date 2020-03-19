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

h(ADD => "X  ->", @[
])

h(SUB => "X  ->", @[
])

h(MUL => "X  ->", @[
])

h(DIVIDE => "X  ->", @[
])
