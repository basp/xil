import lists, opnames, vm, strformat

var 
  stack* = initSinglyLinkedList[Value]()
  saved = initSinglyLinkedList[Value]()

template saved2() = saved.head.next
template saved3() = saved.head.next.next
template saved4() = saved.head.next.next.next
template saved5() = saved.head.next.next.next.next
template saved6() = saved.head.next.next.next.next.next

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

proc popt[T](): T {.inline.} = 
  cast[T](pop())

proc peekt[T](): T {.inline.} = 
  cast[T](peek())

method floatable(x: Value): bool {.base,inline.} = false
method floatable(x: Int): bool {.inline.} = true
method floatable(x: Float): bool {.inline.} = true

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

proc oneParameter(name: string) {.inline.} =
  doAssert stack.head != nil, name

proc twoParameters(name: string) {.inline.} =
  oneParameter(name)
  doAssert stack.head.next != nil, name

proc threeParameters(name: string) {.inline.} =
  twoParameters(name)
  doAssert stack.head.next.next != nil, name

proc fourParameters(name: string) {.inline.} =
  threeParameters(name)
  doAssert stack.head.next.next.next != nil, name

proc fiveParameters(name: string) {.inline.} =
  fourParameters(name)
  doAssert stack.head.next.next.next.next != nil, name

proc integer(name: string) {.inline.} =
  doAssert stack.head.value.integer

proc integerAsSecond(name: string) {.inline.} =
  doAssert stack.head.next.value.integer

proc twoIntegers(name: string) {.inline.} =
  doAssert stack.head.value.integer, name
  doAssert stack.head.next.value.integer, name

proc integerOrFloat(name: string) {.inline.} =
  doAssert stack.head.value.floatable, name

proc integerOrFloatAsSecond(name: string) {.inline.} =
  doAssert stack.head.next.value.floatable, name

proc logical(name: string) {.inline.} =
  doAssert stack.head.value.logical, name

proc logicalAsSecond(name: string) {.inline.} =
  doAssert stack.head.next.value.logical, name

proc ordinal(name: string) {.inline.} =
  doAssert stack.head.value.ordinal, name

proc aggregate(name: string) {.inline.} =
  doAssert stack.head.value.aggregate, name

proc aggregateAsSecond(name: string) {.inline.} =
  doAssert stack.head.next.value.aggregate

proc listAsSecond(name: string) {.inline.} =
  doAssert stack.head.next.value.list

proc oneQuote(name: string) {.inline.} =
  doAssert stack.head.value.list, name

proc twoQuotes(name: string) {.inline.} =
  oneQuote(name)
  doAssert stack.head.next.value.list, name

proc threeQuotes(name: string) {.inline.} =
  twoQuotes(name)
  doAssert stack.head.next.next.value.list, name

proc fourQuotes(name: string) {.inline.} =
  threeQuotes(name)
  doAssert stack.head.next.next.next.value.list, name

template unary(op: untyped, name: string) =
  let x = pop()
  push(op(x))

template binary(op: untyped, name: string) =
  let y = pop()
  let x = pop()
  push(op(x, y))

template unfloatop(op: untyped, name: string) =
  oneParameter(name)
  integerOrFloat(name)
  unary(op, name)

template bifloatop(op: untyped, name: string) =
  twoParameters(name)
  integerOrFloat(name)
  integerOrFloatAsSecond(name)
  binary(op, name)

template bilogicop(op: untyped, name: string) =
  twoParameters(name)
  logical(name)
  logicalAsSecond(name)
  binary(op, name)

proc opId() {.inline.} = discard

proc opDup(name: auto) {.inline.} = 
  oneParameter(name)
  push(peek().clone)

# X Y  ->  Y X
proc opSwap(name: auto) {.inline.} =
  twoParameters(name)
  let y = pop()
  let x = pop()
  push(y)
  push(x)

# X Y Z  ->  Z X Y
proc opRollup(name: auto) {.inline.} =
  threeParameters(name)
  let z = pop()
  let y = pop()
  let x = pop()
  push(z)
  push(x)
  push(y)

# X Y Z  ->  Y Z X
proc opRolldown(name: auto) {.inline.} =
  threeParameters(name)
  let z = pop()
  let y = pop()
  let x = pop()
  push(y)
  push(z)
  push(x)

# X Y Z  ->  Z Y X
proc opRotate(name: auto) {.inline.} =
  threeParameters(name)
  let z = pop()
  let y = pop()
  let x = pop()
  push(z)
  push(y)
  push(x)

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

proc opOr(name: auto) {.inline.} = bilogicop(`or`, name)
proc opXor(name: auto) {.inline.} = bilogicop(`xor`, name)
proc opAnd(name: auto) {.inline.} = bilogicop(`and`, name)

proc opNot(name: auto) {.inline.} =
  oneParameter(name)
  logical(name)
  unary(`not`, name) 
  
proc opAdd(name: auto) {.inline.} = bifloatop(`+`, name)
proc opSub(name: auto) {.inline.} = bifloatop(`-`, name)
proc opMul(name: auto) {.inline.} = bifloatop(`*`, name)
proc opDivf(name: auto) {.inline.} = bifloatop(`/`, name)

proc opRem(name: auto) {.inline.} = bifloatop(`mod`, name)

# proc opDiv(name: auto) {.inline.} =
#   twoParameters(name)
#   twoIntegers(name)
#   let j = cast[Int](pop())
#   let i = cast[Int](pop())
#   let (k, l) = `div`(i, j)
#   push(k)
#   push(l)

# proc opSign(name: auto) {.inline.} = unfloatop(sign, name)
# proc opNeg(name: auto) {.inline.} =
#   let x = pop()
#   push(neg(x))

# template unordop(op: untyped, name: auto) =
#   oneParameter(name)
#   ordinal(name)
#   let x = pop()
#   push(op(x))

# proc opOrd(name: auto) {.inline.} = unordop(ord, name)
# proc opChr(name: auto) {.inline.} = unordop(chr, name)

# proc opAbs(name: auto) {.inline.} = unfloatop(abs, name)

# proc opMin(name: auto) {.inline.} = bifloatop(min, name)
# proc opMax(name: auto) {.inline.} = bifloatop(max, name)

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

# proc opPred(name: auto) {.inline.} = unordop(pred, name)
# proc opSucc(name: auto) {.inline.} = unordop(succ, name)

proc opPut(name: auto) {.inline.} =
  oneParameter(name)
  let x = pop()
  echo x

proc opPeek(name: string) {.inline.} =
  oneParameter(name)

proc opCons(name: auto) {.inline.} =
  twoParameters(name)
  aggregate(name)
  binary(cons, name)
  
proc opSwons(name: auto) {.inline.} =
  opSwap(name)
  opCons(name)

proc opFirst(name: auto) {.inline.} =
  oneParameter(name)
  aggregate(name)
  let a = pop()
  push(first(a))

proc opRest(name: auto) {.inline.} =
  oneParameter(name)
  aggregate(name)
  let a = pop()
  push(rest(a))

# template indexop(name: auto) =
#   let i = cast[Int](pop())
#   let a = pop()
#   push(at(a, i))

# proc opAt(name: auto) {.inline.} =
#   twoParameters(name)
#   integer(name)
#   aggregateAsSecond(name)
#   indexop(name)

# proc opOf(name: auto) {.inline.} =
#   twoParameters(name)
#   aggregate(name)
#   integerAsSecond(name)
#   opSwap(name)
#   indexop(name)

proc opSize(name: auto) {.inline.} =
  oneParameter(name)
  aggregate(name)
  let a = pop()
  push(size(a))

# proc popuncons(name: auto): (Value, Value) {.inline.} =
#   oneParameter(name)
#   aggregate(name)
#   let a = pop()
#   uncons(a)

# proc opUncons(name: auto) {.inline.} =
#   let (first, rest) = popuncons(name)
#   push(first)
#   push(rest)

# proc opUnswons(name: auto) {.inline.} =
#   let (first, rest) = popuncons(name)
#   push(rest)
#   push(first)

proc opNull(name: auto) {.inline.} =
  oneParameter(name)
  let x = pop()
  push(null(x))

proc opSmall(name: auto) {.inline.} =
  oneParameter(name)
  let x = pop()
  push(small(x))

template cmpop(op: untyped, name: auto) =
  twoParameters(name)
  let y = pop()
  let x = pop()
  push(newBool(op(cmp(x, y).value, 0)))

proc opLt(name: auto) {.inline.} = cmpop(`<`, name)
proc opGt(name: auto) {.inline.} = cmpop(`>`, name)
proc opLte(name: auto) {.inline.} = cmpop(`<=`, name)
proc opGte(name: auto) {.inline.} = cmpop(`>=`, name)

proc opEq(name: auto) =
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
  let p = popt[ListVal]()
  execterm(p)

proc opX(name: auto) {.inline.} =
  oneParameter(name)
  oneQuote(name)
  let p = peekt[ListVal]()
  execterm(p)

proc opDip(name: auto) {.inline.} =
  twoParameters(name)
  oneQuote(name)
  let p = popt[ListVal]()
  let x = pop()
  execterm(p)
  push(x)

proc opApp1(name: auto) {.inline.} =
  twoParameters(name)
  oneQuote(name)
  let p = popt[ListVal]()
  discard pop()
  execterm(p)

template nary(paramcount: untyped, name: auto, top: untyped) =
  paramcount(name)
  oneQuote(name)
  saved = stack
  let p = popt[ListVal]()
  execterm(p)
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
  let p = popt[ListVal]()
  stack.head = saved2
  execterm(p)
  let py = peek()
  stack.head = saved3
  execterm(p)
  let px = peek()
  stack.head = saved4
  push(px)
  push(py)

proc opUnary3(name: auto) =
  fourParameters(name)
  oneQuote(name)
  saved = stack
  let p = popt[ListVal]()
  stack.head = saved2
  execterm(p)
  let pz = peek()
  stack.head = saved3
  execterm(p)
  let py = peek()
  stack.head = saved4
  execterm(p)
  let px = peek()
  stack.head = saved5
  push(px)
  push(py)
  push(pz)

proc opUnary4(name: auto) =
  fiveParameters(name)
  oneQuote(name)
  saved = stack
  let p = popt[ListVal]()
  stack.head = saved2
  execterm(p)
  let pw = peek()
  stack.head = saved3
  execterm(p)
  let pz = peek()
  stack.head = saved4
  execterm(p)
  let py = peek()
  stack.head = saved5
  execterm(p)
  let px = peek()
  stack.head = saved6
  push(px)
  push(py)
  push(pz)
  push(pw)

proc opIfte(name: auto) =
  threeParameters(name)
  let f = popt[ListVal]()
  let t = popt[ListVal]()
  let b = popt[ListVal]()
  saved = stack
  execterm(b)
  let p = peek()
  stack = saved
  if(isThruthy(p)):
    execterm(t)
  else:
    execterm(f)    

proc opMap(name: auto) =
  twoParameters(name)
  oneQuote(name)
  listAsSecond(name)
  saved = stack
  let b = newList(@[])
  let p = popt[List]()
  let a = popt[List]()
  for x in a.val:
    push(x)
    execterm(p)
    b.val.append(pop())
    stack = saved
  stack.head = saved3
  push(b)

proc opTimes(name: auto) =
  twoParameters(name)
  oneQuote(name)
  integerAsSecond(name)
  let p = popt[List]()
  let n = popt[Int]()
  for i in 0..<n.val:
    execterm(p)

proc filterList() =
  var a1 = newList(@[])
  let p = popt[List]()
  let a = popt[List]()
  saved = stack
  for x in a.val:
    push(x)
    execterm(p)
    if isThruthy(peek()):
      a1.val.append(x)
    stack = saved
  push(a1)

proc filterSet() =
  var a1 = 0
  let p = popt[List]()
  let a = popt[Set]()
  saved = stack
  for x in items(a.val:
    push(newInt(x))
    execterm(p)
    if isThruthy(peek()):
      a1 = add(a1, x)
    stack = saved
  push(newSet(a1))

proc filterString() =
  var a1 = ""
  let p = popt[List]()
  let a = popt[String]()
  saved = stack
  for x in a.val:
    push(newChar(x))
    execterm(p)
    if isThruthy(peek()):
      a1 &= x
    stack = saved
  push(newString(a1))

proc opFilter(name: auto) =
  twoParameters(name)
  oneQuote(name)
  aggregateAsSecond(name)
  if stack.head.next.value of List:
    filterList()
  elif stack.head.next.value of Set:
    filterSet()
  elif stack.head.next.value of String:
    filterString()

method eval*(x: Value) {.base.} =
  push(x)

method eval*(x: Ident) =
  case x.value
  of ID: opId()
  of DUP: opDup(DUP)
  of SWAP: opSwap(SWAP)
  of ROLLUP: opRollup(ROLLUP)
  of ROLLDOWN: opRolldown(ROLLDOWN)
  of ROTATE: opRotate(ROTATE)
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
  of DIVF: opDivf(DIVF)
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
  of CONS: opCons(CONS)
  of SWONS: opSwons(SWONS)
  of FIRST: opFirst(FIRST)
  of REST: opRest(REST)
  of AT: opAt(AT)
  of OF: opOf(OF)
  of SIZE: opSize(SIZE)
  of UNCONS: opUncons(UNCONS)
  of UNSWONS: opUnswons(UNSWONS)
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
  of MAP: opMap(MAP)
  of TIMES: opTimes(TIMES)
  of FILTER: opFilter(FILTER)
  else:
    let msg = "undefined symbol `" & $x & "`"
    raiseRuntimeError(msg)