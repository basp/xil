import lists

# const maxSetSize = 32

type
  Value* = ref object of RootObj
  Bool* = ref object of Value
    val*: bool
  Char* = ref object of Value
    val*: char
  Int* = ref object of Value
    val*: int
  Float* = ref object of Value
    val*: float
  String* = ref object of Value
    val*: string
  Set* = ref object of Value
    val*: int
  List* = ref object of Value
    val*: SinglyLinkedList[Value]
  Ident* = ref object of Value
    val*: string
  Usr* = ref object of Value
    id*: Ident
    term*: List
  RuntimeException* = object of Exception

proc raiseRuntimeError*(msg: string) =
  raise newException(RuntimeException, msg)

proc newBool*(val: bool): Bool =
  Bool(val: val)

proc newChar*(val: char): Char =
  Char(val: val)

proc newInt*(val: int): Int =
  Int(val: val)

proc newFloat*(val: float): Float =
  Float(val: val)

proc newString*(val: string): String =
  String(val: val)

proc newSet*(val: int): Set =
  Set(val: val)

proc newList*(): List =
  List(val: initSinglyLinkedList[Value]())

proc newList*(xs: seq[Value]): List =
  result = newList()
  for x in xs: result.val.append(x)

proc newList*(xs: SomeLinkedList[Value]): List =
  List(val: xs)

proc newIdent*(val: string): Ident =
  Ident(val: val)

proc newUsr*(id: Ident, term: seq[Value]): Usr =
  Usr(id: id, term: newList(term))

proc newUsr*(id: Ident, term: List): Usr =
  Usr(id: id, term: term)

template literalEq(t: untyped) =
  method `==`*(a, b: t): bool = a.val == b.val

method `==`*(a, b: Value): bool {.base.} = false

literalEq(Bool)
literalEq(Char)
literalEq(Int)
literalEq(Float)
literalEq(String)
literalEq(Set)
literalEq(Ident)

method `==`*(a, b: List): bool =
  var x = a.val.head
  var y = b.val.head
  while true:
    if x == nil and y == nil:
      return true
    if x == nil:
      return false
    if y == nil:
      return false
    if x.value != y.value:
      return false
    x = x.next
    y = y.next
  true

when isMainModule:
  var a = newList(@[
    cast[Value](newInt(1)),
    cast[Value](newInt(2))])
  var b = newList(@[
    cast[Value](newInt(1)),
    cast[Value](newInt(2))])
  var c = newList(@[
    cast[Value](newInt(1)),
    cast[Value](newInt(2)),
    cast[Value](newInt(3))])
  var d = newList(@[
    cast[Value](newInt(2)),
    cast[Value](newInt(1))])
  assert a == b
  assert b == a
  assert a != c
  assert b != c
  assert a != d
  assert d != a
  assert d == d
  assert d != b
  assert b != d