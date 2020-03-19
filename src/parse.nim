import math, tables, strutils, strformat, scan, vm

type
  Parser = ref object
    s: Scanner
    curTok: Token
    nextTok: Token
  ParseException* = object of Exception

proc raiseParseError*(msg: string) =
  raise newException(ParseException, msg)

proc advance(p: Parser) =
  p.curTok = p.nextTok
  p.nextTok = p.s.next()

var reserved = initTable[string, proc(): Value]()
reserved.add("true", proc(): Value = newBool(true))
reserved.add("false", proc(): Value = newBool(false))
reserved.add("pi", proc(): Value = newFloat(math.PI))
reserved.add("tau", proc(): Value = newFloat(math.TAU))
reserved.add("e", proc(): Value = newFloat(math.E))

proc parseIdent(p: Parser): Value =
  if reserved.hasKey(p.curTok.lexeme):
    result = reserved[p.curTok.lexeme]()
  else:
    result = newIdent(p.curTok.lexeme)
  p.advance()

proc parseChar(p: Parser): Value =
  # TODO:
  # fancier string to char conversion; this 
  # assumes all char literals look like 'c'
  proc convert(s: string): char = s[1]
  let c = convert(p.curTok.lexeme)
  result = newChar(c)
  p.advance()

proc parseString(p: Parser): Value =
  let s = p.curTok.lexeme.strip(chars = {'"'})
  result = newString(s)
  p.advance()

proc tryParseInt(s: string): (bool, int) =
  try:
    let i = strutils.parseInt(s)
    result = (true, i)
  except:
    result = (false, 0)

proc parseNumber(p: Parser): Value =
  let s = p.curTok.lexeme
  let (ok, i) = tryParseInt(s)
  if ok:
    result = newInt(i)
    p.advance()
    return
  # if we can't parse it as an int we'll
  # force parse it as a float instead
  try:
    let f = parseFloat(p.curTok.lexeme)
    result = newFloat(f)
    p.advance()
    return
  except:
    raiseParseError(fmt"bad numeric '{s}'");

proc parseFactor*(p: Parser): Value

proc parseList(p: Parser): Value =
  var terms : seq[Value] = @[]
  p.advance()
  while p.curTok.kind != tkRBrack:
    let fac = p.parseFactor()
    terms.add(fac)
  if p.curTok.kind != tkRBrack:
    raiseParseError(fmt"expected {tkRBrack}")
  p.advance()
  newList(terms)

proc parseSet(p: Parser): Value =
  var s = 0
  p.advance()
  while p.curTok.kind != tkRBrace:
    let i = parseInt(p.curTok.lexeme)
    s = s or (1 shl i)
    p.advance()
  if p.curTok.kind != tkRBrace:
    raiseParseError(fmt"expected {tkRBrace}")  
  p.advance()
  return newSet(s)

proc parseTerm*(p: Parser): List =
  var xs: seq[Value] = @[]
  while(p.curTok.kind != tkEOF and p.curTok.kind != tkSemicolon):
    let fac = p.parseFactor()
    xs.add(fac)
  newList(xs)

proc parseDef*(p: Parser): Value =
  if p.curTok.kind != tkColon:
    raiseParseError(fmt"expected {tkColon}")
  p.advance()
  let id = p.parseIdent()
  let term = p.parseTerm()
  if p.curTok.kind != tkSemicolon:
    raiseParseError(fmt"expected {tkSemicolon}")
  p.advance()
  newUsr(cast[Ident](id), term)

proc tryParseDef*(p: Parser): (bool, Usr) =
  try:
    let x = p.parseDef()
    return (true, cast[Usr](x))
  except:
    return (false, nil)

proc parseFactor*(p: Parser): Value =
  case p.curTok.kind
  of tkColon: p.parseDef()
  of tkLBrack: p.parseList()
  of tkLBrace: p.parseSet()
  of tkNumber: p.parseNumber()
  of tkChar: p.parseChar()
  of tkString: p.parseString()
  of tkIdent: p.parseIdent()
  else: 
    # this will most likely be caused by
    # an unterminated literal and it's
    # highly likely we'll be hitting tkEOF
    raise newException(Exception, $p.curTok.kind)

proc newParser*(s: Scanner): Parser =
  result = new(Parser)
  result.s = s
  result.advance()
  result.advance()