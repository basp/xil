namespace Xil
{
    using System;

    public class Builtin
    {
        public Builtin(Action op)
            : this(op, string.Empty, string.Empty)
        {
        }

        public Builtin(Action op, string effect, params string[] notes)
        {
            this.Op = op;
            this.Effect = effect;
            this.Notes = notes;
        }

        public Action Op { get; }

        public string Effect { get; }

        public string[] Notes { get; }

        internal static Builtin True(Action op) =>
            new Builtin(
                op,
                "->  true",
                "Pushes the value true.");

        internal static Builtin False(Action op) =>
            new Builtin(
                op,
                "->  false",
                "Pushes the value false.");

        internal static Builtin Pi(Action op) =>
            new Builtin(
                op,
                "->  pi",
                "Pushes the value pi.");

        internal static Builtin Stack(Action op) =>
            new Builtin(
                op,
                ".. X Y Z  ->  .. X Y Z [Z Y X ..]",
                "Pushes the stack as a list.");

        internal static Builtin Newstack(Action op) =>
            new Builtin(
                op,
                "..  ->",
                "Clears the stack.");

        internal static Builtin Rand(Action op) =>
            new Builtin(
                op,
                "->  F",
                "F is a random float between 0.0 and 1.0");

        internal static Builtin Id(Action op) =>
            new Builtin(
                op,
                "->",
                "Identity function, does nothing.",
                "Any program of the form  P id Q  is equivalent to just  P Q.");

        internal static Builtin Dup(Action op) =>
            new Builtin(
                op,
                "X  ->  X X",
                "Pushes an extra copy of X onto the stack.");

        internal static Builtin Swap(Action op) =>
            new Builtin(
                op,
                "X Y  ->  Y X",
                "Interchanges X and Y on top of the stack.");

        internal static Builtin Rollup(Action op) =>
            new Builtin(
                op,
                "X Y Z  ->  Z X Y",
                "Moves X and Y up, moves Z down.");

        internal static Builtin Rolldown(Action op) =>
            new Builtin(
                op,
                "X Y Z  ->  Y Z X",
                "Moves Y and Z down, moves X up.");

        internal static Builtin Rotate(Action op) =>
            new Builtin(
                op,
                "X Y Z  ->  Z Y X",
                "Interchanges X and Z.");

        internal static Builtin Popd(Action op) =>
            new Builtin(
                op,
                "As if defined by:   popd  ==  [pop] dip");

        internal static Builtin Dupd(Action op) =>
            new Builtin(
                op,
                "As if defined by:   dupd  ==  [dup] dip");

        internal static Builtin Swapd(Action op) =>
            new Builtin(
                op,
                "As if defined by:   swapd  ==  [swap] dip");

        internal static Builtin Rollupd(Action op) =>
            new Builtin(
                op,
                "As if defined by:   rollupd  ==  [rollup] dip");

        internal static Builtin Rolldownd(Action op) =>
            new Builtin(
                op,
                "As if defined by:   rolldownd  ==  [rolldown] dip");

        internal static Builtin Rotated(Action op) =>
            new Builtin(
                op,
                "As if defined by:   rotated  ==  [rotate] dip");

        internal static Builtin Pop(Action op) =>
            new Builtin(
                op,
                "X  ->",
                "Removes X from top of the stack.");

        internal static Builtin Choice(Action op) =>
            new Builtin(
                op,
                "B T F  ->  X",
                "If B is true, then X = T else X = F.");

        internal static Builtin Or(Action op) =>
            new Builtin(
                op,
                "X Y  ->  Z",
                "Z is the union of sets X and Y, logical disjunction for truth values.");

        internal static Builtin Xor(Action op) =>
            new Builtin(
                op,
                "X Y  ->  Z",
                "Z is the symmetric difference of sets X and Y,",
                "logical exclusive disjunction for truth values.");

        internal static Builtin And(Action op) =>
            new Builtin(
                op,
                "X Y  ->  Z",
                "Z is the intersection of sets X and Y, logical conjunction for truth values.");

        internal static Builtin Not(Action op) =>
            new Builtin(
                op,
                "X  ->  Y",
                "Y is logical negation for truth values.");

        internal static Builtin Add(Action op) =>
            new Builtin(
                op,
                "M I  ->  N",
                "Numeric N is the result of adding integer I to numeric M",
                "Also supports float.");

        internal static Builtin Sub(Action op) =>
            new Builtin(
                op,
                "M I  ->  N",
                "Numeric N is the result of subtracting integer I from numeric M.",
                "Also supports float.");

        internal static Builtin Mul(Action op) =>
            new Builtin(
                op,
                "I J  ->  K",
                "Integer K is the product of integers I and J.",
                "Also supports float.");

        internal static Builtin Ratio(Action op) =>
            new Builtin(
                op,
                "I J  ->  K",
                "Integer K is the (rounded) ratio of integers I and J.",
                "Also supports float.");

        internal static Builtin Rem(Action op) =>
            new Builtin(
                op,
                "I J  ->  K",
                "Integer K is the remainder of dividing I by J.");

        internal static Builtin Div(Action op) =>
            new Builtin(
                op,
                "I J  ->  K L",
                "Integers K and L are the quotient and remainder of dividing I by J.");

        internal static Builtin Sign(Action op) =>
            new Builtin(
                op,
                "N1  ->  N2",
                "Integer N2 is the sign (-1 or 0 or +1) of integer N1,",
                "or float N2 is the sign (-1.0 or 0.0 or 1.0) of float N1.");

        internal static Builtin Neg(Action op) =>
            new Builtin(
                op,
                "I  ->  J",
                "Integer J is the negative of integer I.",
                "Also supports floats.");

        internal static Builtin Ord(Action op) =>
            new Builtin(
                op,
                "C  ->  I",
                "Integer I is the Ascii value of character C (or logical or integer).");

        internal static Builtin Chr(Action op) =>
            new Builtin(
                op,
                "I  ->  C",
                "C is the character whose ascii value is integer I (or logical or character).");

        internal static Builtin Sqrt(Action op) =>
            new Builtin(
                op,
                "F  ->  G",
                "G is the square root of F.");

        internal static Builtin Abs(Action op) =>
            new Builtin(
                op,
                "N1  ->  N2",
                "Integer N2 is the absolute value (0,1,2..) of integer N1,",
                "or float N2 is the absolute value (0.0 ..) of float N1");

        internal static Builtin Acos(Action op) =>
            new Builtin(
                op,
                "");
        internal static Builtin Asin(Action op) =>
            new Builtin(
                op,
                "");
        internal static Builtin Atan(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Cos(Action op) =>
            new Builtin(
                op,
                "F  ->  G",
                "G is the cosine of F.");

        internal static Builtin Cosh(Action op) =>
            new Builtin(
                op,
                "F  ->  G",
                "G is the hyperbolic cosine of F.");

        internal static Builtin Sin(Action op) =>
            new Builtin(
                op,
                "F  ->  G",
                "G is the sine of F.");

        internal static Builtin Sinh(Action op) =>
            new Builtin(
                op,
                "F  ->  G",
                "G is the hyperbolic sine of F.");

        internal static Builtin Tan(Action op) =>
            new Builtin(
                op,
                "F  ->  G",
                "G is the tangent of F.");

        internal static Builtin Tanh(Action op) =>
            new Builtin(
                op,
                "F  ->  G",
                "G is the hyperbolic tangent of F.");

        internal static Builtin Exp(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Log(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Log10(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Floor(Action op) =>
            new Builtin(
                op,
                "F  ->  G",
                "G is the floor of F.");

        internal static Builtin Ceil(Action op) =>
            new Builtin(
                op,
                "F  ->  G",
                "G is the float ceiling of F.");

        internal static Builtin Pred(Action op) =>
            new Builtin(
                op,
                "M  ->  N",
                "Ordinal N is the predecessor of ordinal M.");

        internal static Builtin Succ(Action op) =>
            new Builtin(
                op,
                "M  ->  N",
                "Ordinal N is the successor of ordinal M.");

        internal static Builtin Max(Action op) =>
            new Builtin(
                op,
                "M  ->  N",
                "N is the maximum of numeric values N1 and N2.");

        internal static Builtin Min(Action op) =>
            new Builtin(
                op,
                "M  ->  N",
                "N is the minimum of numeric values N1 and N2.");

        internal static Builtin Unstack(Action op) =>
            new Builtin(
                op,
                "[X Y ..]  ->  .. Y X",
                "The list [X Y ..] becomes the new stack.");

        internal static Builtin Cons(Action op) =>
            new Builtin(
                op,
                "X Y  ->  B",
                "Aggregate B is A with a new member X (first member for sequences).");

        internal static Builtin Swons(Action op) =>
            new Builtin(
                op,
                "A X  ->  B",
                "Aggregate B is A with a new member X (first member for sequences).");

        internal static Builtin First(Action op) =>
            new Builtin(
                op,
                "A  ->  F",
                "F is the first member of the non-empty aggregate A.");

        internal static Builtin Rest(Action op) =>
            new Builtin(
                op,
                "A  ->  R",
                "R is the non-empty aggregate A with its first member removed.");

        internal static Builtin At(Action op) =>
            new Builtin(
                op,
                "A I  ->  X",
                "X (= A[I]) is the member of A at position I.");

        internal static Builtin Of(Action op) =>
            new Builtin(
                op,
                "I A  ->  X",
                "X (= A[I]) is the I-th member of aggregate A.");

        internal static Builtin Size(Action op) =>
            new Builtin(
                op,
                "A  ->  I",
                "Integer I is the number of elements of aggregate A.");

        internal static Builtin Opcase(Action op) =>
            new Builtin(
                op,
                "X [..[X xs]..]  ->  [Xs]",
                "Indexing on the type of X, returns the list [Xs].");

        internal static Builtin Case(Action op) =>
            new Builtin(
                op,
                "X [..[X xs]..]  ->  Y i",
                "Indexing on the value of X, execute the matching Y.");

        internal static Builtin Uncons(Action op) =>
            new Builtin(
                op,
                "A  ->  F R",
                "F and R are the first and rest of non-empty aggregate A.");

        internal static Builtin Unswons(Action op) =>
            new Builtin(
                op,
                "A  ->  R F",
                "R and F are the rest and first of non-empty aggregate A.");

        internal static Builtin Drop(Action op) =>
            new Builtin(
                op,
                "A N  ->  B");

        internal static Builtin Take(Action op) =>
            new Builtin(
                op,
                "A N  ->  B");

        internal static Builtin Concat(Action op) =>
            new Builtin(
                op,
                "S T  ->  U");

        internal static Builtin Enconcat(Action op) =>
            new Builtin(
                op,
                "X S T  ->  U");

        internal static Builtin Name(Action op) =>
            new Builtin(
                op,
                "sym  ->  \"sym\"",
                "For operators and combinators, the string \"sym\" is the name of item sym,",
                "for literals sym the result string is its type.");

        internal static Builtin Intern(Action op) =>
            new Builtin(
                op,
                "\"sym\"  ->  sym",
                "Pushes the item whose name is \"sym\".");

        internal static Builtin Body(Action op) =>
            new Builtin(
                op,
                "U  ->  [P]",
                "Quotation [P] is the body of user-defined symbol U.");

        internal static Builtin Null(Action op) =>
            new Builtin(
                op,
                "X  ->  B",
                "Tests for empty aggregate X or zero numeric.");

        internal static Builtin Small(Action op) =>
            new Builtin(
                op,
                "X  ->  B",
                "Tests whether aggregate X has 0 or 1 members, or numeric 0 or 1.");

        internal static Builtin Cmp(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Eq(Action op) =>
            new Builtin(
                op,
                "X Y  ->  B",
                "Either both X and Y are numeric or both are strings or symbols.",
                "Tests whether X equal to Y.  Also supports float.");

        internal static Builtin Ne(Action op) =>
            new Builtin(
                op,
                "X Y  ->  B");

        internal static Builtin Lt(Action op) =>
            new Builtin(
                op,
                "X Y  ->  B");

        internal static Builtin Lte(Action op) =>
            new Builtin(
                op,
                "X Y  ->  B");

        internal static Builtin Gt(Action op) =>
            new Builtin(
                op,
                "X Y  ->  B");

        internal static Builtin Gte(Action op) =>
            new Builtin(
                op,
                "X Y  ->  B");

        internal static Builtin Equal(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin In(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Has(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Int(Action op) =>
            new Builtin(
                op,
                "X  ->  B",
                "Tests whether X is an integer.");

        internal static Builtin Float(Action op) =>
            new Builtin(
                op,
                "X  ->  B",
                "Tests whether X is a float.");

        internal static Builtin Char(Action op) =>
            new Builtin(
                op,
                "X  ->  B",
                "Tests whether X is a char.");

        internal static Builtin Logical(Action op) =>
            new Builtin(
                op,
                "X  ->  B",
                "Tests whether X is a logical.");

        internal static Builtin String(Action op) =>
            new Builtin(
                op,
                "X  ->  B",
                "Tests whether X is a string.");

        internal static Builtin List(Action op) =>
            new Builtin(
                op,
                "X  ->  B",
                "Tests whether X is a list.");

        internal static Builtin Leaf(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Stream(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin I(Action op) =>
            new Builtin(
                op,
                "[P]  ->  ...",
                "Executes P. So, [P] i  ==  P.");

        internal static Builtin X(Action op) =>
            new Builtin(
                op,
                "[P] i  ->  ...",
                "Executes P without popping [P]. So, [P] x  ==  [P] P.");

        internal static Builtin Dip(Action op) =>
            new Builtin(
                op,
                "X [P]  ->  ... X",
                "Saves X, executes P, pushes X back.");

        internal static Builtin App1(Action op) =>
            new Builtin(
                op,
                "X [P]  ->  R",
                "Executes P, pushes result R on stack without X.");

        internal static Builtin App11(Action op) =>
            new Builtin(
                op,
                "X Y [P]  ->  R",
                "Executes P, pushes result R on stack.");

        internal static Builtin App12(Action op) =>
            new Builtin(
                op,
                "X Y1 Y2 [P]  ->  R1 R2",
                "Executes P twice, with Y1 and Y2, returns R1 and R2.");

        internal static Builtin Construct(Action op) =>
            new Builtin(
                op,
                "[P] [[P1] [P2] ..]  ->  R1 R2 ..",
                "Saves state of stack and executes [P].",
                "Then executes each [Pi] to give Ri pushed onto saved stack.");

        internal static Builtin Nullary(Action op) =>
            new Builtin(
                op,
                "[P]  ->  R",
                "Executes P, which leaves R on top of the stack.",
                "No matter how many parameters this consumes,",
                "none are removed from the stack.");

        internal static Builtin Unary(Action op) =>
            new Builtin(
                op,
                "[P]  ->  R",
                "Executes P, which leaves R on top of the stack.",
                "No matter how many parameters this consumes, ",
                "exactly one is removed from the stack.");

        internal static Builtin Ifte(Action op) =>
            new Builtin(
                op,
                "[B] [T] [F]  ->  ...",
                "Executes B. If that yields true, then executes T else executes F.");

        internal static Builtin Step(Action op) =>
            new Builtin(
                op,
                "A [P]  -> ...",
                "Sequentially putting members of aggregate A onto stack,",
                "executes P for each member of A.");

        internal static Builtin Map(Action op) =>
            new Builtin(
                op,
                "A [P]  ->  B",
                "Executes P on each member of aggregate A,",
                "collects results in sametype aggregate B.");

        internal static Builtin Times(Action op) =>
            new Builtin(
                op,
                "N [P]  ->  ...",
                "N times executes P.");

        internal static Builtin Infra(Action op) =>
            new Builtin(
                op,
                "L1 [P]  ->  L2",
                "Using list L1 as stack, executes P and returns a new list L2.",
                "The first element of L1 is used as the top of stack,",
                "and after execution of P the top of stack becomes the first element of L2.");

        internal static Builtin Reverse(Action op) =>
            new Builtin(
                op,
                "[X Y Z ..]  ->  [Z Y X ..]",
                "Reverses a list on top of the stack.");

        internal static Builtin Strtoi(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Strtof(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Fopen(Action op) =>
            new Builtin(
                op,
                "->  IO");

        internal static Builtin Fclose(Action op) =>
            new Builtin(
                op,
                "IO  ->");

        internal static Builtin Freads(Action op) =>
            new Builtin(
                op,
                "IO  ->  IO S");

        internal static Builtin Put(Action op) =>
            new Builtin(
                op,
                "");

        internal static Builtin Puts(Action op) =>
            new Builtin(
                op,
                "");
    }
}