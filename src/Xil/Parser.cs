namespace Xil
{
    using System.Collections.Generic;
    using System.Linq;
    using Superpower;
    using Superpower.Model;
    using Superpower.Parsers;
    using Superpower.Tokenizers;

    public class Parser
    {
        public static readonly char[] Reserved = new[]
        {
             default(char),
             ':',
             ';',
             '.',
             '{',
             '}',
             '[',
             ']',
             ' ',
             '\n',
             '\r',
             '\t',
        };

        public static readonly IDictionary<string, char> EscapedChars =
            new Dictionary<string, char>
            {
                ["'\\b"] = '\b',
                ["'\\f"] = '\f',
                ["'\\n"] = '\n',
                ["'\\t"] = '\t',
                ["'\\r"] = '\r',
                ["'\\v"] = '\v',
            };

        public static readonly Tokenizer<TokenKind> Tokenizer =
            new TokenizerBuilder<TokenKind>()
                .Ignore(Character.WhiteSpace)
                .Match(Character.EqualTo(':'), TokenKind.Colon)
                .Match(Character.EqualTo(';'), TokenKind.Semicolon)
                .Match(Character.EqualTo('.'), TokenKind.Dot)
                .Match(Character.EqualTo('{'), TokenKind.LBrace)
                .Match(Character.EqualTo('}'), TokenKind.RBrace)
                .Match(Character.EqualTo('['), TokenKind.LBrack)
                .Match(Character.EqualTo(']'), TokenKind.RBrack)
                .Match(
                    Span.EqualTo("true"),
                    TokenKind.True,
                    requireDelimiters: true)
                .Match(
                    Span.EqualTo("false"),
                    TokenKind.False,
                    requireDelimiters: true)
                .Match(Numerics.DecimalDouble, TokenKind.Number)
                .Match(Parse.Ref(() => StringToken), TokenKind.String)
                .Match(
                    Character.ExceptIn(Reserved).AtLeastOnce(),
                    TokenKind.Symbol,
                    requireDelimiters: true)
                .Build();

        public static readonly TokenListParser<TokenKind, Value> Number =
            Token.EqualTo(TokenKind.Number).Select(CreateNumber);

        public static readonly TokenListParser<TokenKind, Value> Symbol =
            Token.EqualTo(TokenKind.Symbol).Select(CreateSymbol);

        public static readonly TokenListParser<TokenKind, Value> String =
            Token.EqualTo(TokenKind.String).Select(CreateString);

        public static readonly TokenListParser<TokenKind, Value> True =
            Token.EqualTo(TokenKind.True).Select(CreateBool);

        public static readonly TokenListParser<TokenKind, Value> False =
            Token.EqualTo(TokenKind.False).Select(CreateBool);

        public static readonly TokenListParser<TokenKind, Value> List =
            from @start in Token.EqualTo(TokenKind.LBrack)
            from factors in Parse.Ref(() => Factor).Many()
            from @end in Token.EqualTo(TokenKind.RBrack)
            select CreateList(factors);

        public static readonly TokenListParser<TokenKind, Value> Factor =
            List
                .Or(Number)
                .Or(String)
                .Or(True)
                .Or(False)
                .Or(Symbol);

        public static readonly TokenListParser<TokenKind, Value[]> Term =
            Factor.Many();

        public static readonly TokenListParser<TokenKind, Value.Usr> Def =
             from @start in Token.EqualTo(TokenKind.Colon)
             from id in Token.EqualTo(TokenKind.Symbol)
             from factors in Parse.Ref(() => Factor).Many()
             from end in Token.EqualTo(TokenKind.Semicolon)
             select CreateUsr(id.ToStringValue(), factors);

        private static readonly TextParser<Unit> StringToken =
            from open in Character.EqualTo('"')
            from content in Span.EqualTo("\\\"").Value(Unit.Value).Try()
                .Or(Character.Except('"').Value(Unit.Value))
                .IgnoreMany()
            from close in Character.EqualTo('"')
            select Unit.Value;
            
        private static Value CreateBool(Token<TokenKind> token) =>
            new Value.Bool(bool.Parse(token.ToStringValue()));

        private static Value CreateNumber(Token<TokenKind> token)
        {
            var s = token.ToStringValue();
            if (int.TryParse(s, out var i))
            {
                return new Value.Int(i);
            }

            var f = double.Parse(s);
            return new Value.Float(f);
        }

        private static Value CreateSymbol(Token<TokenKind> token)
        {
            var s = token.ToStringValue();
            if (s.StartsWith('\''))
            {
                if (EscapedChars.TryGetValue(s, out var esc))
                {
                    return new Value.Char(esc);
                }

                if (s.Length == 2)
                {
                    return new Value.Char(s[1]);
                }
            }

            return new Value.Symbol(token.ToStringValue());
        }

        private static Value CreateString(Token<TokenKind> token)
        {
            var s = token.ToStringValue().Trim('"');
            return (Value)new Value.String(s);
        }

        private static Value CreateList(Value[] factors) =>
            new Value.List(factors);

        private static Value.Usr CreateUsr(string id, IValue[] factors) =>
            new Value.Usr(id, factors);
    }
}