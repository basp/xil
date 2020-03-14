import math, hashes, lists, strutils, sequtils

const maxSetSize = 32

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

template literalClone*(t: untyped, ctor: untyped) =
  method clone*(x: t): Value =
    ctor(x.val)

method clone*(self: Value): Value {.base.} = self

literalClone(Bool, newBool)
literalClone(Char, newChar)
literalClone(Int, newInt)
literalClone(Float, newFloat)
literalClone(String, newString)
literalClone(Set, newSet)
literalClone(Ident, newIdent)

method clone*(self: List): Value =
  var xs = initSinglyLinkedList[Value]()
  for x in self.val:
    xs.append(x.clone())
  newList(xs)

proc add*(a: Set, x: int) =
  a.val = a.val or (1 shl x)

proc add*(a: Set, x: Int) =
  a.add(x.val)

proc add*(a: List, x: Value) =
  a.val.append(x)

proc delete*(a: Set, x: int) =
  a.val = a.val and not (1 shl x)

proc contains*(a: Set, x: int): bool =
  (a.val and (1 shl x)) > 0

proc contains*(a: Set, x: Int): bool =
  a.contains(x.val)

proc contains*(a: List, x: Value): bool =
  a.val.contains(x)

iterator items*(a: String): char =
  for c in a.val:
    yield c

iterator items*(a: Set): int =
  for x in 0..<maxSetSize:
    if a.contains(x):
      yield x

iterator items*(a: List): Value =
  var node = a.val.head
  while node != nil:
    yield node.value
    node = node.next

method `$`*(a: Value): string {.base} = repr(a)

template literalStr(t: untyped) =
  method `$`*(a: t): string = $a.val

literalStr(Bool)
literalStr(Int)
literalStr(Float)
literalStr(List)

method `$`*(a: Char): string = repr(a.val)
method `$`*(a: String): string = escape(a.val)
method `$`*(a: Set): string = "{" & join(toSeq(items(a)), " ") & "}"

template unFloatOp(name: string, op: untyped, fn: untyped) =
  method op*(a: Value): Value {.base.} =
    raiseRuntimeError("badarg for `" & name & "`")
  
  method op*(a: Int): Value  =
    newFloat(fn(a.val.float))
  
  method op*(a: Float): Value =
    newFloat(fn(a.val))

template biFloatOp(name: string, op: untyped, fn: untyped, ctor: untyped) =
  method op*(a: Value, b: Value): Value {.base.} =
    raiseRuntimeError("badargs for `" & name & "`")
  
  method op*(a: Int, b: Int): Value =
    ctor(fn(a.val, b.val))
  
  method op*(a: Int, b: Float): Value  =
    newFloat(fn(a.val.float, b.val))
  
  method op*(a: Float, b: Int): Value =
    newFloat(fn(a.val, b.val.float))
  
  method op*(a: Float, b: Float): Value =
    newFloat(fn(a.val, b.val))

template biLogicOp(name: string, op: untyped) =
  method op*(a: Value, b: Value): Value {.base.} =
    raiseRuntimeError("badargs for `" & name & "`")
  
  method op*(a: Bool, b: Bool): Value =
    newBool(op(a.val, b.val))
  
  method op*(a: Set, b: Set): Value =
    newSet(op(a.val, b.val))

unFloatOp("acos", acos, arcsin)
unFloatOp("asin", asin, arcsin)
unFloatOp("atan", atan, arctan)
unFloatOp("cos", cos, cos)
unFloatOp("sin", sin, sin)
unFloatOp("tan", tan, tan)
unFloatOp("cosh", cosh, cosh)
unFloatOp("sinh", sinh, sinh)
unFloatOp("tanh", tanh, tanh)
unFloatOp("exp", exp, exp)
unFloatOp("sqrt", sqrt, sqrt)

biFloatOp("+", `+`, `+`): newInt
biFloatOp("-", `-`, `-`): newInt
biFloatOp("*", `*`, `*`): newInt
biFloatOp("/", `/`, `/`): newFloat
biFloatOp("rem", `rem`, `mod`): newInt

biLogicOp("and", `and`)
biLogicOp("or", `or`)
biLogicOp("xor", `xor`)

method size*(x: Value): Value {.base.} =
  raiseRuntimeError("badargs for `size`")

method size*(x: List): Value =
  var count = 0
  var next = x.val.head
  while next != nil:
    inc(count)
    next = next.next
  newInt(count)

method size*(x: Set): Value =
  # Brian Kernighan’s algorithm for
  # counting the number of set bits
  var n = x.val
  var count = 0
  while n > 0:
    n = n and (n - 1)
    inc(count)
  newInt(count)
method size*(x: String): Value =
  newInt(len(x.val))

method cons*(x: Value, a: Value): Value {.base.} =
  raiseRuntimeError("badargs for `cons`")

method cons*(x: Value, a: List): Value =
  var b = newList(a.val)
  b.val.prepend(x)
  return b

method cons*(x: Char, a: String): Value =
  var b = newString(a.val)
  b.val.insert($x.val)
  return b

method cons*(x: Int, a: Set): Value =
  var b = newSet(a.val)
  b.add(x)
  return b

method first*(a: Value): Value {.base.} = 
  raiseRuntimeError("badarg for `first`")

method first*(a: String): Value = 
  newChar(a.val[0])

method first*(a: Set): Value =
  newInt(toSeq(items(a))[0])

method first*(a: List): Value =
  toSeq(items(a))[0]

method rest*(a: Value): Value {.base.} =
  raiseRuntimeError("badarg for `rest`")

method rest*(a: List): Value =
  var list = initSinglyLinkedList[Value]()
  list.head = a.val.head.next
  newList(list)

method rest*(a: Set): Value =
  let first = toSeq(items(a))[0]
  a.delete(first)
  return a