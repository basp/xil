import lists, hashes, vm, sequtils

when isMainModule:
  block:
    var a = cast[Value](newList(@[
      cast[Value](newInt(1)),
      cast[Value](newInt(2))]))
    var b = cast[Value](newList(@[
      cast[Value](newInt(1)),
      cast[Value](newInt(2))]))
    var c = cast[Value](newList(@[
      cast[Value](newInt(1)),
      cast[Value](newInt(2)),
      cast[Value](newInt(3))]))
    var d = cast[Value](newList(@[
      cast[Value](newInt(2)),
      cast[Value](newInt(1))]))
    assert a == b
    assert b == a
    assert a != c
    assert b != c
    assert a != d
    assert d != a
    assert d == d
    assert d != b
    assert b != d
  
  block:
    var a = cast[Value](newList(@[
      cast[Value](newInt(1)),
      cast[Value](newInt(2))]))
    var b = cast[Value](newList(@[
      cast[Value](newInt(1)),
      cast[Value](newInt(2))]))
    var c = cast[Value](newList(@[
      cast[Value](newInt(1)),
      cast[Value](newInt(2)),
      cast[Value](newInt(3))]))
    var d = cast[Value](newList(@[
      cast[Value](newInt(2)),
      cast[Value](newInt(1))]))
    assert cmp(a, b) == newInt(0)
    assert cmp(b, a) == newInt(0)
    assert cmp(a, c) == newInt(-1)
    assert cmp(c, a) == newInt(1)
    assert cmp(a, a) == newInt(0)
    assert cmp(a, d) == newInt(-1)
    assert cmp(d, a) == newInt(1)  
  
  block:
    var a = cast[Value](newBool(true))
    var b = cast[Value](newBool(false))
    assert hash(a) == hash(true)
    assert hash(b) == hash(false)
    a = cast[Value](newIdent("foo"))
    b = cast[Value](newIdent("bar"))
    assert hash(a) == hash("foo")
    assert hash(b) == hash("bar")

  block:
    var u = newList(@[
      cast[Value](newInt(1)),
      cast[Value](newInt(2))])
    var v = cast[List](u.clone())
    assert u == v
    v.val.append(newInt(3))
    assert u != v

  block:
    var q = cast[Value](newList(@[]))
    q = cons(newInt(123), q)
    q = cons(newInt(456), q)
    var xs = toSeq(items(cast[List](q)))
    assert xs[0] == newInt(456)
    assert xs[1] == newInt(123)

  block:
    var q = cast[Value](newSet(0))
    q = cons(newInt(1), q)
    q = cons(newInt(2), q)
    assert size(q) == newInt(2)
    q = cons(newInt(3), q)
    assert size(q) == newInt(3)