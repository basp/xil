import scanner, strformat, parser, vm, interp

while true:
  stdout.write(fmt"[{len(stack)}] ")
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