namespace Xil
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
                .Match(Numerics.DecimalDouble, TokenKind.Number)
                .Match(Character.EqualTo(':'), TokenKind.Colon)
                .Match(Character.EqualTo(';'), TokenKind.Semicolon)
                .Match(Character.EqualTo('.'), TokenKind.Dot)
                .Match(Character.EqualTo('{'), TokenKind.OpenBrace)
                .Match(Character.EqualTo('}'), TokenKind.CloseBrace)
                .Match(Character.EqualTo('['), TokenKind.OpenBracket)
                .Match(Character.EqualTo(']'), TokenKind.CloseBracket)
                .Match(
                    Span.EqualTo("true"),
                    TokenKind.True,
                    requireDelimiters: true)
                .Match(
                    Span.EqualTo("false"),
                    TokenKind.False,
                    requireDelimiters: true)
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
            from @start in Token.EqualTo(TokenKind.OpenBracket)
            from factors in Parse.Ref(() => Factor).Many()
            from @end in Token.EqualTo(TokenKind.CloseBracket)
            select CreateList(factors);

        public static readonly TokenListParser<TokenKind, Value> Set =
            from @start in Token.EqualTo(TokenKind.OpenBrace)
            from factors in Parse.Ref(() => Factor).Many()
            from @end in Token.EqualTo(TokenKind.CloseBrace)
            select CreateSet(factors);

        public static readonly TokenListParser<TokenKind, Value> Factor =
            List
                .Or(Number)
                .Or(String)
                .Or(True)
                .Or(False)
                .Or(Symbol);

        public static readonly TokenListParser<TokenKind, Value[]> Term =
            Factor.Many();

        public static readonly TokenListParser<TokenKind, Value.Definition> Def =
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

            var f = double.Parse(s, CultureInfo.InvariantCulture);
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

        private static Value CreateSet(IValue[] factors)
        {
            throw new NotImplementedException();
        }

        private static Value.Definition CreateUsr(string id, IValue[] factors) =>
            new Value.Definition(id, factors);
    }
}