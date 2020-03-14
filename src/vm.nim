import hashes, lists

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

when isMainModule:
  var a = cast[Value](newList(@[
    cast[Value](newInt(1)),
    cast[Value](newInt(2))]))
  var b = cast[Value](newList(@[
    cast[Value](newInt(1)),
    cast[Value](newInt(2))]))
  var c = cast[Value](newList(@[
    cast[Value](newInt(1)),
    cast[Value](newInt(2)),
    cast[Value](newInt(3))]))
  var d = cast[Value](newList(@[
    cast[Value](newInt(2)),
    cast[Value](newInt(1))]))
  assert a == b
  assert b == a
  assert a != c
  assert b != c
  assert a != d
  assert d != a
  assert d == d
  assert d != b
  assert b != d

template literalHash(t: untyped) =
  method hash*(a: t): Hash = hash(a.val)

method hash*(a: Value): Hash {.base.} =
  raiseRuntimeError("badarg for `hash`")

literalHash(Bool)
literalHash(Char)
literalHash(Int)
literalHash(Float)
literalHash(Set)
literalHash(String)
literalHash(Ident)

when isMainModule:
  a = cast[Value](newBool(true))
  b = cast[Value](newBool(false))
  assert hash(a) == hash(true)
  assert hash(b) == hash(false)
  a = cast[Value](newIdent("foo"))
  b = cast[Value](newIdent("bar"))
  assert hash(a) == hash("foo")
  assert hash(b) == hash("bar")

method cmp*(a: Value, b: Value): Int {.base.} =
  raiseRuntimeError("TILT cmp")
method cmp*(a: Int, b: Int): Int =
  newInt(cmp(a.val, b.val))
method cmp*(a: Int, b: Float): Int =
  newInt(cmp(a.val.float, b.val))
method cmp*(a: Float, b: Int): Int =
  newInt(cmp(a.val, b.val.float))
method cmp*(a: Bool, b: Bool): Int =
  newInt(cmp(a.val, b.val))
method cmp*(a: Bool, b: Int): Int =
  newInt(cmp(ord(a.val), b.val))
method cmp*(a: Int, b: Bool): Int =
  newInt(cmp(a.val, ord(b.val)))
method cmp*(a: Bool, b: Char): Int =
  newInt(cmp(ord(a.val), ord(b.val)))
method cmp*(a: Char, b: Bool): Int =
  newInt(cmp(ord(a.val), ord(b.val)))
method cmp*(a: Char, b: Char): Int =
  newInt(cmp(a.val, b.val))
method cmp*(a: Char, b: Int): Int =
  newInt(cmp(ord(a.val), b.val))
method cmp*(a: Int, b: Char): Int =
  newInt(cmp(a.val, ord(b.val)))
method cmp*(a: Set, b: Set): Int =
  newInt(cmp(a.val, b.val))
method cmp*(a: String, b: String): Int =
  newInt(cmp(a.val, b.val))
method cmp*(a, b: List): Int =
  var x = a.val.head
  var y = b.val.head
  while x != nil or y != nil:
    if x == nil:
      return newInt(-1)
    if y == nil:
      return newInt(1)
    let z = cmp(x.value, y.value)
    if z.val != 0:
      return z
    x = x.next
    y = y.next
  return newInt(0)

when isMainModule:
  a = cast[Value](newList(@[
    cast[Value](newInt(1)),
    cast[Value](newInt(2))]))
  b = cast[Value](newList(@[
    cast[Value](newInt(1)),
    cast[Value](newInt(2))]))
  c = cast[Value](newList(@[
    cast[Value](newInt(1)),
    cast[Value](newInt(2)),
    cast[Value](newInt(3))]))
  d = cast[Value](newList(@[
    cast[Value](newInt(2)),
    cast[Value](newInt(1))]))
  assert cmp(a, b) == newInt(0)
  assert cmp(b, a) == newInt(0)
  assert cmp(a, c) == newInt(-1)
  assert cmp(c, a) == newInt(1)
  assert cmp(a, a) == newInt(0)
  assert cmp(a, d) == newInt(-1)
  assert cmp(d, a) == newInt(1)
  