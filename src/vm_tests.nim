import lists, hashes, sequtils, vm

when isMainModule:
  block:
    # test deep equality for lists
    # this will implicitly test for 
    # deep eqaulity of values
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
    # test deep comparison for lists
    # this will implicitly test for 
    # deep comparison of values
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
    # test hashing on basic values
    var a = cast[Value](newBool(true))
    var b = cast[Value](newBool(false))
    assert hash(a) == hash(true)
    assert hash(b) == hash(false)
    a = cast[Value](newIdent("foo"))
    b = cast[Value](newIdent("bar"))
    assert hash(a) == hash("foo")
    assert hash(b) == hash("bar")

  block:
    # test deep cloning on lists
    var u = newList(@[
      cast[Value](newInt(1)),
      cast[Value](newInt(2))])
    var v = cast[List](u.clone())
    assert u == v
    v.val.append(newInt(3))
    assert u != v

  block:
    # test cons on lists
    var q = cast[Value](newList(@[]))
    q = cons(newInt(123), q)
    q = cons(newInt(456), q)
    var xs = toSeq(items(cast[List](q)))
    assert xs[0] == newInt(456)
    assert xs[1] == newInt(123)

  block:
    # test cons on sets
    var q = cast[Value](newSet(0))
    q = cons(newInt(1), q)
    q = cons(newInt(2), q)
    assert size(q) == newInt(2)
    q = cons(newInt(3), q)
    assert size(q) == newInt(3)

  block:
    # test the null operator
    var tests = @[
      (cast[Value](newInt(0)), true),
      (newInt(1), false),
      (newBool(true), false),
      (newBool(false), true),
      (newString(""), true),
      (newString("foo"), false),
      (newChar(char(0)), true),
      (newChar('a'), false),
      (newList(), true)]      
    for (x, exp) in tests:
      assert null(x) == exp

  block:
    # test rest and first operators on sets
    # note that sets are always ordered ordinally
    var s = cast[Value](newSet(0))
    s = cons(newInt(1), s)
    s = cons(newInt(2), s)
    s = cons(newInt(3), s)
    assert s.first == newInt(1)
    assert s.rest.first == newInt(2)
    assert s.rest.rest.first == newInt(3)

  block:
    # test rest and first operators on lists
    # note that lists are always ordered by 
    # insertion order
    var s = cast[Value](newList())
    s = cons(newInt(1), s)
    s = cons(newInt(2), s)
    s = cons(newInt(3), s)
    assert s.first == newInt(3)
    assert s.rest.first == newInt(2)
    assert s.rest.rest.first == newInt(1)
