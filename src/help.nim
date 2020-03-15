import tables, opnames, macros

type
  Help = object
    effect*: string
    info*: seq[string]

var helptable* = initTable[string,Help]()

macro help(arg: untyped): untyped =
  arg.expectKind nnkInfix
  let key = arg[1]
  let eff = arg[2]
  quote do:
    helptable.add($`key`, Help(effect: $`eff`))

help(ID := "->")
help(DUP := "X  ->  X X")
help(SWAP :=  "X Y  ->  Y X")
help(PUT := "X  ->")