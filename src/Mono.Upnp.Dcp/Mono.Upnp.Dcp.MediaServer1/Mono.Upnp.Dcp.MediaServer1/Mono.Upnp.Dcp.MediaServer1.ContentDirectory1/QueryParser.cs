// 
// QueryParser.cs
//  
// Author:
//       Scott Thomas <lunchtimemama@gmail.com>
// 
// Copyright (c) 2010 Scott Thomas
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Text;

namespace Mono.Upnp.Dcp.MediaServer1.ContentDirectory1
{
    // Refer to ContentDirectory1 Service Template 1.0.1, Section 2.5.5.1: Search Criteria String Syntax
    public abstract class QueryParser
    {
        delegate TResult Func<T, TResult> (T argument);
        delegate TResult Func<T1, T2, TResult> (T1 argument1, T2 argument2);

        protected abstract QueryParser OnCharacter (char character);

        protected abstract Query OnDone ();

        protected static bool IsWhiteSpace (char character)
        {
            switch (character) {
            case ' ': return true;
            case '\t': return true;
            case '\n': return true;
            case '\v': return true;
            case '\f': return true;
            case '\r': return true;
            default: return false;
            }
        }

        class RootQueryParser : QueryParser
        {
            const string wild_card_error_message = "The wildcard must be used alone.";

            int parentheses;

            public RootQueryParser ()
            {
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (IsWhiteSpace (character)) {
                    return this;
                } else if (character == '*') {
                    if (parentheses == -1) {
                        throw new QueryParsingException (wild_card_error_message);
                    } else {
                        parentheses = -1;
                        return this;
                    }
                } else if (character == '(') {
                    if (parentheses == -1) {
                        throw new QueryParsingException (wild_card_error_message);
                    } else {
                        parentheses++;
                        return this;
                    }
                } else if (character == ')') {
                    if (parentheses > 0) {
                        throw new QueryParsingException ("Empty expressions are not allowed.");
                    } else {
                        throw new QueryParsingException ("The parentheses are unbalanced.");
                    }
                } else {
                    if (parentheses == -1) {
                        throw new QueryParsingException (wild_card_error_message);
                    } else {
                        return new PropertyParser (token => new RootPropertyOperatorParser (token,
                            expression => new ExpressionParser (expression, parentheses))).OnCharacter (character);
                    }
                }
            }

            protected override Query OnDone ()
            {
                if (parentheses == -1) {
                    return visitor => visitor.VisitAllResults ();
                } else {
                    throw new QueryParsingException ("The query is empty.");
                }
            }
        }

        class ExpressionParser : QueryParser
        {
            const int disjunction_priority = 1;
            const int conjunction_priority = 2;
            const int parenthetical_priority = 3;

            protected readonly Query Expression;
            protected int Parentheses;

            public ExpressionParser (Query expression, int parentheses)
            {
                Expression = expression;
                Parentheses = parentheses;
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (IsWhiteSpace (character)) {
                    return this;
                } else if (character == '(') {
                    Parentheses++;
                    return this;
                } else if (character == ')') {
                    Parentheses--;
                    if (Parentheses < 0) {
                        throw new QueryParsingException ("The parentheses are unbalanced.");
                    } else {
                        return this;
                    }
                } else if (character == 'a') {
                    var priority = GetPriority (conjunction_priority);
                    return new ConjunctionParser (Parentheses, priority, MakeHandler (priority,
                        (leftOperand, rightOperand) => visitor => visitor.VisitAnd (leftOperand, rightOperand)));
                } else if (character == 'o') {
                    var priority = GetPriority (disjunction_priority);
                    return new DisjunctionParser (Parentheses, priority, MakeHandler (priority,
                        (leftOperand, rightOperand) => visitor => visitor.VisitOr (leftOperand, rightOperand)));
                } else {
                    throw new QueryParsingException (string.Format ("Unexpected operator begining: {0}.", character));
                }
            }

            int GetPriority (int priority)
            {
                if (Parentheses > 0) {
                    return parenthetical_priority + Parentheses + priority;
                } else {
                    return priority;
                }
            }

            protected virtual Func<int, Func<Query, Query>, Func<Query, Query>> MakeHandler (int priority,
                                                                                             Func<Query, Query, Query> binaryOperator)
            {
                return (priorPriority, priorOperator) => {
                    if (priorPriority < priority) {
                        return priorOperand => priorOperator (binaryOperator (Expression, priorOperand));
                    } else {
                        return priorOperand => binaryOperator (Expression, priorOperator (priorOperand));
                    }
                };
            }

            protected override Query OnDone ()
            {
                if (Parentheses == 0) {
                    return Expression;
                } else {
                    throw new QueryParsingException ("The parentheses are unbalanced.");
                }
            }
        }

        class JoinedExpressionParser : ExpressionParser
        {
            readonly Func<int, Func<Query, Query>, Func<Query, Query>> previous_handler;
            readonly int priority;

            public JoinedExpressionParser (Query expression,
                                           int parentheses,
                                           int priority,
                                           Func<int, Func<Query, Query>, Func<Query, Query>> previousHandler)
                : base (expression, parentheses)
            {
                this.previous_handler = previousHandler;
                this.priority = priority;
            }

            protected override Func<int, Func<Query, Query>, Func<Query, Query>> MakeHandler (int priority,
                                                                                              Func<Query, Query, Query> binaryOperator)
            {
                // Even I admit this is unreadable. But is a very slick operator priority algorithm.
                if (this.priority < priority) {
                    return (priorPriority, priorOperator) => {
                        if (this.priority < priorPriority) {
                            return priorOperand => previous_handler (this.priority, operand => operand) (
                                priorOperator (binaryOperator (Expression, priorOperand)));
                        } else {
                            return priorOperand => previous_handler (
                                priorPriority, operand => priorOperator (operand)) (
                                binaryOperator (Expression, priorOperand));
                        }
                    };
                } else {
                    return (priorPriority, priorOperator) => priorOperand => priorOperator (previous_handler (
                        priorPriority, operand => binaryOperator (operand, priorOperand)) (Expression));
                }
            }

            protected override Query OnDone ()
            {
                return previous_handler (priority, operand => operand) (Expression);
            }
        }

        abstract class JunctionParser : QueryParser
        {
            readonly Func<int, Func<Query, Query>, Func<Query, Query>> previous_handler;
            readonly int priority;
            int parentheses;

            protected JunctionParser (int parentheses,
                                      int priority,
                                      Func<int, Func<Query, Query>, Func<Query, Query>> previousHandler)
            {
                this.parentheses = parentheses;
                this.priority = priority;
                this.previous_handler = previousHandler;
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (IsWhiteSpace (character)) {
                    return this;
                } else if (character == '(') {
                    parentheses++;
                    return this;
                } else if (character == ')') {
                    return Fail<QueryParser> ();
                } else {
                    return new PropertyParser (token => new RootPropertyOperatorParser (
                        token, expression => new JoinedExpressionParser (
                            expression, parentheses, priority, previous_handler))).OnCharacter (character);
                }
            }

            protected override Query OnDone ()
            {
                return Fail<Query> ();
            }

            T Fail<T> ()
            {
                throw new QueryParsingException (string.Format (
                    "Expecting an expression after the {0}.", Junction));
            }

            protected abstract string Junction { get; }
        }

        class ConjunctionParser : JunctionParser
        {
            const int a_state = 0;
            const int n_state = 1;
            const int d_state = 2;

            int state;

            public ConjunctionParser (int parentheses,
                                      int priority,
                                      Func<int, Func<Query, Query>, Func<Query, Query>> previousHandler)
                : base (parentheses, priority, previousHandler)
            {
            }

            protected override QueryParser OnCharacter (char character)
            {
                switch (state) {
                case a_state:
                    if (character == 'n') {
                        state++;
                        return this;
                    } else if (IsWhiteSpace (character)) {
                        throw new QueryParsingException ("Unexpected operator: a.");
                    } else {
                        throw new QueryParsingException (string.Format (
                            "Unexpected operator begining: a{0}.", character));
                    }
                case n_state:
                    if (character == 'd') {
                        state++;
                        return this;
                    } else if (IsWhiteSpace (character)) {
                        throw new QueryParsingException ("Unexpected operator: an.");
                    } else {
                        throw new QueryParsingException (string.Format (
                            "Unexpected operator begining: an{0}.", character));
                    }
                case d_state:
                    if (IsWhiteSpace (character)) {
                        state++;
                        return this;
                    } else {
                        throw new QueryParsingException (string.Format (
                            "Unexpected operator begining: and{0}.", character));
                    }
                default:
                    return base.OnCharacter (character);
                }
            }

            protected override Query OnDone ()
            {
                if (state < d_state) {
                    throw new QueryParsingException (string.Format (
                        "Unexpected operator: {0}.", "and".Substring (0, state + 1)));
                } else {
                    return base.OnDone ();
                }
            }

            protected override string Junction {
                get { return "conjunction"; }
            }
        }

        class DisjunctionParser : JunctionParser
        {
            const int o_state = 0;
            const int r_state = 1;

            int state;

            public DisjunctionParser (int parentheses,
                                      int priority,
                                      Func<int, Func<Query, Query>, Func<Query, Query>> previousHandler)
                : base (parentheses, priority, previousHandler)
            {
            }

            protected override QueryParser OnCharacter (char character)
            {
                switch (state) {
                case o_state:
                    if (character == 'r') {
                        state++;
                        return this;
                    } else if (IsWhiteSpace (character)) {
                        throw new QueryParsingException ("Unexpected operator: o.");
                    } else {
                        throw new QueryParsingException (string.Format (
                            "Unexpected operator begining: o{0}.", character));
                    }
                case r_state:
                    if (IsWhiteSpace (character)) {
                        state++;
                        return this;
                    } else {
                        throw new QueryParsingException (string.Format (
                            "Unexpected operator begining: or{0}.", character));
                    }
                default:
                    return base.OnCharacter (character);
                }
            }

            protected override Query OnDone ()
            {
                if (state < r_state) {
                    throw new QueryParsingException ("Unexpected operator: o.");
                } else {
                    return base.OnDone ();
                }
            }

            protected override string Junction {
                get { return "disjunction"; }
            }
        }

        abstract class PropertyOperatorParser : QueryParser
        {
            protected readonly string Property;
            protected readonly Func<Query, QueryParser> Consumer;

            protected PropertyOperatorParser (string property, Func<Query, QueryParser> consumer)
            {
                Property = property;
                Consumer = consumer;
            }
        }

        class RootPropertyOperatorParser : PropertyOperatorParser
        {
            public RootPropertyOperatorParser (string property, Func<Query, QueryParser> consumer)
                : base (property, consumer)
            {
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (IsWhiteSpace (character)) {
                    return this;
                }

                switch (character) {
                case '=': return new EqualityParser (Property, Consumer);
                case '!': return new InequalityParser (Property, Consumer);
                case '<': return new LessThanParser (Property, Consumer);
                case '>': return new GreaterThanParser (Property, Consumer);
                case 'c': return new ContainsParser (Property, Consumer).OnCharacter ('c');
                case 'e': return new ExistsParser (Property, Consumer).OnCharacter ('e');
                case 'd': return new DerivedFromOrDoesNotContainParser (Property, Consumer);
                default: throw new QueryParsingException (string.Format (
                    "Unexpected operator begining: {0}.", character));
                }
            }

            protected override Query OnDone ()
            {
                throw new QueryParsingException (string.Format (
                    "No operator is applied to the property identifier: {0}.", Property));
            }
        }

        abstract class TokenOperatorParser : PropertyOperatorParser
        {
            protected bool Initialized;

            protected TokenOperatorParser (string property, Func<Query, QueryParser> consumer)
                : base (property, consumer)
            {
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (Initialized) {
                    if (IsWhiteSpace (character)) {
                        return this;
                    } else {
                        return GetOperandParser ().OnCharacter (character);
                    }
                } else if (IsWhiteSpace (character)) {
                    Initialized = true;
                    return this;
                } else {
                    throw new QueryParsingException (string.Format (
                        "Whitespace is required around the operator: {0}.", Operator));
                }
            }

            protected abstract QueryParser GetOperandParser ();

            protected abstract string Operator { get; }

            protected override Query OnDone ()
            {
                throw new QueryParsingException (string.Format (
                    "There is no operand for the operator: {0}.", Operator));
            }
        }

        abstract class StringOperatorParser : TokenOperatorParser
        {
            readonly string @operator;
            int position;

            protected StringOperatorParser (string @operator, string property, Func<Query, QueryParser> consumer)
                : base (property, consumer)
            {
                this.@operator = @operator;
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (Initialized) {
                    return base.OnCharacter (character);
                } else if (position == @operator.Length) {
                    if (IsWhiteSpace (character)) {
                        return base.OnCharacter (character);
                    } else {
                        throw new QueryParsingException (string.Format (
                            "Unexpected operator begining: {0}{1}.", @operator, character));
                    }
                } else if (character != @operator[position]) {
                    if (IsWhiteSpace (character)) {
                        throw new QueryParsingException (string.Format (
                            "Unexpected operator: {0}.", @operator.Substring (0, position)));
                    } else {
                        throw new QueryParsingException (string.Format (
                            "Unexpected operator begining: {0}{1}.", @operator.Substring (0, position), character));
                    }
                } else {
                    position++;
                    return this;
                }
            }

            protected override string Operator {
                get { return @operator; }
            }
        }

        class ContainsParser : StringOperatorParser
        {
            public ContainsParser (string property, Func<Query, QueryParser> consumer)
                : base ("contains", property, consumer)
            {
            }

            protected override QueryParser GetOperandParser ()
            {
                return new StringParser (Operator,
                    value => Consumer (visitor => visitor.VisitContains (Property, value)));
            }
        }

        class ExistsParser : StringOperatorParser
        {
            public ExistsParser (string property, Func<Query, QueryParser> consumer)
                : base ("exists", property, consumer)
            {
            }

            protected override QueryParser GetOperandParser ()
            {
                return new BooleanParser (value => Consumer (visitor => visitor.VisitExists (Property, value)));
            }
        }

        abstract class EqualityOperatorParser : TokenOperatorParser
        {
            protected bool HasEqualsSign;

            protected EqualityOperatorParser (string property, Func<Query, QueryParser> consumer)
                : base (property, consumer)
            {
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (HasEqualsSign || character != '=') {
                    return base.OnCharacter (character);
                } else {
                    HasEqualsSign = true;
                    return this;
                }
            }
        }

        class DerivedFromOrDoesNotContainParser : PropertyOperatorParser
        {
            public DerivedFromOrDoesNotContainParser (string property, Func<Query, QueryParser> consumer)
                : base (property, consumer)
            {
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (character == 'e') {
                    return new DerivedFromParser (Property, Consumer).OnCharacter ('d').OnCharacter ('e');
                } else  if (character == 'o') {
                    return new DoesNotContainParser (Property, Consumer).OnCharacter ('d').OnCharacter ('o');
                } else if (IsWhiteSpace (character)) {
                    throw new QueryParsingException ("Unexpected operator: d.");
                } else {
                    throw new QueryParsingException (string.Format (
                        "Unexpected operator begining: d{0}.", character));
                }
            }

            protected override Query OnDone ()
            {
                throw new QueryParsingException ("Unexpected operator begining: d.");
            }
        }

        class DerivedFromParser : StringOperatorParser
        {
            public DerivedFromParser (string property, Func<Query, QueryParser> consumer)
                : base ("derivedFrom", property, consumer)
            {
            }

            protected override QueryParser GetOperandParser ()
            {
                return new StringParser (Operator,
                    value => Consumer (visitor => visitor.VisitDerivedFrom (Property, value)));
            }
        }

        class DoesNotContainParser : StringOperatorParser
        {
            public DoesNotContainParser (string property, Func<Query, QueryParser> consumer)
                : base ("doesNotContain", property, consumer)
            {
            }

            protected override QueryParser GetOperandParser ()
            {
                return new StringParser (Operator,
                    value => Consumer (visitor => visitor.VisitDoesNotContain (Property, value)));
            }
        }

        class EqualityParser : TokenOperatorParser
        {
            public EqualityParser (string property, Func<Query, QueryParser> consumer)
                : base (property, consumer)
            {
            }

            protected override QueryParser GetOperandParser ()
            {
                return new StringParser (Operator,
                    value => Consumer (visitor => visitor.VisitEquals (Property, value)));
            }

            protected override string Operator {
                get { return "="; }
            }
        }

        class InequalityParser : EqualityOperatorParser
        {
            public InequalityParser (string property, Func<Query, QueryParser> consumer)
                : base (property, consumer)
            {
            }

            protected override QueryParser OnCharacter (char character)
            {
                var parser = base.OnCharacter (character);
                if (Initialized && !HasEqualsSign) {
                    throw new QueryParsingException ("Incomplete operator: !=.");
                }
                return parser;
            }

            protected override QueryParser GetOperandParser ()
            {
                return new StringParser (Operator,
                    value => Consumer (visitor => visitor.VisitDoesNotEqual (Property, value)));
            }

            protected override string Operator {
                get { return "!="; }
            }
        }

        class LessThanParser : EqualityOperatorParser
        {
            public LessThanParser (string property, Func<Query, QueryParser> consumer)
                : base (property, consumer)
            {
            }

            protected override QueryParser GetOperandParser ()
            {
                if (HasEqualsSign) {
                    return new StringParser (Operator,
                        value => Consumer (visitor => visitor.VisitLessThanOrEqualTo (Property, value)));
                } else {
                    return new StringParser (Operator,
                        value => Consumer (visitor => visitor.VisitLessThan (Property, value)));
                }
            }

            protected override string Operator {
                get { return HasEqualsSign ? "<=" : "<"; }
            }
        }

        class GreaterThanParser : EqualityOperatorParser
        {
            public GreaterThanParser (string property, Func<Query, QueryParser> consumer)
                : base (property, consumer)
            {
            }

            protected override QueryParser GetOperandParser ()
            {
                if (HasEqualsSign) {
                    return new StringParser (Operator,
                        value => Consumer (visitor => visitor.VisitGreaterThanOrEqualTo (Property, value)));
                } else {
                    return new StringParser (Operator,
                        value => Consumer (visitor => visitor.VisitGreaterThan (Property, value)));
                }
            }

            protected override string Operator {
                get { return HasEqualsSign ? ">=" : ">"; }
            }
        }

        class PropertyParser : QueryParser
        {
            readonly Func<string, QueryParser> consumer;
            StringBuilder builder = new StringBuilder ();

            public PropertyParser (Func<string, QueryParser> consumer)
            {
                this.consumer = consumer;
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (IsWhiteSpace (character)) {
                    return consumer (builder.ToString ());
                } else {
                    builder.Append (character);
                    return this;
                }
            }

            protected override Query OnDone ()
            {
                throw new QueryParsingException (string.Format (
                    @"The property identifier is not a part of an expression: {0}.", builder.ToString ()));
            }
        }

        class StringParser : QueryParser
        {
            readonly string @operator;
            readonly Func<string, QueryParser> consumer;
            StringBuilder builder;
            bool escaped;

            public StringParser (string @operator, Func<string, QueryParser> consumer)
            {
                this.@operator = @operator;
                this.consumer = consumer;
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (builder == null) {
                    if (character == '"') {
                        builder = new StringBuilder ();
                        return this;
                    } else {
                        throw new QueryParsingException (string.Format (
                            "Expecting double-quoted string operand with the operator: {0}.", @operator));
                    }
                } else if (escaped) {
                    if (character == '\\') {
                        builder.Append ('\\');
                    } else  if (character == '"') {
                        builder.Append ('"');
                    } else {
                        throw new QueryParsingException (string.Format (
                            "Unexpected escape sequence: \\{0}.", character));
                    }
                    escaped = false;
                    return this;
                } else if (character == '\\') {
                    escaped = true;
                    return this;
                } else if (character == '"') {
                    return consumer (builder.ToString ());
                } else {
                    builder.Append (character);
                    return this;
                }
            }

            protected override Query OnDone ()
            {
                throw new QueryParsingException (string.Format (
                    @"The double-quoted string is not terminated: ""{0}"".", builder.ToString ()));
            }
        }

        class BooleanParser : QueryParser
        {
            readonly Func<bool, QueryParser> consumer;
            bool @true;
            int position;

            public BooleanParser (Func<bool, QueryParser> consumer)
            {
                this.consumer = consumer;
            }

            protected override QueryParser OnCharacter (char character)
            {
                if (position == 0) {
                    if (IsWhiteSpace (character)) {
                        return this;
                    } else if (character == 't') {
                        @true = true;
                        return Check ("true", character);
                    } else if (character == 'f') {
                        return Check ("false", character);
                    } else {
                        return Fail<QueryParser> ();
                    }
                } else if (@true) {
                    return Check ("true", character);
                } else {
                    return Check ("false", character);
                }
            }

            protected override Query OnDone ()
            {
                if (@true) {
                    if (position == "true".Length) {
                        return consumer (true).OnDone ();
                    } else {
                        return Fail<Query> ();
                    }
                } else {
                    if (position == "false".Length) {
                        return consumer (false).OnDone ();
                    } else {
                        return Fail<Query> ();
                    }
                }
            }

            QueryParser Check (string expected, char character)
            {
                if (position == expected.Length) {
                    if (IsWhiteSpace (character)) {
                        return consumer (@true);
                    } else if (character == ')' || character == '(') {
                        return consumer (@true).OnCharacter (character);
                    } else {
                        return Fail<QueryParser> ();
                    }
                } else if (expected[position] == character) {
                    position++;
                    return this;
                } else {
                    return Fail<QueryParser> ();
                }
            }

            T Fail<T> ()
            {
                throw new QueryParsingException (@"Expecting either ""true"" or ""false"".");
            }
        }

        public static Query Parse (string query)
        {
            QueryParser parser = new RootQueryParser ();

            foreach (var character in query) {
                parser = parser.OnCharacter (character);
            }

            return parser.OnDone ();
        }
    }
}