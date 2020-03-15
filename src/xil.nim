import scanner, strformat, parser, vm, interp

const banner = """       
      _ _ 
 __ _(_) |
 \ \ / | |
 /_\_\_|_| v0.1.0
 """

if isMainModule:
  stdout.writeLine(banner)
  while true:
    stdout.write(fmt"[{len(stack)}] ")
    let src = stdin.readLine()
    let scanner = newScanner(src);
    let parser = newParser(scanner)
    try:
      var (ok, def) = parser.tryParseDef()
      if ok:
        eval(def)
      else:
        let term = parser.parseTerm()
        for x in term: eval(x)
    except RuntimeException:
      let msg = getCurrentExceptionMsg()
      echo "Runtime error: ", msg
    except Exception:
      let
        e = getCurrentException()
        msg = getCurrentExceptionMsg()
      echo "Exception: ", repr(e), " with message ", msg