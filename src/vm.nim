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

method isThruthy*(x: Value): bool {.base, inline.} = false
method isThruthy*(x: Int): bool {.inline.} = x.val != 0
method isThruthy*(x: Float): bool {.inline.} = x.val != 0
method isThruthy*(x: String): bool {.inline.} = len(x.val) > 0
method isThruthy*(x: Bool): bool {.inline.} = x.val
method isThruthy*(x: List): bool {.inline.} = x.val.head != nil
method isThruthy*(x: Set): bool {.inline.} = x.val > 0
method isThruthy*(x: Char): bool {.inline.} = ord(x.val) > 0

method null*(x: Value): bool {.base.} = false
method null*(x: Bool): bool = x.val == false
method null*(x: Char): bool = x.val == char(0)
method null*(x: Int): bool = x.val == 0
method null*(x: String): bool = len(x.val) == 0
method null*(x: Set): bool = x.val == 0
method null*(x: List): bool = x.val.head == nil
method null*(x: Ident): bool = false

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
literalStr(Ident)

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

method `div`*(a: Value, b: Value): (Value, Value) {.base, inline.} =
  raiseRuntimeError("badarg for `div` " & repr(a))
method `div`*(a: Int, b: Int): (Value, Value) {.inline.} =
  let q = a.val div b.val
  let rem = a.val mod b.val
  (newInt(q), newInt(rem))

method sign*(a: Value): Value {.base, inline.} =
  raiseRuntimeError("badarg for `sign` " & repr(a))
method sign*(a: Int): Value {.inline.} =
  newInt(sgn[int](a.val))
method sign*(a: Float): Value {.inline.} =
  newfloat(sgn[float](a.val).float)

method neg*(a: Value): Value {.base, inline.} =
  raiseRuntimeError("badarg for `neg` " & repr(a))
method neg*(a: Int): Value {.inline.} =
  newInt(-a.val)
method neg*(a: Float): Value {.inline.} =
  newFloat(-a.val)

method abs*(a: Value): Value {.base, inline.} =
  raiseRuntimeError("badarg for `abs` " & repr(a))
method abs*(a: Int): Value {.inline.} =
  newInt(abs(a.val))
method abs*(a: Float): Value {.inline.} =
  newFloat(abs(a.val))

method ord*(a: Value): Int {.base, inline.} =
  raiseRuntimeError("badarg for `ord` " & repr(a))
method ord*(a: Char): Int {.inline.} =
  newInt(ord(a.val))
method ord*(a: Int): Int {.inline.} =
  newInt(ord(a.val))
method ord*(a: Bool): Int {.inline.} =
  newInt(ord(a.val))

method chr*(a: Value): Value {.base, inline.} =
  raiseRuntimeError("badarg for `chr` " & repr(a))
method chr*(a: Char): Value {.inline.} =
  newChar(chr(ord(a.val)))
method chr*(a: Int): Value {.inline.} =
  newChar(chr(a.val))
method chr*(a: Bool): Value {.inline.} =
  newChar(chr(ord(a.val)))

method pred*(a: Value): Value {.base, inline.} =
  raiseRuntimeError("badarg for `pred` " & repr(a))
method pred*(a: Bool): Value {.inline.} =
  newBool(pred(a.val))
method pred*(a: Char): Value {.inline.} =
  newChar(pred(a.val))
method pred*(a: Int): Value {.inline.} =
  newInt(pred(a.val))

method succ*(a: Value): Value {.base, inline.} =
  raiseRuntimeError("badarg for `succ` " & repr(a))
method succ*(a: Bool): Value {.inline.} =
  newBool(succ(a.val))
method succ*(a: Char): Value {.inline.} =
  newChar(succ(a.val))
method succ*(a: Int): Value {.inline.} =
  newInt(succ(a.val))

method max*(a: Value, b: Value): Value {.base, inline.} =
  raiseRuntimeError("badarg for `max` " & repr(a))
method max*(a: Int, b: Int): Value {.inline.} =
  newInt(max(a.val, b.val))
method max*(a: Float, b: Int): Value {.inline.} =
  newFloat(max(a.val, b.val.float))
method max*(a: Int, b: Float): Value {.inline.} =
  newFloat(max(a.val.float, b.val))
method max*(a: Float, b: Float): Value {.inline.} =
  newFloat(max(a.val, b.val))

method min*(a: Value, b: Value): Value {.base, inline.} =
  raiseRuntimeError("badarg for `min` " & repr(a))
method min*(a: Int, b: Int): Value {.inline.} =
  newInt(min(a.val, b.val))
method min*(a: Float, b: Int): Value {.inline.} =
  newFloat(min(a.val, b.val.float))
method min*(a: Int, b: Float): Value {.inline.} =
  newFloat(min(a.val.float, b.val))
method min*(a: Float, b: Float): Value {.inline.} =
  newFloat(min(a.val, b.val))

biLogicOp("and", `and`)
biLogicOp("or", `or`)
biLogicOp("xor", `xor`)

proc `not`*(x: Value): Value = newBool(not isThruthy(x))

method size*(x: Value): Value {.base.} =
  raiseRuntimeError("badarg for `size` " & repr(x))

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
  if len(a.val) == 0:
    raiseRuntimeError("empty string is invalid for `first`")
  newChar(a.val[0])

method first*(a: Set): Value =
  for i in 0..<maxSetSize:
    if (a.val and (1 shl i)) > 0:
      return newInt(i)
  raiseRuntimeError("empty set is invalid for `first`")

method first*(a: List): Value =
  if a.val.head == nil:
    raiseRuntimeError("empty list is invalid for `first`")
  a.val.head.value

method rest*(a: Value): Value {.base.} =
  raiseRuntimeError("badarg for `rest` " & repr(a))

method rest*(a: String): Value =
  if len(a.val) == 0:
    return newString("")
  newString(a.val.substr(1))

method rest*(a: List): Value =
  var list = initSinglyLinkedList[Value]()
  if a.val.head == nil:
    return newList()
  list.head = a.val.head.next
  newList(list)

method rest*(a: Set): Value =
  if a.val == 0:
    return newSet(0)
  let first = cast[Int](first(a))
  let val = a.val and not (1 shl first.val)
  newSet(val)

method uncons*(a: Value): (Value, Value) {.base.} =
  raiseRuntimeError("badarg for `uncons` " & repr(a))

method uncons*(a: List): (Value, Value) =
  (a.first, a.rest)

method at*(a: Value, i: int): Value {.base.} =
  raiseRuntimeError("badarg for `at` " & repr(a))

method at*(a: List, i: int): Value =
  var p = 0
  var next = a.val.head
  while p < i: 
    next = next.next
    if next == nil:
      raiseRuntimeError("index out of range")
    inc(p)
  next.value