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
  let ss = $len(stack)
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
  echo ':'.repeat(len(ss) + 1) & "> " & $x

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

proc opConcat(name: auto) {.inline.} =
  twoParameters(name)
  twoAggregates(name)
  sameTwoKinds(name)
  let b = pop()
  let a = pop()
  push(a.concat(b))

proc opReverse(name: auto) {.inline.} =
  oneParameter(name)
  aggregateOnTop(name)
  let a = pop()
  push(a.reverse())

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

proc opI(name: auto) {.inline.} =
  oneParameter(name)
  oneQuote(name)
  let p = cast[List](pop())
  execTerm(p)

proc opX(name: auto) {.inline.} =
  oneParameter(name)
  oneQuote(name)
  let p = cast[List](pop())
  execTerm(p)

proc opDip(name: auto) {.inline.} =
  twoParameters(name)
  oneQuote(name)
  let p = cast[List](pop())
  let x = pop()
  execTerm(p)
  push(x)

proc opApp1(name: auto) {.inline.} =
  twoParameters(name)
  oneQuote(name)
  let p = cast[List](pop())
  discard pop()
  execTerm(p)

template nary(paramcount: untyped, name: auto, top: untyped) =
  paramcount(name)
  oneQuote(name)
  saved = stack
  let p = cast[List](pop())
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
  let p = cast[List](pop())
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
  let p = cast[List](pop())
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
  let p = cast[List](pop())
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
  let f = cast[List](pop())
  let t = cast[List](pop())
  let b = cast[List](pop())
  saved = stack
  execTerm(b)
  let p = peek()
  stack = saved
  if(isThruthy(p)):
    execTerm(t)
  else:
    execTerm(f)

proc opCond(name: auto) =
  discard

proc opWhile(name: string) =
  discard

proc opLinrec(name: auto) =
  fourParameters(name)
  fourQuotes(name)
  let
    r2 = cast[List](pop())
    r1 = cast[List](pop())
    t = cast[List](pop())
    p = cast[List](pop())
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
  let r1 = cast[List](pop())
  let t = cast[List](pop())
  let p = cast[List](pop())
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

proc opBinrec(name: auto) =
  discard

proc opGenrec(name: auto) =
  fourParameters(name)
  fourQuotes(name)
  let r2 = cast[List](pop())
  let r1 = cast[List](pop())
  let t = cast[List](pop())
  let i = cast[List](pop())
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
  discard

proc opStep(name: auto) =
  twoParameters(name)
  oneQuote(name)
  listAsSecond(name)
  let p = cast[List](pop())
  let a = cast[List](pop())
  for x in items(a):
    push(x)
    execTerm(p)

# A V0 [P]  ->  V
proc opFold(name: auto) =
  threeParameters(name)
  oneQuote(name)
  let p = cast[List](pop())
  let v0 = pop()
  if peek() of List:
    let a = cast[List](pop())
    push(v0)
    for x in items(a):
      push(x)
      execTerm(p)
  if peek() of Set:
    let a = cast[Set](pop())
    push(v0)
    for x in items(a):
      push(newInt(x))
      execTerm(p)
  if peek() of String:
    let a = cast[String](pop())
    push(v0)
    for x in items(a):
      push(newChar(x))
      execTerm(p)  

proc opMap(name: auto) =
  twoParameters(name)
  oneQuote(name)
  listAsSecond(name)
  saved = stack
  let b = newList(@[])
  let p = cast[List](pop())
  let a = cast[List](pop())
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
  let p = cast[List](pop())
  let n = cast[Int](pop())
  for i in 0..<n.val:
    execTerm(p)

proc opInfra(name: auto) =
  twoParameters(name)
  oneQuote(name)
  listAsSecond(name)
  let p = cast[List](pop())
  let l1 = cast[List](pop())
  saved = stack
  stack = l1.val
  execTerm(p)
  let l2 = pop()
  stack = saved
  push(l2)

proc opPrimrec(name: auto) =
  threeParameters(name)
  twoQuotes(name)
  let c = cast[List](pop())
  let i = cast[List](pop())
  let x = pop()
  var n = 0
  if x of List:
    let list = cast[List](x)
    for y in items(list):
      push(y)
      inc(n)
  elif x of Set:
    let s = cast[Set](x)
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
  # TODO: support other aggregates
  listAsSecond(name)
  if stack.head.next.value of List:
    filterList()
  else:
    raiseRuntimeError("unsupported aggregate for `" & name & "`")

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
  let a = cast[List](pop())
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

method eval*(x: Value) {.base.} = push(x)

method eval*(x: Usr) = usrtable[x.id.val] = x

method eval*(x: Ident) =
  if usrtable.hasKey(x.val):
    execTerm(usrtable[x.val].term)
    return
  case x.val
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
  of UNCONS: opUncons(UNCONS)
  of UNSWONS: opUnswons(UNSWONS)
  of CONCAT: opConcat(CONCAT)
  of REVERSE: opReverse(REVERSE)
  of NULL: opNull(NULL)
  of SMALL: opSmall(SMALL)
  of GT: opGt(GT)
  of LT: opLt(LT)
  of GTE: opGte(GTE)
  of LTE: opLte(LTE)
  of EQ: opEq(EQ)
  of NEQ: opNeq(NEQ)
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
  of HELP: opHelp(HELP)
  of HELPDETAIL: opHelpdetail(HELPDETAIL)
  else:
    let msg = "undefined symbol `" & $x & "`"
    raiseRuntimeError(msg)