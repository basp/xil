import tables, opnames, macros

type
  Help = object
    effect*: string
    info*: seq[string]

var helptable* = initTable[string,Help]()

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

h(PUT => "X  ->", @[
  "Writes X to output, pops X off stack."
])