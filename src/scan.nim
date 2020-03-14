import strformat

type 
  TokenKind* = enum
    tkNone,
    tkIllegal,
    tkEOF,
    tkIdent,
    tkNumber,
    tkChar,
    tkString,
    tkList,
    tkLBrack,
    tkRBrack,
    tkLBrace,
    tkRBrace,
    tkColon,
    tkSemicolon,
  Token* = object
    kind*: TokenKind
    lexeme*: string
    pos*: int
  Scanner* = ref object
    src: string
    pos: int
    readPos: int
    ch: char

proc initToken(kind: TokenKind, lexeme: string, pos: int): Token =
  result.kind = kind
  result.lexeme = lexeme
  result.pos = pos

proc advance(s: Scanner) =
  if s.readPos >= len(s.src):
    s.ch = char(0)
  else:
    s.ch = s.src[s.readPos]
  s.pos = s.readPos
  s.readPos += 1

proc isReserved(ch: char): bool =
  case ch
  of '[': true
  of ']': true
  of '{': true
  of '}': true
  of ':': true
  of ';': true
  else: false

proc isDigit(ch: char): bool = '0' <= ch and ch <= '9'

proc isWhitespace(ch: char): bool =
  ch == ' ' or ch == '\t' or ch == '\n' or ch == '\r'

proc isIdentChar(ch: char): bool =
  not (isWhitespace(ch) or isReserved(ch) or ch == char(0))

proc skipWhitespace(s: Scanner) =
  while isWhitespace(s.ch):
    s.advance()

proc readIdent(s: Scanner): string =
  let pos = s.pos
  while isIdentChar(s.ch):
    s.advance()
  s.src[pos..<s.pos]

proc readNumber(s: Scanner): string =
  let pos = s.pos
  if s.ch == '-':
    s.advance()
  while isDigit(s.ch):
    s.advance()
  if s.ch == '.':
    s.advance()
  while isDigit(s.ch):
    s.advance()
  s.src[pos..<s.pos]

proc readUntil(s: Scanner, fin: char, name: string): string =
  let pos = s.pos
  s.advance()
  while s.ch != fin and s.ch != char(0):
    s.advance()
  if s.ch != fin:
    let msg = fmt"unterminated {name} literal (pos: {pos})"
    raise newException(Exception, msg)
  s.advance()
  s.src[pos..<s.pos]

proc newScanner*(src: string): Scanner =
  result = new(Scanner)
  result.src = src
  result.advance()

proc next*(s: Scanner): Token =
  s.skipWhitespace()
  case s.ch
  of '[': result = initToken(tkLBrack, "[", s.pos)
  of ']': result = initToken(tkRBrack, "]", s.pos)
  of '{': result = initToken(tkLBrace, "{", s.pos)
  of '}': result = initToken(tkRBrace, "}", s.pos)
  of ':': result = initToken(tkColon, ":", s.pos)
  of ';': result = initToken(tkSemicolon, ";", s.pos)
  of '"':
    result.pos = s.pos
    result.lexeme = s.readUntil('"', "string")
    result.kind = tkString
    return
  of '\'':
    result.pos = s.pos
    result.lexeme = s.readUntil('\'', "char")
    result.kind = tkChar
    return
  of char(0):
    result.lexeme = ""
    result.kind = tkEOF
    return
  else:
    if isDigit(s.ch) or s.ch == '-':
      result.pos = s.pos
      result.lexeme = s.readNumber()
      result.kind = tkNumber
    else:
      result.pos = s.pos
      result.lexeme = s.readIdent()
      result.kind = tkIdent
    return
  s.advance()

when isMainModule:
  let src = "'foo' \"bar\" 123 [] quux 123.56 true false"
  let tests = [
    (tkChar, "'foo'"),
    (tkString, "\"bar\""),
    (tkNumber, "123"),
    (tkLBrack, "["),
    (tkRBrack, "]"),
    (tkIdent, "quux"),
    (tkNumber, "123.56"),
    (tkIdent, "true"),
    (tkIdent, "false")
  ]

  var tok: Token
  let scanner = newScanner(src)
  for (kind, lexeme) in tests:
    tok = scanner.next()
    assert tok.kind == kind, $kind
    assert tok.lexeme == lexeme, lexeme
