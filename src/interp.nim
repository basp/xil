import
  tables,
  lists,
  strformat,
  strutils,
  sequtils,
  sugar,
  algorithm,
  typetraits,
  opnames,
  vm,
  help,
  scan,
  parse

var
  stack* = initSinglyLinkedList[Value]()
  saved = initSinglyLinkedList[Value]()
  usrtable* = initTable[string, Usr]()

template saved1 = saved.head
template saved2 = saved.head.next
template saved3 = saved.head.next.next
template saved4 = saved.head.next.next.next
template saved5 = saved.head.next.next.next.next
template saved6 = saved.head.next.next.next.next.next

method eval*(x: Value) {.base,locks:0.}

proc execTerm(p: List) =
  for x in p.val:
    eval(x)

proc push(x: Value) {.inline.} =
  let node = newSinglyLinkedNode(x)
  stack.prepend(node)

proc pop(): Value {.inline.} =
  result = stack.head.value
  stack.head = stack.head.next

proc peek(): Value {.inline.} =
  stack.head.value

method numeric(x: Value): bool {.base,inline.} = false
method numeric(x: Int): bool {.inline.} = true
method numeric(x: Float): bool {.inline.} = true
method numeric(x: Char): bool {.inline.} = true

method integer(x: Value): bool {.base,inline.} = false
method integer(x: Int): bool {.inline.} = true

method logical(x: Value): bool {.base,inline.} = false
method logical(x: Bool): bool {.inline.} = true
method logical(x: Set): bool {.inline.} = true

method ordinal(x: Value): bool {.base,inline.} = false
method ordinal(x: Bool): bool {.inline.} = true
method ordinal(x: Int): bool {.inline.} = true
method ordinal(x: Char): bool {.inline.} = true

method aggregate(x: Value): bool {.base,inline.} = false
method aggregate(x: List): bool {.inline.} = true
method aggregate(x: Set): bool {.inline.} = true
method aggregate(x: String): bool {.inline.} = true

method list(x: Value): bool {.base,inline.} = false
method list(x: List): bool {.inline.} = true

proc raiseExecError(msg, name: string) =
  raiseRuntimeError(fmt"{msg} needed for `{name}`")

proc oneParameter(name: string) =
  const msg = "one parameter"
  if stack.head == nil:
    raiseExecError(msg, name)

proc twoParameters(name: string) =
  const msg = "two parameters"
  if stack.head == nil:
    raiseExecError(msg, name)
  if stack.head.next == nil:
    raiseExecError(msg, name)

proc threeParameters(name: string) =
  const msg = "three parameters"
  if stack.head == nil:
    raiseExecError(msg, name)
  if stack.head.next == nil:
    raiseExecError(msg, name)
  if stack.head.next.next == nil:
    raiseExecError(msg, name)

proc fourParameters(name: string) =
  const msg = "four parameters"
  if stack.head == nil:
    raiseExecError(msg, name)
  if stack.head.next == nil:
    raiseExecError(msg, name)
  if stack.head.next.next == nil:
    raiseExecError(msg, name)
  if stack.head.next.next.next == nil:
    raiseExecError(msg, name)

proc fiveParameters(name: string) =
  const msg = "five parameters"
  if stack.head == nil:
    raiseExecError(msg, name)
  if stack.head.next == nil:
    raiseExecError(msg, name)
  if stack.head.next.next == nil:
    raiseExecError(msg, name)
  if stack.head.next.next.next == nil:
    raiseExecError(msg, name)
  if stack.head.next.next.next.next == nil:
    raiseExecError(msg, name)

proc integerOnTop(name: string) =
  const msg = "integer on top"
  if not integer(stack.head.value):
    raiseExecError(msg, name)

proc integerAsSecond(name: string) =
  const msg = "integer as second parameter"
  if not integer(stack.head.next.value):
    raiseExecError(msg, name)

proc twoIntegers(name: string) =
  const msg = "two integers"
  if not integer(stack.head.value):
    raiseExecError(msg, name)
  if not integer(stack.head.next.value):
    raiseExecError(msg, name)

proc ordinalOnTop(name: string) =
  const msg = "ordinal on top"
  if not ordinal(stack.head.value):
    raiseExecError(msg, name)

proc listOnTop(name: auto) =
  const msg = "list on top"
  if not list(stack.head.value):
    raiseExecError(msg, name)

proc listAsSecond(name: auto) =
  const msg = "list as second parameter"
  if not list(stack.head.next.value):
    raiseExecError(msg, name)

proc quoteOnTop(name: string) =
  const msg = "quotation on top"
  if not list(stack.head.value):
    raiseExecError(msg, name)

proc oneQuote(name: auto) = quoteOnTop(name)

proc twoQuotes(name: auto) =
  const msg = "two quotes on top"
  if not list(stack.head.value):
    raiseExecError(msg, name)
  if not list(stack.head.next.value):
    raiseExecError(msg, name)

proc threeQuotes(name: auto) =
  const msg = "three quotes on top"
  if not list(stack.head.value):
    raiseExecError(msg, name)
  if not list(stack.head.next.value):
    raiseExecError(msg, name)
  if not list(stack.head.next.next.value):
    raiseExecError(msg, name)

proc fourQuotes(name: auto) =
  const msg = "four quotes on top"
  if not list(stack.head.value):
    raiseExecError(msg, name)
  if not list(stack.head.next.value):
    raiseExecError(msg, name)
  if not list(stack.head.next.next.value):
    raiseExecError(msg, name)
  if not list(stack.head.next.next.next.value):
    raiseExecError(msg, name)

proc aggregateOnTop(name: string) =
  const msg = "aggregate on top"
  if not aggregate(stack.head.value):
    raiseExecError(msg, name)

proc aggregateAsSecond(name: string) =
  const msg = "aggregate as second parameter"
  if not aggregate(stack.head.next.value):
    raiseExecError(msg, name)

proc twoAggregates(name: string) =
  const msg = "two aggregates"
  if not aggregate(stack.head.value):
    raiseExecError(msg, name)
  if not aggregate(stack.head.next.value):
    raiseExecError(msg, name)

proc nonEmptyAggregate(name: string) =
  const msg = "non-empty aggregate"
  if not aggregate(stack.head.value):
    raiseExecError(msg, name)
  if size(stack.head.value).val < 1:
    raiseExecError(msg, name)

proc numericOnTop(name: string) =
  const msg = "numeric on top"
  if not numeric(stack.head.value):
    raiseExecError(msg, name)

proc twoNumerics(name: string) =
  const msg = "two numerics"
  if not numeric(stack.head.value):
    raiseExecError(msg, name)
  if not numeric(stack.head.next.value):
    raiseExecError(msg, name)

proc logicalOnTop(name: string) =
  const msg = "logical on top"
  if not logical(stack.head.value):
    raiseExecError(msg, name)

proc twoLogicals(name: string) =
  const msg = "two logicals"
  if not logical(stack.head.value):
    raiseExecError(msg, name)
  if not logical(stack.head.next.value):
    raiseExecError(msg, name)

proc stringOnTop(name: string) =
  const msg = "string on top"
  if stack.head.value.kind != vkString:
    raiseExecError(msg, name)

proc identOnTop(name: string) =
  const msg = "symbol on top"
  if stack.head.value.kind != vkIdent:
    raiseExecError(msg, name)

proc sameTwoKinds(name: string) =
  const msg = "same two kinds"
  if stack.head.value.kind != stack.head.next.value.kind:
    raiseExecError(msg, name)

template unary(op: untyped, name: string) =
  let x = pop()
  push(op(x))

template binary(op: untyped, name: string) =
  let y = pop()
  let x = pop()
  push(op(x, y))

template unFloatOp(op: untyped, name: string) =
  oneParameter(name)
  numericOnTop(name)
  unary(op, name)

template biFloatOp(op: untyped, name: string) =
  twoParameters(name)
  twoNumerics(name)
  binary(op, name)

template biLogicalOp(op: untyped, name: string) =
  twoParameters(name)
  twoLogicals(name)
  binary(op, name)

proc opStack(name: auto) =
  push(newList(stack))

proc opNewstack(name: auto) =
  stack = initSinglyLinkedList[Value]()

proc opId() {.inline.} = discard

proc opDup(name: auto) {.inline.} =
  oneParameter(name)
  push(peek().clone)

proc opSwap(name: auto) {.inline.} =
  twoParameters(name)
  saved = stack
  stack.head = saved3
  push(saved1.value)
  push(saved2.value)

proc opRollup(name: auto) {.inline.} =
  threeParameters(name)
  saved = stack
  stack.head = saved4
  push(saved1.value)
  push(saved3.value)
  push(saved2.value)

proc opRolldown(name: auto) {.inline.} =
  threeParameters(name)
  saved = stack
  stack.head = saved4
  push(saved2.value)
  push(saved1.value)
  push(saved3.value)

proc opRotate(name: auto) {.inline.} =
  threeParameters(name)
  saved = stack
  stack.head = saved4
  push(saved1.value)
  push(saved2.value)
  push(saved3.value)

proc dipped(name: auto, p: proc()) =
  twoParameters(name)
  let x = pop()
  p()
  push(x)

proc opPopd(name: auto) {.inline.} =
  twoParameters(name)
  dipped(name, proc() = discard(pop()))

proc opDupd(name: auto) {.inline.} =
  twoParameters(name)
  dipped(name, proc() = opDup(name))

proc opSwapd(name: auto) {.inline.} =
  threeParameters(name)
  dipped(name, proc() = opSwap(name))

proc opRollupd(name: auto) {.inline.} =
  fourParameters(name)
  dipped(name, proc() = opRollup(name))

proc opRolldownd(name: auto) {.inline.} =
  fourParameters(name)
  dipped(name, proc() = opRolldown(name))

proc opRotated(name: auto) {.inline.} =
  fourParameters(name)
  dipped(name, proc() = opRotate(name))

proc opPop(name: auto): Value {.inline.} =
  oneParameter(name)
  pop()

proc opChoice(name: auto) {.inline.} =
  threeParameters(name)
  let f = pop()
  let t = pop()
  let b = pop()
  if isThruthy(b):
    push(t)
  else:
    push(f)

proc opCmp(name: auto) {.inline.} =
  let y = pop()
  let x = pop()
  push(cmp(x, y))

proc opOr(name: auto) {.inline.} = biLogicalOp(`or`, name)
proc opXor(name: auto) {.inline.} = biLogicalOp(`xor`, name)
proc opAnd(name: auto) {.inline.} = biLogicalOp(`and`, name)

proc opNot(name: auto) {.inline.} =
  oneParameter(name)
  logicalOnTop(name)
  unary(`not`, name)

proc opAdd(name: auto) {.inline.} = biFloatOp(`+`, name)
proc opSub(name: auto) {.inline.} = biFloatOp(`-`, name)
proc opMul(name: auto) {.inline.} = biFloatOp(`*`, name)
proc opDivide(name: auto) {.inline.} = biFloatOp(`/`, name)

proc opRem(name: auto) {.inline.} = biFloatOp(`rem`, name)

proc opDiv(name: auto) {.inline.} =
  twoParameters(name)
  twoIntegers(name)
  let j = cast[Int](pop())
  let i = cast[Int](pop())
  let (k, l) = `div`(i, j)
  push(k)
  push(l)

proc opSign(name: auto) {.inline.} = unFloatOp(sign, name)

proc opNeg(name: auto) {.inline.} =
  let x = pop()
  push(neg(x))

template unOrdinalOp(op: untyped, name: auto) =
  oneParameter(name)
  ordinalOnTop(name)
  let x = pop()
  push(op(x))

proc opOrd(name: auto) {.inline.} = unOrdinalOp(ord, name)
proc opChr(name: auto) {.inline.} = unOrdinalOp(chr, name)

proc opAbs(name: auto) {.inline.} = unfloatop(abs, name)

proc opMin(name: auto) {.inline.} = bifloatop(min, name)
proc opMax(name: auto) {.inline.} = bifloatop(max, name)

proc opSqrt(name: auto) {.inline.} = unfloatop(sqrt, name)
proc opSin(name: auto) {.inline.} = unfloatop(sin, name)
proc opCos(name: auto) {.inline.} = unfloatop(cos, name)
proc opTan(name: auto) {.inline.} = unfloatop(tan, name)
proc opAsin(name: auto) {.inline.} = unfloatop(asin, name)
proc opAcos(name: auto) {.inline.} = unfloatop(acos, name)
proc opAtan(name: auto) {.inline.} = unfloatop(atan, name)
proc opSinh(name: auto) {.inline.} = unfloatop(sinh, name)
proc opCosh(name: auto) {.inline.} = unfloatop(cosh, name)
proc opTanh(name: auto) {.inline.} = unfloatop(tanh, name)

proc opPred(name: auto) {.inline.} = unOrdinalOp(pred, name)
proc opSucc(name: auto) {.inline.} = unOrdinalOp(succ, name)

proc opPut(name: auto) {.inline.} =
  oneParameter(name)
  let x = pop()
  let ss = $len(stack)
  echo '='.repeat(len(ss)) & "> " & $x

proc opGet(name: auto) {.inline.} =
  stdout.write("< ")
  let src = stdin.readLine()
  let scanner = newScanner(src)
  let parser = newParser(scanner)
  let fac = parser.parseFactor()
  push(fac)

proc opPeek(name: string) {.inline.} =
  oneParameter(name)
  let x = peek()
  let ss = $len(stack)
  echo ':'.repeat(len(ss)) & "> " & $x

proc opCons(name: auto) {.inline.} =
  twoParameters(name)
  aggregateOnTop(name)
  binary(cons, name)

proc opSwons(name: auto) {.inline.} =
  opSwap(name)
  opCons(name)

proc opFirst(name: auto) {.inline.} =
  oneParameter(name)
  nonEmptyAggregate(name)
  let a = pop()
  push(first(a))

proc opRest(name: auto) {.inline.} =
  oneParameter(name)
  aggregateOnTop(name)
  let a = pop()
  push(rest(a))

template indexOp(name: auto) =
  let i = cast[Int](pop())
  let a = pop()
  push(at(a, i.val))

proc opAt(name: auto) {.inline.} =
  twoParameters(name)
  integerOnTop(name)
  aggregateAsSecond(name)
  indexOp(name)

proc opOf(name: auto) {.inline.} =
  twoParameters(name)
  aggregateOnTop(name)
  integerAsSecond(name)
  opSwap(name)
  indexOp(name)

proc opSize(name: auto) {.inline.} =
  oneParameter(name)
  aggregateOnTop(name)
  let a = pop()
  push(size(a))

proc checkNonEmptyList(x: Value, name: auto) =
  if x.kind != vkList:
    raiseExecError("non-empty internal list", name)
  let xs = cast[vm.List](x)
  if len(xs) < 1:
    raiseExecError("non-empty internal list", name)

# opcase      :  X [..[X Xs]..]  ->  [Xs]
# Indexing on type of X, returns the list [Xs].
proc opOpcase(name: auto) =
  twoParameters(name)
  listOnTop(name)
  let a = cast[vm.List](pop())
  let x = pop()
  var xs: vm.List
  for list in items(a):
    checkNonEmptyList(list, name)
    let f = first(list)
    xs = cast[vm.List](rest(list))
    if name(x) == name(f):
      push(xs)
      return
  push(xs)

# case      :  X [..[X Y]..]  ->  Y i
# Indexing on the value of X, execute the matching Y.
proc opCase(name: auto) =
  twoParameters(name)
  listOnTop(name)
  let a = cast[vm.List](pop())
  let x = pop()
  var y: vm.List
  for list in items(a):
    checkNonEmptyList(list, name)
    let f = first(list)
    y = cast[vm.List](rest(list))
    if x == f:
      execTerm(y)
      return
  execTerm(y)

proc popUncons(name: auto): (Value, Value) {.inline.} =
  oneParameter(name)
  aggregateOnTop(name)
  let a = pop()
  uncons(a)

proc opUncons(name: auto) {.inline.} =
  let (first, rest) = popUncons(name)
  push(first)
  push(rest)

proc opUnswons(name: auto) {.inline.} =
  let (first, rest) = popUncons(name)
  push(rest)
  push(first)

proc opDrop(name: auto) =
  twoParameters(name)
  aggregateAsSecond(name)
  let n = cast[vm.Int](pop())
  let a = cast[vm.List](pop())
  push(drop(a, n.val))

proc opTake(name: auto) =
  twoParameters(name)
  aggregateAsSecond(name)
  let n = cast[vm.Int](pop())
  let a = cast[vm.List](pop())
  push(take(a, n.val))

proc opConcat(name: auto) {.inline.} =
  twoParameters(name)
  twoAggregates(name)
  sameTwoKinds(name)
  let b = pop()
  let a = pop()
  push(a.concat(b))

proc opEnconcat(name: auto) {.inline.} =
  threeParameters(name)
  sameTwoKinds(name)
  opSwapd(name)
  opCons(name)
  opConcat(name)

proc opReverse(name: auto) {.inline.} =
  oneParameter(name)
  aggregateOnTop(name)
  let a = pop()
  push(a.reverse())

proc name(x: Value): string =
  case x.kind
  of vkNone:
    "none"
  of vkBool:
    "bool"
  of vkChar:
    "char"
  of vkInt:
    "int"
  of vkFloat:
    "float"
  of vkString:
    "string"
  of vkSet:
    "set"
  of vkList:
    "list"
  of vkFile:
    "file"    
  of vkIdent:
    let id = cast[Ident](x)
    id.val
  of vkUsr:
    let usr = cast[Usr](x)
    usr.id.val

proc opName(name: auto) =
  let x = pop()
  push(newString(name(x)))

proc opIntern(name: auto) =
  oneParameter(name)
  stringOntop(name)
  let x = cast[vm.String](pop())
  push(newIdent(x.val))

proc opBody(name: auto) =
  oneParameter(name)
  identOnTop(name)
  let id = cast[Ident](pop())
  if not usrtable.hasKey(id.val):
    raiseRuntimeError(fmt"undefined symbol `{id.val}`")
  push(usrtable[id.val].term)
    
proc opNull(name: auto) {.inline.} =
  oneParameter(name)
  let x = pop()
  push(newBool(null(x)))

proc opSmall(name: auto) {.inline.} =
  oneParameter(name)
  let x = pop()
  push(small(x))

template cmpOp(op: untyped, name: auto) =
  twoParameters(name)
  let y = pop()
  let x = pop()
  push(newBool(op(cmp(x, y).val, 0)))

proc opLt(name: auto) {.inline.} = cmpOp(`<`, name)
proc opGt(name: auto) {.inline.} = cmpOp(`>`, name)
proc opLte(name: auto) {.inline.} = cmpOp(`<=`, name)
proc opGte(name: auto) {.inline.} = cmpOp(`>=`, name)

proc opEq(name: auto) {.inline.} =
  twoParameters(name)
  let y = pop()
  let x = pop()
  push(newBool(x == y))

proc opNeq(name: auto) {.inline.} =
  opEq(name)
  let x = pop()
  push(x.not)

proc opInteger(name: auto) {.inline.} =
  oneParameter(name)
  push(newBool(pop().kind == vkInt))

proc opFloat(name: auto) {.inline.} =
  oneParameter(name)
  push(newBool(pop().kind == vkFloat))

proc opChar(name: auto) {.inline.} =
  oneParameter(name)
  push(newBool(pop().kind == vkChar))

proc opLogical(name: auto) {.inline.} =
  oneParameter(name)
  push(newBool(logical(pop())))

proc opSet(name: auto) {.inline.} =
  oneParameter(name)
  push(newBool(pop().kind == vkSet))

proc opString(name: auto) {.inline.} =
  oneParameter(name)
  push(newBool(pop().kind == vkString))

proc opList(name: auto) {.inline.} =
  oneParameter(name)
  push(newBool(pop().kind == vkList))

proc opI(name: auto) {.inline.} =
  oneParameter(name)
  oneQuote(name)
  let p = cast[vm.List](pop())
  execTerm(p)

proc opX(name: auto) {.inline.} =
  oneParameter(name)
  oneQuote(name)
  let p = cast[vm.List](pop())
  execTerm(p)

proc opDip(name: auto) {.inline.} =
  twoParameters(name)
  oneQuote(name)
  let p = cast[vm.List](pop())
  let x = pop()
  execTerm(p)
  push(x)

proc opApp1(name: auto) {.inline.} =
  twoParameters(name)
  oneQuote(name)
  let p = cast[vm.List](pop())
  discard pop()
  execTerm(p)

template nary(paramcount: untyped, name: auto, top: untyped) =
  paramcount(name)
  oneQuote(name)
  saved = stack
  let p = cast[vm.List](pop())
  execTerm(p)
  let x = peek()
  stack.head = top
  stack.prepend(x)

proc opNullary(name: auto) {.inline.} = nary(oneParameter, name, saved2)
proc opUnary(name: auto) {.inline.} = nary(twoParameters, name, saved3)
proc opBinary(name: auto) {.inline.} = nary(threeParameters, name, saved4)
proc opTernary(name: auto) {.inline.} = nary(fourParameters, name, saved5)

proc opUnary2(name: auto) =
  threeParameters(name)
  oneQuote(name)
  saved = stack
  let p = cast[vm.List](pop())
  stack.head = saved2
  execTerm(p)
  let py = peek()
  stack.head = saved3
  execTerm(p)
  let px = peek()
  stack.head = saved4
  push(px)
  push(py)

proc opUnary3(name: auto) =
  fourParameters(name)
  oneQuote(name)
  saved = stack
  let p = cast[vm.List](pop())
  stack.head = saved2
  execTerm(p)
  let pz = peek()
  stack.head = saved3
  execTerm(p)
  let py = peek()
  stack.head = saved4
  execTerm(p)
  let px = peek()
  stack.head = saved5
  push(px)
  push(py)
  push(pz)

proc opUnary4(name: auto) =
  fiveParameters(name)
  oneQuote(name)
  saved = stack
  let p = cast[vm.List](pop())
  stack.head = saved2
  execTerm(p)
  let pw = peek()
  stack.head = saved3
  execTerm(p)
  let pz = peek()
  stack.head = saved4
  execTerm(p)
  let py = peek()
  stack.head = saved5
  execTerm(p)
  let px = peek()
  stack.head = saved6
  push(px)
  push(py)
  push(pz)
  push(pw)

proc opIfte(name: auto) =
  threeParameters(name)
  let f = cast[vm.List](pop())
  let t = cast[vm.List](pop())
  let b = cast[vm.List](pop())
  saved = stack
  execTerm(b)
  let p = peek()
  stack = saved
  if(isThruthy(p)):
    execTerm(t)
  else:
    execTerm(f)

proc opCond(name: auto) =
  raiseRuntimeError("not implemented")

# while      :  [B] [D]  ->  ...
# While executing B yields true executes D.
proc opWhile(name: string) =
  twoParameters(name)
  twoQuotes(name)
  let d = cast[List](pop())
  let b = cast[List](pop())
  saved = stack
  while true:
    stack = saved
    execTerm(b)
    if not isThruthy(peek()):
      break
    stack = saved
    execTerm(d)
    saved = stack
  stack = saved

proc opLinrec(name: auto) =
  fourParameters(name)
  fourQuotes(name)
  let
    r2 = cast[vm.List](pop())
    r1 = cast[vm.List](pop())
    t = cast[vm.List](pop())
    p = cast[vm.List](pop())
  proc linrecaux() =
    saved = stack
    execTerm(p)
    let res = pop()
    stack = saved
    if isThruthy(res):
      execTerm(t)
    else:
      execTerm(r1)
      linrecaux()
      execTerm(r2)
  linrecaux()

proc opTailrec(name: auto) =
  threeParameters(name)
  threeQuotes(name)
  let r1 = cast[vm.List](pop())
  let t = cast[vm.List](pop())
  let p = cast[vm.List](pop())
  proc tailrecaux() =
    saved = stack
    execTerm(p)
    let res = pop()
    stack = saved
    if isThruthy(res):
      execTerm(t)
    else:
      execTerm(r1)
      tailrecaux()
  tailrecaux()

# binrec      :  [B] [T] [R1] [R2]  ->  ...
# Executes P. If that yields true, executes T.
# Else uses R1 to produce two intermediates, recurses on both,
# then executes R2 to combines their results.
proc opBinrec(name: auto) =
  fourParameters(name)
  fourQuotes(name)
  let
    r2 = cast[vm.List](pop())
    r1 = cast[vm.List](pop())
    t = cast[vm.List](pop())
    b = cast[vm.List](pop())
  proc binrecaux() =
    saved = stack
    execTerm(b)
    let res = peek()
    stack = saved
    if isThruthy(res):
      execTerm(t)
    else:
      # The r1 proggrma leaves two items 
      # on the stack and we need to recurse
      # on both of them
      execTerm(r1)
      # So we first pop the first (top) value
      # and recurse on the second value first
      let v1 = pop()
      binrecaux()
      # Now we push back the first (top) value
      # and recurse on that as well
      push(v1)
      binrecaux()
      # Finally, we execute the r2 program
      # which combines the results
      execTerm(r2)
  binrecaux()

proc opGenrec(name: auto) =
  fourParameters(name)
  fourQuotes(name)
  let r2 = cast[vm.List](pop())
  let r1 = cast[vm.List](pop())
  let t = cast[vm.List](pop())
  let i = cast[vm.List](pop())
  saved = stack
  execTerm(i)
  let result = pop()
  stack = saved
  if isThruthy(result):
    execTerm(t)
  else:
    execTerm(r1)
    let q = newList()
    q.add(i)
    q.add(t)
    q.add(r1)
    q.add(r2)
    q.add(newIdent("genrec"))
    push(q)
    execTerm(r2)

proc opCondlinrec(name: auto) =
  raiseRuntimeError("not implemented")

proc opStep(name: auto) =
  twoParameters(name)
  oneQuote(name)
  listAsSecond(name)
  let p = cast[vm.List](pop())
  let a = cast[vm.List](pop())
  for x in items(a):
    push(x)
    execTerm(p)

proc foldString(v0: Value, p: List) =
  let a = cast[vm.String](pop())
  push(v0)
  for x in items(a):
    push(newChar(x))
    execTerm(p)  

proc foldSet(v0: Value, p: List) =
  let a = cast[vm.Set](pop())
  push(v0)
  for x in items(a):
    push(newInt(x))
    execTerm(p)

proc foldList(v0: Value, p: List) =
  let a = cast[vm.List](pop())
  push(v0)
  for x in items(a):
    push(x)
    execTerm(p)

# A V0 [P]  ->  V
proc opFold(name: auto) =
  threeParameters(name)
  oneQuote(name)
  let p = cast[vm.List](pop())
  let v0 = pop()
  if peek() of vm.String:
    foldString(v0, p)
  if peek() of vm.Set:
    foldSet(v0, p)
  if peek() of vm.List:
    foldList(v0, p)

proc opMap(name: auto) =
  twoParameters(name)
  oneQuote(name)
  listAsSecond(name)
  saved = stack
  let b = newList(@[])
  let p = cast[vm.List](pop())
  let a = cast[vm.List](pop())
  for x in a.val:
    push(x)
    execTerm(p)
    b.val.append(pop())
    stack = saved
  stack.head = saved3
  push(b)

proc opTimes(name: auto) =
  twoParameters(name)
  oneQuote(name)
  integerAsSecond(name)
  let p = cast[vm.List](pop())
  let n = cast[Int](pop())
  for i in 0..<n.val:
    execTerm(p)

proc opInfra(name: auto) =
  twoParameters(name)
  oneQuote(name)
  listAsSecond(name)
  let p = cast[vm.List](pop())
  let l1 = cast[vm.List](pop())
  saved = stack
  stack = l1.val
  execTerm(p)
  let l2 = pop()
  stack = saved
  push(l2)

proc opPrimrec(name: auto) =
  threeParameters(name)
  twoQuotes(name)
  let c = cast[vm.List](pop())
  let i = cast[vm.List](pop())
  let x = pop()
  var n = 0
  if x of vm.List:
    let list = cast[vm.List](x)
    for y in items(list):
      push(y)
      inc(n)
  elif x of vm.Set:
    let s = cast[vm.Set](x)
    for y in items(s):
      push(newInt(y))
      inc(n)    
  elif x of Int:
    let i = cast[Int](x)
    var j = i.val
    while j > 0:
      push(newInt(j))
      dec(j)
      inc(n)
  execTerm(i)
  for i in 0..<n:
    execTerm(c)

proc filterString() =
  var a1 = newString("")
  let p = cast[List](pop())
  let a = cast[String](pop())
  saved = stack
  for x in items(a):
    push(newChar(x))
    execTerm(p)
    if isThruthy(peek()):
      a1.val &= x
    stack = saved
  push(a1)

proc filterSet() =
  var a1 = newSet()
  let p = cast[List](pop())
  let a = cast[Set](pop())
  saved = stack
  for x in items(a):
    push(newInt(x))
    execTerm(p)
    if isThruthy(peek()):
      a1.add(x)
    stack = saved
  push(a1)

proc filterList() =
  var a1 = newList(@[])
  let p = cast[List](pop())
  let a = cast[List](pop())
  saved = stack
  for x in a.val:
    push(x)
    execTerm(p)
    if isThruthy(peek()):
      a1.val.append(x)
    stack = saved
  push(a1)

proc opFilter(name: auto) =
  twoParameters(name)
  oneQuote(name)
  aggregateAsSecond(name)
  if stack.head.next.value of vm.String:
    filterString()
    return
  if stack.head.next.value of vm.Set:
    filterSet()
    return
  if stack.head.next.value of vm.List:
    filterList()
    return
  else:
    raiseRuntimeError("unsupported aggregate for `" & name & "`")

proc splitString(b: List): (Value, Value) =
  let a1 = newString("")
  let a2 = newString("")
  let a = cast[String](pop())
  saved = stack
  for x in items(a):
    stack = saved
    push(newChar(x))
    execTerm(b)
    if isThruthy(pop()):
      a1.val &= x
    else:
      a2.val &= x
  stack = saved
  (a1, a2)

proc splitList(b: List): (Value, Value) =
  let a1 = newList()
  let a2 = newList()
  let a = cast[List](pop())
  saved = stack
  for x in items(a):
    stack = saved
    push(x)
    execTerm(b)
    if isThruthy(pop()):
      a1.add(x)
    else:
      a2.add(x)
  stack = saved
  (a1, a2)

proc splitSet(b: List): (Value, Value) =
  let a1 = newSet()
  let a2 = newSet()
  let a = cast[Set](pop())
  saved = stack
  for x in items(a):
    stack = saved
    push(newInt(x))
    execTerm(b)
    if isThruthy(pop()):
      a1.add(x)
    else:
      a2.add(x)
  stack = saved
  (a1, a2)

# split      :  A [B]  ->  A1 A2
proc opSplit(name: auto) =
  twoParameters(name)
  oneQuote(name)
  aggregateAsSecond(name)
  let b = cast[vm.List](pop())
  var a1, a2: Value
  let top = peek()
  if top of vm.List:
    (a1, a2) = splitList(b)
  elif top of vm.Set:
    (a1, a2) = splitSet(b)
  elif top of vm.String:
    (a1, a2) = splitString(b)
  else:
    raiseRuntimeError("tilt")
  push(a1)
  push(a2)

# treestep      :  T [P]  ->  ...
# Recursively traverses leaves of tree T, executes P for each leaf.
proc opTreestep(name: auto) =
  discard

# treerec      :  T [O] [C]  ->  ...
# T is a tree. If T is a leaf, executes O. 
# Else executes [[O] [C] treerec] C.
proc opTreerec(name: auto) =
  discard

proc opHelp(name: auto) =
  var len = 0
  for k in helptable.keys:
    if k.len > len:
      len = k.len
  var keys = toSeq(keys(helptable))
  keys.sort()
  for k in keys:
    let v = helptable[k]
    let pad = len - len(k)
    echo k & repeat(" ", pad) & "  :  " & v.effect
  
proc opHelpdetail(name: auto) =
  oneParameter(name)
  listOnTop(name)
  let a = cast[vm.List](pop())
  for x in items(a):
    let id = $x
    if helptable.hasKey(id):
      let effect = helptable[id].effect
      stdout.writeLine(fmt"{id}  :  {effect}")
      for line in helptable[id].info:
        stdout.writeLine(line)
      stdout.writeLine("")
    if usrtable.hasKey(id):
      let line = fmt"{id}  :  {usrtable[id].term}"
      stdout.writeLine(line)

proc opQuit(name: auto) = quit(0)

method eval*(x: Value) {.base.} = push(x)

method eval*(x: Usr) = usrtable[x.id.val] = x

method eval*(x: Ident) =
  if usrtable.hasKey(x.val):
    execTerm(usrtable[x.val].term)
    return
  case x.val
  of NEWSTACK: opNewstack(NEWSTACK)
  of STACK: opStack(STACK)
  of ID: opId()
  of DUP: opDup(DUP)
  of SWAP: opSwap(SWAP)
  of ROLLUP: opRollup(ROLLUP)
  of ROLLDOWN: opRolldown(ROLLDOWN)
  of ROTATE: opRotate(ROTATE)
  of POPD: opPopd(POPD)
  of DUPD: opDupd(DUPD)
  of SWAPD: opSwapd(SWAPD)
  of ROLLUPD: opRollupd(ROLLUPD)
  of ROLLDOWND: opRolldownd(ROLLDOWND)
  of ROTATED: opRotated(ROTATED)
  of POP: discard opPop(POP)
  of CHOICE: opChoice(CHOICE)
  of CMP: opCmp(CMP)
  of OR: opOr(OR)
  of XOR: opXor(XOR)
  of AND: opAnd(AND)
  of NOT: opNot(NOT)
  of ADD: opAdd(ADD)
  of SUB: opSub(SUB)
  of MUL: opMul(MUL)
  of DIVIDE: opDivide(DIVIDE)
  of REM: opRem(REM)
  of DIV: opDiv(DIV)
  of SIGN: opSign(SIGN)
  of NEG: opNeg(NEG)
  of ORD: opOrd(ORD)
  of CHR: opChr(CHR)
  of ABS: opAbs(ABS)
  of ACOS: opAcos(ACOS)
  of ASIN: opAsin(ASIN)
  of ATAN: opAtan(ATAN)
  of COS: opCos(COS)
  of COSH: opCosh(COSH)
  of SIN: opSin(SIN)
  of SINH: opSinh(SINH)
  of SQRT: opSqrt(SQRT)
  of TAN: opTan(TAN)
  of TANH: opTanh(TANH)
  of PRED: opPred(PRED)
  of SUCC: opSucc(SUCC)
  of MAX: opMax(MAX)
  of MIN: opMin(MIN)
  of PEEK: opPeek(PEEK)
  of PUT: opPut(PUT)
  of GET: opGet(GET)
  of CONS: opCons(CONS)
  of SWONS: opSwons(SWONS)
  of FIRST: opFirst(FIRST)
  of REST: opRest(REST)
  of AT: opAt(AT)
  of OF: opOf(OF)
  of SIZE: opSize(SIZE)
  of OPCASE: opOpcase(OPCASE)
  of CASE: opCase(CASE)
  of UNCONS: opUncons(UNCONS)
  of UNSWONS: opUnswons(UNSWONS)
  of DROP: opDrop(DROP)
  of TAKE: opTake(TAKE)
  of CONCAT: opConcat(CONCAT)
  of ENCONCAT: opEnconcat(ENCONCAT)
  of REVERSE: opReverse(REVERSE)
  of NAME: opName(NAME)
  of INTERN: opIntern(INTERN)
  of BODY: opBody(BODY)
  of NULL: opNull(NULL)
  of SMALL: opSmall(SMALL)
  of GT: opGt(GT)
  of LT: opLt(LT)
  of GTE: opGte(GTE)
  of LTE: opLte(LTE)
  of EQ: opEq(EQ)
  of NEQ: opNeq(NEQ)
  of opnames.INTEGER: opInteger(opnames.INTEGER)
  of opNames.FLOAT: opFloat(opnames.FLOAT)
  of opnames.CHAR: opChar(opnames.CHAR)
  of opnames.LOGICAL: opLogical(opnames.LOGICAL)
  of opnames.SET: opSet(opnames.SET)
  of opnames.STRING: opString(opnames.STRING)
  of opnames.LIST: opList(opnames.LIST)
  of I: opI(I)
  of X: opX(X)
  of DIP: opDip(DIP)
  of APP1: opApp1(APP1)
  of NULLARY: opNullary(NULLARY)
  of UNARY: opUnary(UNARY)
  of BINARY: opBinary(BINARY)
  of TERNARY: opTernary(TERNARY)
  of UNARY2: opUnary2(UNARY2)
  of UNARY3: opUnary3(UNARY3)
  of UNARY4: opUnary4(UNARY4)
  of IFTE: opIfte(IFTE)
  of COND: opCond(COND)
  of WHILE: opwhile(WHILE)
  of LINREC: opLinrec(LINREC)
  of TAILREC: opTailrec(TAILREC)
  of BINREC: opBinrec(BINREC)
  of GENREC: opGenrec(GENREC)
  of CONDLINREC: opCondlinrec(CONDLINREC)
  of STEP: opStep(STEP)
  of FOLD: opFold(FOLD)
  of MAP: opMap(MAP)
  of TIMES: opTimes(TIMES)
  of INFRA: opInfra(INFRA)
  of PRIMREC: opPrimrec(PRIMREC)
  of FILTER: opFilter(FILTER)
  of SPLIT: opSplit(SPLIT)
  of TREESTEP: opTreestep(TREESTEP)
  of TREEREC: opTreerec(TREEREC)
  of HELP: opHelp(HELP)
  of HELPDETAIL: opHelpdetail(HELPDETAIL)
  of QUIT: opQuit(QUIT)
  else:
    let msg = "undefined symbol `" & $x & "`"
    raiseRuntimeError(msg)