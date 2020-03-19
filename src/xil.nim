import strformat, scan, parse, vm, interp

const version = "0.1.0"

const banner = fmt"""       
      _ _ 
 __ _(_) |
 \ \ / | |
 /_\_\_|_| v{version}
 """

if isMainModule:
  stdout.writeLine(banner)
  while true:
    try:
      stdout.write(fmt"{len(stack)}> ")
      let src = stdin.readLine()
      let scanner = newScanner(src);
      let parser = newParser(scanner)
      if scanner.peek() == ':':
        var def = parser.parseDef()
        eval(def)
      else:
        let term = parser.parseTerm()
        for x in term: eval(x)
    except ParseException:
      let msg = getCurrentExceptionMsg()
      echo "Parse error: ", msg
    except RuntimeException:
      let msg = getCurrentExceptionMsg()
      echo "Runtime error: ", msg
    except SyntaxException:
      let msg = getCurrentExceptionMsg()
      echo "Syntax error: ", msg
    except Exception:
      let
        e = getCurrentException()
        msg = getCurrentExceptionMsg()
      echo "Exception: ", repr(e), " with message ", msg
