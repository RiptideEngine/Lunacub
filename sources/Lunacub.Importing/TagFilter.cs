using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Importing;

internal readonly struct TagFilter {
    public readonly string Query;
    public readonly Expression RootExpression;
    
    public TagFilter(string query) {
        Query = query;
        RootExpression = ParseQuery();
    }
    
    private Expression ParseQuery() {
        // Lexing
        int current = 0;
        List<Token> tokens = [];

        while (current < Query.Length) {
            int start = current;
            char currentCharacter = Query[current++];

            switch (currentCharacter) {
                case '(': tokens.Add(new(TokenType.LeftParenthesis, start..current)); break;
                case ')': tokens.Add(new(TokenType.RightParenthesis, start..current)); break;
                case '&': tokens.Add(new(TokenType.And, start..current)); break;
                case '|': tokens.Add(new(TokenType.Or, start..current)); break;
                case '^': tokens.Add(new(TokenType.Xor, start..current)); break;
                case '!' or '~' or '-': tokens.Add(new(TokenType.Not, start..current)); break;
                case ' ' or '\t' or '\r' or '\n': break;
                default:
                    if (IsTagCharacter(currentCharacter)) {
                        while (current < Query.Length && IsTagCharacter(Query[current])) {
                            current++;
                        }
                        
                        tokens.Add(new(TokenType.Tag, start..current));
                        break;
                    }

                    throw new ArgumentException($"Unexpected character '{currentCharacter}' at position {current - 1}.");
            }
        }
        
        // Scanning
        return new Parser(tokens).Expression();

        static bool IsTagCharacter(char character) {
            return char.IsAsciiLetterOrDigit(character) || character is '_';
        }
    }

    public bool Check(ImmutableArray<string> tags) {
        return RootExpression.Check(tags, Query);
    }

    public enum TokenType {
        Tag,
        
        LeftParenthesis, RightParenthesis,
        And, Or, Not, Xor,
    }

    public readonly record struct Token(TokenType Type, Range Range);

    public abstract class Expression {
        public abstract bool Check(ImmutableArray<string> tags, string query);
    }

    public sealed class LiteralExpression : Expression {
        public readonly Range ValueRange;

        public LiteralExpression(Range valueRange) {
            ValueRange = valueRange;
        }
        
        public override bool Check(ImmutableArray<string> tags, string query) {
            ReadOnlySpan<char> searchingTag = query.AsSpan(ValueRange);
            
            foreach (var tag in tags) {
                if (searchingTag.SequenceEqual(tag)) {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class UnaryExpression : Expression {
        public readonly TokenType Operator;
        public readonly Expression Expression;

        public UnaryExpression(TokenType @operator, Expression expression) {
            Operator = @operator;
            Expression = expression;
        }
        
        public override bool Check(ImmutableArray<string> tags, string query) {
            switch (Operator) {
                case TokenType.Not: return !Expression.Check(tags, query);
                default: throw new UnreachableException();
            }
        }
    }

    public sealed class BinaryExpression : Expression {
        public readonly Expression Left;
        public readonly TokenType Operator;
        public readonly Expression Right;

        public BinaryExpression(Expression left, TokenType @operator, Expression right) {
            Left = left;
            Operator = @operator;
            Right = right;
        }
        
        public override bool Check(ImmutableArray<string> tags, string query) {
            switch (Operator) {
                case TokenType.And: return Left.Check(tags, query) && Right.Check(tags, query);
                case TokenType.Or: return Left.Check(tags, query) || Right.Check(tags, query);
                case TokenType.Xor: return Left.Check(tags, query) ^ Right.Check(tags, query);
                default: throw new UnreachableException();
            }
        }
    }

    private struct Parser {
        private readonly List<Token> _tokens;
        private int _current;
        
        public Parser(List<Token> tokens) {
            _tokens = tokens;
        }

        public Expression Expression() {
            return Or();
        }

        private Expression Or() {
            Expression expr = Xor();

            while (Match(TokenType.Or)) {
                Token @operator = Previous();
                Expression right = Xor();
                expr = new BinaryExpression(expr, @operator.Type, right);
            }

            return expr;
        }

        private Expression Xor() {
            Expression expr = And();

            while (Match(TokenType.Xor)) {
                Token @operator = Previous();
                Expression right = And();
                expr = new BinaryExpression(expr, @operator.Type, right);
            }

            return expr;
        }

        private Expression And() {
            Expression expr = Not();

            while (Match(TokenType.And)) {
                Token @operator = Previous();
                Expression right = Not();
                expr = new BinaryExpression(expr, @operator.Type, right);
            }

            return expr;
        }

        private Expression Not() {
            if (Match(TokenType.Not)) {
                Token @operator = Previous();
                Expression right = Not();
                return new UnaryExpression(@operator.Type, right);
            }

            return Primary();
        }

        private Expression Primary() {
            if (Match(TokenType.Tag)) {
                return new LiteralExpression(Previous().Range);
            }

            if (Match(TokenType.LeftParenthesis)) {
                Expression expr = Expression();

                if (!Match(TokenType.RightParenthesis)) {
                    throw new ArgumentException("Expected ')' after expression.");
                }

                return expr;
            }

            throw new UnreachableException($"Unknown token type '{_tokens[_current].Type}'.");
        }

        private bool Match(TokenType type) {
            if (_current >= _tokens.Count) return false;
            if (_tokens[_current].Type != type) return false;

            _current++;
            return true;
        }
        
        private Token Previous() => _tokens[_current - 1];
    }
}