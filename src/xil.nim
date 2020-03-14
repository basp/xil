import lists, scanner, parser, vm, interp

while true:
  let src = stdin.readLine()
  try:
    let scanner = newScanner(src)
    let parser = newParser(scanner)
    let term = parser.parseTerm()
    for fac in term:
      eval(fac)
  except:
    let msg = getCurrentExceptionMsg()
    echo(msg)
  finally:
    echo "current stack:"
    for x in stacK.items:
      echo x