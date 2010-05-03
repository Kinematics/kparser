using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Monad
{
    // Type definition to specify location within the parse stream.
    // In this case, it represents the index of the string/token
    // within the List<string> or List<Token> object.
    using Position = System.Int32;

    /// <summary>
    /// Class representing the current state of the input.
    /// </summary>
    /// <typeparam name="TType">The type of token/object the state list contains.</typeparam>
    internal class ParserState<TInput>
    {
        internal readonly Position Position;
        internal readonly List<TInput> Input;

        internal bool EOF
        {
            get { return (Position >= Input.Count); }
        }

        internal ParserState(Position position, List<TInput> input)
        {
            this.Position = position;
            this.Input = input;
        }

        public override string ToString()
        {
            // place a '^' before the current location in the string; this is helpful when looking at
            // values inside the debugger
            return this.Input.Take(this.Position).ToString() + "^" +
                   this.Input.Skip(this.Position).ToString();
        }
    }

    internal class ErrorInfo
    {
        internal Position Position;
        internal IEnumerable<string> Expectations;
        internal string Message;

        internal ErrorInfo(Position position) : this(position, Enumerable.Empty<string>(), "unknown error") { }

        internal ErrorInfo(Position position, IEnumerable<string> expectations, string message)
        {
            this.Position = position; this.Expectations = expectations; this.Message = message;
        }

        internal ErrorInfo Merge(ErrorInfo other)
        {
            // Note that this.Position == other.Position holds true, except when one is a failed Try that looked ahead.
            // Since the most common idiom is Try(p1).Or(p2), we favor keeping position2 and message2, as p2 is 
            // likely to consume and thus be the only contributor to the final expectations.
            // But if Try(p1).Or(Try(p2)) fails, and both alternatives looked ahead different number of tokens, then
            // a single error message is bound to be confusing, reporting some wrong position or details.
            // This is an intrinsic difficulty when it comes to reporting errors for languages that do not admit 
            // predictive parsers, and it illustrates why Try() and backtracking should be used judiciously.
            return new ErrorInfo(other.Position, this.Expectations.Concat(other.Expectations), other.Message);
        }

        internal ErrorInfo SetExpectation(string label)
        {
            if (string.IsNullOrEmpty(label))
                return new ErrorInfo(this.Position, Enumerable.Empty<string>(), this.Message);
            else
                return new ErrorInfo(this.Position, new List<string>() {label}, this.Message);
        }

        public override string ToString()
        {
            List<string> expectations = this.Expectations.ToList();
            StringBuilder result = new StringBuilder(
                string.Format("At position {0}, {1}", this.Position, this.Message));

            if (expectations.Count != 0)
            {
                result.Append(", ");
                if (expectations.Count == 1)
                {
                    result.Append("expected " + expectations[0]);
                }
                else
                {
                    result.Append("expected ");
                    for (int i = 0; i < expectations.Count - 2; ++i)
                    {
                        result.Append(expectations[i]);
                        result.Append(", ");
                    }
                    result.Append(expectations[expectations.Count - 2]);
                    result.Append(" or ");
                    result.Append(expectations[expectations.Count - 1]);
                }
            }
            return result.ToString();
        }
    }

    /// <summary>
    /// The output result for the parser.
    /// Works on lists of type Token.
    /// </summary>
    /// <typeparam name="TInput">The type of token/object used to represent the input.
    /// <typeparam name="TValue">The type of result stored.</typeparam>
    /// The type of parser state.</typeparam>
    internal class ParseResult<TInput, TValue>
    {
        internal bool Succeeded;

        // if Succeeded, all fields below are used, else only ErrorInfo is used
        internal ErrorInfo ErrorInfo;

        readonly TValue result;
        readonly ParserState<TInput> remainingInput;

        #region Properties and returned values
        internal TValue Result
        {
            get { if (!this.Succeeded) throw new InvalidOperationException(); return this.result; }
        }

        internal ParserState<TInput> RemainingInput
        {
            get { if (!this.Succeeded) throw new InvalidOperationException(); return this.remainingInput; }
        }

        internal List<TInput> ConsumedInputSince(ParseResult<TInput, TValue> previousParseResult)
        {
            return ConsumedInputSince(previousParseResult.remainingInput);
        }

        internal List<TInput> ConsumedInputSince(ParserState<TInput> previousState)
        {
            if (!this.Succeeded)
                throw new InvalidOperationException();

            if (this.remainingInput.Input != previousState.Input)
                throw new InvalidOperationException();

            int consumedItems = this.remainingInput.Position - previousState.Position;

            if (consumedItems == 0)
                throw new InvalidOperationException();

            List<TInput> consumed = this.remainingInput.Input
                .Skip(previousState.Position)
                .Take(consumedItems).ToList<TInput>();

            return consumed;
        }
        #endregion

        #region Constructors
        internal ParseResult(TValue result, ParserState<TInput> remainingInput)
        {
            this.result = result;
            this.remainingInput = remainingInput;
        }

        internal ParseResult(TValue result, ParserState<TInput> remainingInput, ErrorInfo errorInfo)
        {
            this.result = result;
            this.remainingInput = remainingInput;
            this.ErrorInfo = errorInfo;
            this.Succeeded = true;
        }

        internal ParseResult(ErrorInfo errorInfo)
        {
            this.ErrorInfo = errorInfo;
        }
        #endregion

        #region Error handling and overrides
        internal ParseResult<TInput, TValue> MergeError(ErrorInfo otherError)
        {
            if (this.Succeeded)
                return new ParseResult<TInput, TValue>(this.Result, this.RemainingInput, this.ErrorInfo.Merge(otherError));
            else
                return new ParseResult<TInput, TValue>(this.ErrorInfo.Merge(otherError));
        }

        internal ParseResult<TInput, TValue> SetExpectation(string label)
        {
            if (this.Succeeded)
                return new ParseResult<TInput, TValue>(this.Result, this.RemainingInput, this.ErrorInfo.SetExpectation(label));
            else
                return new ParseResult<TInput, TValue>(this.ErrorInfo.SetExpectation(label));
        }

        public override bool Equals(object obj)
        {
            ParseResult<TInput, TValue> other = obj as ParseResult<TInput, TValue>;

            if (other == null)
                return false;
            if (!object.Equals(other.Result, this.Result))
                return false;
            if (!object.Equals(other.RemainingInput, this.RemainingInput))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return this.RemainingInput.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// Parse result with info about whether it consumed any input.
    /// </summary>
    /// <typeparam name="TInput">The type of object used to represent the ParserState.</typeparam>
    /// <typeparam name="TValue">The type of result object the ParseResult stores.</typeparam>
    internal class Consumed<TInput, TValue>
    {
        internal bool HasConsumedInput;
        internal ParseResult<TInput, TValue> ParseResult;

        #region Constructor
        internal Consumed(bool hasConsumedInput, ParseResult<TInput, TValue> result)
        {
            this.HasConsumedInput = hasConsumedInput;
            this.ParseResult = result;
        }
        #endregion

        #region Properties/Info Functions
        internal List<TInput> ConsumedInputSince(Consumed<TInput, TValue> previousConsumedResult)
        {
            return this.ParseResult.ConsumedInputSince(previousConsumedResult.ParseResult);
        }

        internal List<TInput> ConsumedInputSince(ParseResult<TInput, TValue> previousParseResult)
        {
            return this.ParseResult.ConsumedInputSince(previousParseResult);
        }

        internal List<TInput> ConsumedInputSince(ParserState<TInput> previousState)
        {
            return this.ParseResult.ConsumedInputSince(previousState);
        }
        #endregion

        #region Error handling and overrides
        internal Consumed<TInput, TValue> Tag(string label)
        {
            if (this.HasConsumedInput)
                return this;

            return new Consumed<TInput, TValue>(this.HasConsumedInput, this.ParseResult.SetExpectation(label));
        }

        public override string ToString()
        {
            return this.ParseResult.ToString();
        }
        #endregion
    }

    /// <summary>
    /// The delegate representation type for parsers.
    /// </summary>
    /// <typeparam name="TType">The token type used to represent the state list.</typeparam>
    /// <typeparam name="RType">The result type that the Consumed object stores.</typeparam>
    /// <param name="input">The ParserState object to be worked on.</param>
    /// <returns>Returns a Consumed object representing the result of the parse.</returns>
    internal delegate Consumed<TInput, TValue> P<TInput, TValue>(ParserState<TInput> input);


    /// <summary>
    /// Base abstract class for monadic parsers.  These functions know the
    /// underlying storage type.
    /// Contains all the basic parsers that are independent of return type.
    /// </summary>
    /// <typeparam name="TType">The type of object/token that the ParseState stores.</typeparam>
    internal abstract class BaseParser<TInput> where TInput : IComparable
    {
        // Always fail.  Stores the provided message in the error info.
        protected internal static P<TInput, T> Fail<T>(string message)
        {
            return input =>
                new Consumed<TInput, T>(
                    false,
                    new ParseResult<TInput, T>(
                        new ErrorInfo(input.Position, Enumerable.Empty<string>(), message)));
        }

        // Always succeed, and returns the specified value
        protected internal static P<TInput, T> Return<T>(T value)
        {
            return input =>
                new Consumed<TInput, T>(
                    false,
                    new ParseResult<TInput, T>(
                        value,
                        input,
                        new ErrorInfo(input.Position)));
        }

        /// <summary>
        /// Item() consumes the first element of the input string list and returns it as a result.
        /// </summary>
        /// <returns></returns>
        protected internal P<TInput, TInput> Item()
        {
            return Satisfy(c => true).Tag("any item");
        }

        /// <summary>
        /// Try(p) attempts to run parser p.  If it fails, it pretends
        /// that it hasn't consumed any input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        internal P<TInput, TValue> Try<TValue>(P<TInput, TValue> p)
        {
            return input =>
            {
                Consumed<TInput, TValue> consumed = p(input);

                if (consumed.HasConsumedInput && !consumed.ParseResult.Succeeded)
                    return new Consumed<TInput, TValue>(false, consumed.ParseResult);
                else
                    return consumed;
            };
        }

        /// <summary>
        /// Satisfy(predicate) succeeds only if the predicate, when applied to
        /// a string, returns true.  If it succeeds, return the string that passed.
        /// </summary>
        /// <param name="pred">The predicate comparitor.</param>
        /// <returns></returns>
        protected internal P<TInput, TInput> Satisfy(Predicate<TInput> pred)
        {
            return input =>
            {
                // If we're past the end of the input, can't comply, fail.
                if (input.EOF)
                {
                    return new Consumed<TInput, TInput>(
                        false,
                        new ParseResult<TInput, TInput>(
                            new ErrorInfo(
                                input.Position,
                                Enumerable.Empty<string>(),
                                "unexpected end of input")));
                }
                // If predicate fails, return fail result.
                else if (!pred(input.Input[input.Position]))
                {
                    return new Consumed<TInput, TInput>(
                        false,
                        new ParseResult<TInput, TInput>(
                            new ErrorInfo(
                                input.Position,
                                Enumerable.Empty<string>(),
                                "unexpected value '" + input.Input[input.Position].ToString() + "'")));
                }
                // Otherwise return new Consumed result.
                else
                {
                    return new Consumed<TInput, TInput>(
                        true,
                        new ParseResult<TInput, TInput>(
                            input.Input[input.Position],
                            new ParserState<TInput>(
                                input.Position + 1,
                                input.Input),
                            new ErrorInfo(input.Position + 1)));
                }
            };
        }

        protected internal P<TInput, TValue> Satisfy<TValue>(Predicate<TInput> pred, TValue result)
        {
            return input =>
                {
                    var consumed = Satisfy(pred)(input);

                    if (consumed.ParseResult.Succeeded)
                        return new Consumed<TInput, TValue>(
                            consumed.HasConsumedInput,
                            new ParseResult<TInput, TValue>(
                                result, consumed.ParseResult.RemainingInput));
                    else
                        return new Consumed<TInput, TValue>(
                            false,
                            new ParseResult<TInput, TValue>(
                                consumed.ParseResult.ErrorInfo));
                };
        }

        protected internal P<TInput, TInput> DoesNotSatisfy(Predicate<TInput> pred)
        {
            return input =>
            {
                // If we're past the end of the input, can't comply, fail.
                if (input.EOF)
                {
                    return new Consumed<TInput, TInput>(
                        false,
                        new ParseResult<TInput, TInput>(
                            new ErrorInfo(
                                input.Position,
                                Enumerable.Empty<string>(),
                                "unexpected end of input")));
                }
                // If predicate passes, this function fails.
                else if (pred(input.Input[input.Position]))
                {
                    return new Consumed<TInput, TInput>(
                        false,
                        new ParseResult<TInput, TInput>(
                            new ErrorInfo(
                                input.Position,
                                Enumerable.Empty<string>(),
                                "unexpected value '" + input.Input[input.Position].ToString() + "'")));
                }
                // Otherwise return new Consumed result.
                else
                {
                    return new Consumed<TInput, TInput>(
                        true,
                        new ParseResult<TInput, TInput>(
                            input.Input[input.Position],
                            new ParserState<TInput>(
                                input.Position + 1,
                                input.Input),
                            new ErrorInfo(input.Position + 1)));
                }
            };
        }

        protected internal P<TInput, TValue> DoesNotSatisfy<TValue>(Predicate<TInput> pred, TValue result)
        {
            return input =>
            {
                var consumed = DoesNotSatisfy(pred)(input);

                if (consumed.ParseResult.Succeeded)
                    return new Consumed<TInput, TValue>(
                        consumed.HasConsumedInput,
                        new ParseResult<TInput, TValue>(
                            result, consumed.ParseResult.RemainingInput));
                else
                    return new Consumed<TInput, TValue>(
                        false,
                        new ParseResult<TInput, TValue>(
                            consumed.ParseResult.ErrorInfo));
            };
        }


        /// <summary>
        /// Many(p) parses with p zero or more times, returning the sequence of results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        protected internal P<TInput, IEnumerable<TValue>> Many<TValue>(P<TInput, TValue> p)
        {
            return Many1<TValue>(p).Or(Return(Enumerable.Empty<TValue>()));
        }

        /// <summary>
        /// Many1(p) parses with p one or more times, returning the sequence of results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        protected internal P<TInput, IEnumerable<TValue>> Many1<TValue>(P<TInput, TValue> p)
        {
            return from x in p
                   from xs in Many(p)
                   select Cons(x, xs);
        }

        // Literal(s, x) parses a particular string, and returns a fixed result
        protected internal P<TInput, TValue> Literal<TValue>(TInput toParse, TValue result)
        {
            return LiteralHelper(new List<TInput>() { toParse }, result)
                .Tag("literal string \"" + toParse.ToString() + "\"");
        }

        protected internal P<TInput, TValue> Literal<TValue>(List<TInput> toParse, TValue result)
        {
            return LiteralHelper(toParse, result)
                .Tag("literal string \"" + AggregateList(toParse) + "\"");
        }

        P<TInput, TValue> LiteralHelper<TValue>(List<TInput> toParse, TValue result)
        {
            if (toParse.Count == 0)
                return Return(result);
            else
                return Literal(toParse[0]).Then_(LiteralHelper(toParse.Skip(1).ToList(), result));
        }

        protected internal P<TInput, TInput> Literal(TInput toParse)
        {
            return Satisfy(x => x.Equals(toParse))
                .Tag("string '" + toParse.ToString() + "'");
        }

        protected internal P<TInput, TValue> OneOf<TValue>(List<TInput> toParse, TValue result)
        {
            if (toParse.Count == 0)
                return Fail<TValue>("Failed to match any");
            else
                return Literal(toParse[0], result)
                    .Or<TInput, TValue>(OneOf(toParse.Skip(1).ToList(), result));
        }

        protected internal P<TInput, TValue> OneOf<TValue>(List<Predicate<TInput>> conditions, TValue result)
        {
            if (conditions.Count == 0)
                return Fail<TValue>("Failed to match any conditions");
            else
                return Satisfy((conditions[0]), result)
                    .Or<TInput, TValue>(OneOf(conditions.Skip(1).ToList(), result));
        }

        protected internal P<TInput, TValue> NoneOf<TValue>(List<TInput> toParse, TValue result)
        {
            if (toParse.Count == 0)
                return Return(result);
            else
                return Literal(toParse[0], result)
                    .Or<TInput, TValue>(OneOf(toParse.Skip(1).ToList(), result));
        }

        protected internal P<TInput, TValue> NoneOf<TValue>(List<Predicate<TInput>> conditions, TValue result)
        {
            if (conditions.Count == 0)
                return Return(result);
            else
                return Satisfy((conditions[0]), result)
                    .Or<TInput, TValue>(OneOf(conditions.Skip(1).ToList(), result));
        }


        /// <summary>
        /// Cons(x, rest) returns a new list like rest but with x appended to the front
        /// </summary>
        /// <typeparam name="T">The type of object in the enumeration.</typeparam>
        /// <param name="x">The item to be added to the front of the list.</param>
        /// <param name="rest">The remainder of the list.</param>
        /// <returns></returns>
        protected internal static IEnumerable<T> Cons<T>(T x, IEnumerable<T> rest)
        {
            yield return x;
            foreach (T t in rest)
                yield return t;
        }

        /// <summary>
        /// Function to take a list of TInputs and construct a string representation of them.
        /// Used for tagging/debugging.
        /// </summary>
        /// <param name="toParse"></param>
        /// <returns></returns>
        protected string AggregateList(List<TInput> toParse)
        {
            if ((toParse == null) || (toParse.Count == 0))
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(toParse.First().ToString());

            foreach (var x in toParse.Skip(1))
            {
                sb.Append(" ");
                sb.Append(x.ToString());
            }

            return sb.ToString();
        }

    }


    /// <summary>
    /// By providing Where, Select and SelectMany methods on Parser<TInput,TValue>
    /// we make the C# Query Expression syntax available for manipulating Parsers.
    /// </summary>
    internal static class ParserCombinatorsLINQExtensions
    {
        // Always succeed, and returns the specified value.
        // Reimplimented in this static class for local usage.
        private static P<TInput, TValue> MReturn<TInput, TValue>(TValue value)
        {
            return input =>
                new Consumed<TInput, TValue>(
                    false,
                    new ParseResult<TInput, TValue>(
                        value,
                        input,
                        new ErrorInfo(input.Position)));
        }

        /// <summary>
        /// Conditional check to select values.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="parser"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        internal static P<TInput, TValue> Where<TInput, TValue>(
            this P<TInput, TValue> parser,
            Predicate<TValue> pred)
        {
            return input =>
            {
                var resultOfBaseParse = parser(input);

                // If the parser itself failed, return a Fail.
                if ((resultOfBaseParse == null) || (resultOfBaseParse.ParseResult.Succeeded == false))
                    return new Consumed<TInput, TValue>(
                               false,
                               new ParseResult<TInput, TValue>(
                               new ErrorInfo(input.Position, Enumerable.Empty<string>(), "Where parser failed")));

                // If it succeeded, then we want to run the predicate on the
                // result of the parser.
                if (pred(resultOfBaseParse.ParseResult.Result))
                {
                    // If predicate passes, return the Consumed object.
                    return resultOfBaseParse;
                }
                else
                {
                    // If predicate results in false, return a Fail.
                    return new Consumed<TInput, TValue>(
                               false,
                               new ParseResult<TInput, TValue>(
                               new ErrorInfo(input.Position, Enumerable.Empty<string>(), "Where parser failed")));
                }
            };
        }

        /// <summary>
        /// A selection parser.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TValue2"></typeparam>
        /// <param name="parser"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        internal static P<TInput, TValue2> Select<TInput, TValue, TValue2>(
            this P<TInput, TValue> parser, Func<TValue, TValue2> selector)
            where TInput : IComparable
        {
            return parser.Then(x => MReturn<TInput, TValue2>(selector(x)));
        }

        /// <summary>
        /// A selection parser for many values.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TIntermediate"></typeparam>
        /// <typeparam name="TValue2"></typeparam>
        /// <param name="p"></param>
        /// <param name="selector"></param>
        /// <param name="projector"></param>
        /// <returns></returns>
        internal static P<TInput, TValue2> SelectMany<TInput, TValue, TIntermediate, TValue2>(
            this P<TInput, TValue> p,
            Func<TValue, P<TInput, TIntermediate>> selector,
            Func<TValue, TIntermediate, TValue2> projector)
            where TInput : IComparable
        {
            return p.Then(r1 => selector(r1).Then(r2 => MReturn<TInput, TValue2>(projector(r1, r2))));
        }
    }

    /// <summary>
    /// These are the basic extensions to the parser delegate that
    /// allow combining parsers together (ie: combinators).
    /// </summary>
    internal static class ParserCombinatorExtensions
    {
        /// <summary>
        /// Add a label to the state of any parse result for debugging.
        /// </summary>
        /// <typeparam name="T">The type of the result object.</typeparam>
        /// <param name="p">The predicate this is extending.</param>
        /// <param name="label">A string to use to tag the resulting parse state.</param>
        /// <returns></returns>
        internal static P<TInput, TValue> Tag<TInput, TValue>(this P<TInput, TValue> p, string label)
        {
            return input => p(input).Tag(label);
        }

        /// <summary>
        /// This function runs parser p1 on the input.  If it fails, return
        /// an empty Consumed.  If it succeeds, run the provided function on the result
        /// of the initial function.
        /// </summary>
        /// <typeparam name="TValue1">The type of result object generated by the first function.</typeparam>
        /// <typeparam name="TValue2">The type of result object generated by the second function.</typeparam>
        /// <param name="p1">The initial parser to run.</param>
        /// <param name="f">The function defining the second parser to run.</param>
        /// <returns>Returns the completely constructed parser.</returns>
        internal static P<TInput, TValue2> Then<TInput, TValue1, TValue2>(
            this P<TInput, TValue1> p1, Func<TValue1, P<TInput, TValue2>> f)
            where TInput : IComparable
        {
            return input =>
            {
                Consumed<TInput, TValue1> consumed1 = p1(input);

                if (consumed1.ParseResult.Succeeded)
                {
                    Consumed<TInput, TValue2> consumed2 =
                        f(consumed1.ParseResult.Result)(consumed1.ParseResult.RemainingInput);

                    return new Consumed<TInput, TValue2>(
                           consumed1.HasConsumedInput || consumed2.HasConsumedInput,
                           consumed2.HasConsumedInput
                               ? consumed2.ParseResult
                               : consumed2.ParseResult.MergeError(consumed1.ParseResult.ErrorInfo));
                }
                else
                {
                    return new Consumed<TInput, TValue2>(
                        consumed1.HasConsumedInput,
                        new ParseResult<TInput, TValue2>(consumed1.ParseResult.ErrorInfo));
                }
            };
        }

        /// <summary>
        /// Then_ is a variant on Then.  It runs p1 predicate on the input,
        /// discards the results, and runs p2 on the result of the p1 function.
        /// </summary>
        /// <typeparam name="T">The type of result object generated by the first function.</typeparam>
        /// <typeparam name="U">The type of result object generated by the second function.</typeparam>
        /// <param name="p1">The first parser to run.</param>
        /// <param name="p2">The second parser to run.</param>
        /// <returns>Returns the combined parser function.</returns>
        internal static P<TInput, TValue2> Then_<TInput, TValue1, TValue2>(
            this P<TInput, TValue1> p1, P<TInput, TValue2> p2)
            where TInput : IComparable
        {
            return p1.Then<TInput, TValue1, TValue2>(dummy => p2);
        }

        /// <summary>
        /// p1.Or(p2) tries p1, but if it fails without consuming input, runs p2 instead.
        /// </summary>
        /// <typeparam name="T">The type of result object generated.</typeparam>
        /// <param name="p1">The first parser to run.</param>
        /// <param name="p2">The second parser to run.</param>
        /// <returns>Returns the combined parser function.</returns>
        internal static P<TInput, TValue> Or<TInput, TValue>(this P<TInput, TValue> p1, P<TInput, TValue> p2)
            where TInput : IComparable
        {
            return input =>
            {
                Consumed<TInput, TValue> consumed1 = p1(input);

                if (consumed1.ParseResult.Succeeded || consumed1.HasConsumedInput)
                {
                    return consumed1;
                }
                else
                {
                    Consumed<TInput, TValue> consumed2 = p2(input);

                    if (consumed2.HasConsumedInput)
                        return consumed2;

                    return new Consumed<TInput, TValue>(
                        consumed2.HasConsumedInput,
                        consumed2.ParseResult.MergeError(consumed1.ParseResult.ErrorInfo));
                }
            };
        }

        /// <summary>
        /// p1.FollowedBy(p2) matches p1 then p2, and returns p1's result (discards p2's result)
        /// </summary>
        /// <typeparam name="TValue1"></typeparam>
        /// <typeparam name="TValue2"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        internal static P<TInput, TValue1> FollowedBy<TInput, TValue1, TValue2>(
            this P<TInput, TValue1> p1, P<TInput, TValue2> p2)
            where TInput : IComparable
        {
            return from result in p1
                   from discard in p2
                   select result;
        }

        /// <summary>
        /// p1.NotFollowedBy(p2,label) runs p1 and then looks ahead for p2, failing if p2 is there
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TValue1"></typeparam>
        /// <typeparam name="TValue2"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p2Label"></param>
        /// <returns></returns>
        internal static P<TInput, TValue1> NotFollowedBy<TInput, TValue1, TValue2>(
            this P<TInput, TValue1> p1, P<TInput, TValue2> p2, string p2Label)
            where TInput : IComparable
        {
            return p1.Then(result =>
                Try_<TInput, TValue1>(
                p2.Then_(BaseParser<TInput>.Fail<TValue1>("unexpected " + p2Label))
                .Or<TInput, TValue1>(BaseParser<TInput>.Return<TValue1>(result))));
        }

        /// <summary>
        /// Try(p) attempts to run parser p.  If it fails, it pretends
        /// that it hasn't consumed any input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static P<TInput, TValue> Try_<TInput, TValue>(this P<TInput, TValue> p)
        {
            return input =>
            {
                Consumed<TInput, TValue> consumed = p(input);

                if (consumed.HasConsumedInput && !consumed.ParseResult.Succeeded)
                    return new Consumed<TInput, TValue>(false, consumed.ParseResult);
                else
                    return consumed;
            };
        }

    }

    /// <summary>
    /// Token tuple.
    /// </summary>
    internal class TokenTuple
    {
        internal TokenType TokenType;
        internal string Text;
    }

    /// <summary>
    /// Enumerated token types
    /// </summary>
    internal enum TokenType
    {
        Cover,
        MagicBurst,
        Bust,
        Resist,
        Skillchain,
        DiceRoll,
        AdditionalEffect,
        ProperNoun,
        DrainType,
        Verb,
        Word,
    };
}
