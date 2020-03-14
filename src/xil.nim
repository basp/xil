import scanner

while true:
  let src = stdin.readLine()
  let scanner = newScanner(src)
  try:
    var tok = scanner.next()
    while tok.kind != tkEOF:
      echo(tok)
      tok = scanner.next()
  except:
    let msg = getCurrentExceptionMsg()
    echo(msg)