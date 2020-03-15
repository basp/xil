import scan

when isMainModule:
  let src = """
    'foo' "bar" 123 [] quux 
    123.56 true false -123 
    -123.456"""
  let tests = [
    (tkChar, "'foo'"),
    (tkString, "\"bar\""),
    (tkNumber, "123"),
    (tkLBrack, "["),
    (tkRBrack, "]"),
    (tkIdent, "quux"),
    (tkNumber, "123.56"),
    (tkIdent, "true"),
    (tkIdent, "false"),
    (tkNumber, "-123"),
    (tkNumber, "-123.456")]
  var tok: Token
  let scanner = newScanner(src)
  for (kind, lexeme) in tests:
    tok = scanner.next()
    assert tok.kind == kind, $kind
    assert tok.lexeme == lexeme, lexeme