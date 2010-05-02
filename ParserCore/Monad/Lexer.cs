using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// This is a set of static classes to define appropriate extension methods
/// for monadic parsing.
/// </summary>
namespace WaywardGamers.KParser.Monad
{
    /// <summary>
    /// The lexer takes the broken up string and performs an analysis to group
    /// words together (if necessary) and notate what type of token each
    /// word group represents.  This list of tokens will then be passed on
    /// to the next stage of parsing.
    /// </summary>
    internal static class Lexer
    {
        #region LINQ syntax enablers
        // By providing Where, Select and SelectMany methods on Parser<TInput,TValue> we make the 
        // C# Query Expression syntax available for manipulating Parsers.  

        public static P<U, string> Select<T, U>(this P<T, string> p, Func<T, U> selector)
        {
            return p.Then(x => Return<U>(selector(x)));
        }

        public static P<V, string> SelectMany<T, U, V>(this P<T, string> p,
                                          Func<T, P<U, string>> selector, Func<T, U, V> projector)
        {
            return p.Then(r1 => selector(r1).Then(r2 => Return<V>(projector(r1, r2))));
        }

        public static P<T, string> Where<T>(this P<T, string> p, Predicate<T> pred)
        {
            return input =>
            {
                var resultOfBaseParse = p(input);

                // If the parser itself failed, return a Fail.
                if ((resultOfBaseParse == null) || (resultOfBaseParse.ParseResult.Succeeded == false))
                    return Fail<T>("Where parser failed")(input);

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
                    return Fail<T>("Where predicate failed")(input);
                }
            };
        }

        #endregion

        #region Core functions that are required for the parser to be monadic.

        /// <summary>
        /// This function always fails (consumes no input), regardless of input.
        /// </summary>
        /// <typeparam name="T">The type of result object generated.</typeparam>
        /// <returns>Returns a new empty Consumed object where none of the input was consumed,
        /// but the input position is marked.</returns>
        internal static P<T, string> Fail<T>(string message)
        {
            return input =>
                new Consumed<T, string>(
                    false,
                    new ParseResult<T, string>(
                        new ErrorInfo(input.Position, Enumerable.Empty<string>(), message)));
        }

        /// <summary>
        /// Return(x) is a parser that always succeeds and returns a Consumed object
        /// containing a ParseResult of type T that contains x, without consuming
        /// any input.
        /// </summary>
        /// <typeparam name="T">The type of result object generated.</typeparam>
        /// <param name="x">The result object generated.</param>
        /// <returns>Always returns a Consumed object containing a ParseResult
        /// built with the provided result object.</returns>
        internal static P<T, string> Return<T>(T x)
        {
            return input =>
                new Consumed<T, string>(
                    false,
                    new ParseResult<T, string>(
                        x,
                        input,
                        new ErrorInfo(input.Position)));
        }

        /// <summary>
        /// Return(f) is a parser that always succeeds returns the function f
        /// (which processes objects of type T), without consuming any input.
        /// </summary>
        /// <typeparam name="T">The type of result object generated.</typeparam>
        /// <param name="f">The function to be returned.</param>
        /// <returns>Always returns a function capable of operating on
        /// objects of type T.</returns>
        internal static P<Func<T, T, T>, string> Return<T>(Func<T, T, T> f)
        {
            // Use base Return function to handle the explicit construction work.
            return Return<Func<T, T, T>>(f);
        }

        /// <summary>
        /// This function runs parser p1 on the input.  If it fails, return
        /// null.  If it succeeds, run the provided function on the result
        /// of the initial function.
        /// </summary>
        /// <typeparam name="T">The type of result object generated by the first function.</typeparam>
        /// <typeparam name="U">The type of result object generated by the second function.</typeparam>
        /// <param name="p1">The initial parser to run.</param>
        /// <param name="f">The function defining the second parser to run.</param>
        /// <returns>Returns the completely constructed parser.</returns>
        internal static P<U, string> Then<T, U>(this P<T, string> p1, Func<T, P<U, string>> f)
        {
            return input =>
            {
                Consumed<T, string> consumed1 = p1(input);

                if (consumed1.ParseResult.Succeeded)
                {
                    Consumed<U, string> consumed2 = f(consumed1.ParseResult.Result)(consumed1.ParseResult.RemainingInput);
                    return new Consumed<U, string>(
                           consumed1.HasConsumedInput || consumed2.HasConsumedInput,
                           consumed2.HasConsumedInput
                               ? consumed2.ParseResult
                               : consumed2.ParseResult.MergeError(consumed1.ParseResult.ErrorInfo));
                }
                else
                {
                    return new Consumed<U, string>(
                        consumed1.HasConsumedInput,
                        new ParseResult<U, string>(consumed1.ParseResult.ErrorInfo));
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
        internal static P<U, string> Then_<T, U>(this P<T, string> p1, P<U, string> p2)
        {
            return p1.Then<T, U>(dummy => p2);
        }

        /// <summary>
        /// p1.Or(p2) tries p1, but if it fails without consuming input, runs p2 instead.
        /// </summary>
        /// <typeparam name="T">The type of result object generated.</typeparam>
        /// <param name="p1">The first parser to run.</param>
        /// <param name="p2">The second parser to run.</param>
        /// <returns>Returns the combined parser function.</returns>
        internal static P<T, string> Or<T>(this P<T, string> p1, P<T, string> p2)
        {
            return input =>
            {
                Consumed<T, string> consumed1 = p1(input);

                if (consumed1.ParseResult.Succeeded || consumed1.HasConsumedInput)
                {
                    return consumed1;
                }
                else
                {
                    Consumed<T, string> consumed2 = p2(input);

                    if (consumed2.HasConsumedInput)
                        return consumed2;

                    return new Consumed<T, string>(
                        consumed2.HasConsumedInput,
                        consumed2.ParseResult.MergeError(consumed1.ParseResult.ErrorInfo));
                }
            };
        }

        /// <summary>
        /// Try(p) behaves like p, except if p fails, it pretends it hasn't consumed any input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static P<T, string> Try<T>(this P<T, string> p)
        {
            return input =>
            {
                Consumed<T, string> consumed = p(input);
                if (consumed.HasConsumedInput && !consumed.ParseResult.Succeeded)
                    return new Consumed<T, string>(false, consumed.ParseResult);
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
        internal static P<string, string> Satisfy(Predicate<string> pred)
        {
            return input =>
            {
                // If we're past the end of the input, can't comply, fail.
                if (input.Position >= input.Input.Count)
                {
                    return new Consumed<string, string>(
                        false,
                        new ParseResult<string, string>(
                            new ErrorInfo(
                                input.Position,
                                Enumerable.Empty<string>(),
                                "unexpected end of input")));
                }
                // If predicate fails, return fail result.
                else if (!pred(input.Input[input.Position]))
                {
                    return new Consumed<string, string>(
                        false,
                        new ParseResult<string, string>(
                            new ErrorInfo(
                                input.Position,
                                Enumerable.Empty<string>(),
                                "unexpected string '" + input.Input[input.Position] + "'")));
                }
                // Otherwise return new Consumed result.
                else
                {
                    return new Consumed<string, string>(
                        true,
                        new ParseResult<string, string>(
                            input.Input[input.Position],
                            new ParserState<string>(
                                input.Position + 1,
                                input.Input),
                            new ErrorInfo(input.Position + 1)));
                }
            };
        }

        /// <summary>
        /// Item() consumes the first element of the input string list and returns it as a result.
        /// </summary>
        /// <returns></returns>
        internal static P<string, string> Item()
        {
            return Satisfy(c => true).Tag("any string");
        }

        #endregion

        #region Other helpful support functions
        /// <summary>
        /// Add a label to the state of any parse result.
        /// p.Tag(label) makes it so if p fails without consuming input, the error "expected label" occurs
        /// </summary>
        /// <typeparam name="T">The type of the result object.</typeparam>
        /// <param name="p">The predicate this is extending.</param>
        /// <param name="label">A string to use to tag the resulting parse state.</param>
        /// <returns></returns>
        internal static P<T, string> Tag<T>(this P<T, string> p, string label)
        {
            return input => p(input).Tag(label);
        }

        /// <summary>
        /// Check if the next value in the remaining input is the specified string.
        /// If it is, return that as a parse result.
        /// </summary>
        /// <param name="toParse"></param>
        /// <returns></returns>
        internal static P<string, string> Literal(string toParse)
        {
            return Satisfy(x => x.ToLower() == toParse.ToLower()).Tag("string '" + toParse + "'");
        }

        // Literal(s, x) parses a particular string, and returns a fixed result
        internal static P<T, string> Literal<T>(string toParse, T result)
        {
            return LiteralHelper(new List<string>() { toParse }, result)
                .Tag("literal string \"" + toParse + "\"");
        }

        internal static P<T, string> Literal<T>(List<string> toParse, T result)
        {
            return LiteralHelper(toParse, result)
                .Tag("literal string \"" + toParse.Aggregate((x, y) => x + " " + y) + "\"");
        }

        static P<T, string> LiteralHelper<T>(List<string> toParse, T result)
        {
            if (toParse.Count == 0)
                return Lexer.Return(result);
            else
                return Literal(toParse[0]).Then_(LiteralHelper(toParse.Skip(1).ToList(), result));
        }

        /// <summary>
        /// Cons(x, rest) returns a new list like rest but with x appended to the front
        /// </summary>
        /// <typeparam name="T">The type of object in the enumeration.</typeparam>
        /// <param name="x">The item to be added to the front of the list.</param>
        /// <param name="rest">The remainder of the list.</param>
        /// <returns></returns>
         internal static IEnumerable<T> Cons<T>(T x, IEnumerable<T> rest)
        {
            yield return x;
            foreach (T t in rest)
                yield return t;
        }

        /// <summary>
        /// Many(p) parses with p zero or more times, returning the sequence of results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static P<IEnumerable<T>, string> Many<T>(P<T, string> p)
        {
            return Many1<T>(p).Or(Return(Enumerable.Empty<T>()));
        }

        /// <summary>
        /// Many1(p) parses with p one or more times, returning the sequence of results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static P<IEnumerable<T>, string> Many1<T>(P<T, string> p)
        {
            return p.Then(x =>
                   Many(p).Then(xs =>
                   Return(Cons(x, xs))));
        }

        // p1.FollowedBy(p2) matches p1 then p2, and returns p1's result (discards p2's result)
        public static P<T, string> FollowedBy<T, U>(this P<T, string> p1, P<U, string> p2)
        {
            return from result in p1
                   from discard in p2
                   select result;
        }

        // p1.NotFollowedBy(p2,label) runs p1 and then looks ahead for p2, failing if p2 is there
        internal static P<T, string> NotFollowedBy<T, U>(this P<T, string> p1, P<U, string> p2, string p2Label)
        {
            return p1.Then(result =>
                Try(p2.Then_(Fail<T>("unexpected " + p2Label))
                .Or(Return(result))));
        }

        #endregion

        //#region Custom rules for KParser

        //internal static P<TokenType, string> PlayerOrMobName<T>()
        //{
        //    return input =>
        //    {
        //        return new Consumed<TokenType, string>(false, new ParseResult<T, string>(new ErrorInfo(0)));

        //        Consumed<TokenType, string> c = IsYou(input);
        //        if (c.ParseResult.Succeeded)
        //            return c;


        //    };
        //}

        //internal static P<T, string> WhileNotB<T>(Predicate<string> predA, Predicate<string> predB, T result)
        //{
        //    return input =>
        //        {
        //            return WhileNotBHelper(predA, predB, result);
        //        };
        //}

        //static P<T, string> WhileNotBHelper<T>(Predicate<string> predA, Predicate<string> predB, T result)
        //{
        //    return input =>
        //        {
        //            if (predB(input.Input[input.Position]))
        //                return Lexer.Return(result);
        //            else
        //                return Literal(toParse[0]).Then_(LiteralHelper(toParse.Skip(1).ToList(), result));
        //        };
        //}

        //internal static P<string, string> Capitalized()
        //{
        //    return Satisfy(c => Regex.Match(c, @"^[A-Z]").Success).Tag("capitalized string");
        //}

        //internal static P<string, string> Possessive()
        //{
        //    return Satisfy(c => c == "'s").Tag("possessive string");
        //}

        //#endregion

        #region Tokenizer


        // The Tokenizer section takes the scanned list of words and groups certain ones together.
        // It returns a list of word 'clusters' that are identified and saved as Tokens.

        // Predicates
        static Predicate<string> PredIsCapitalized = (s => Regex.Match(s, @"^[A-Z]").Success);
        static Predicate<string> PredIsPosessive = (s => s == "'s");
        static Predicate<string> PredIsYou = (s => s == "You");
        static Predicate<string> PredIsThe = (s => string.Compare(s, "the", true) == 0);
        static Predicate<string> PredIsA = (s => Regex.Match(s, @"^an?", RegexOptions.IgnoreCase).Success);

        // Rules
        static P<string, string> IsThe = Try(Satisfy(PredIsThe));
        static P<string, string> IsA = Try(Satisfy(PredIsA));
        static P<string, string> IsYou = Try(Satisfy(PredIsYou));
        static P<string, string> IsProper = Try(Satisfy(PredIsCapitalized));
        static P<string, string> IsPossessive = Try(Satisfy(PredIsPosessive));


        internal static P<TokenType, string> Prefixes =
            Literal(new List<string> { "Cover", "!" }, TokenType.Cover).Or(
            Literal(new List<string> { "Resist", "!" }, TokenType.Resist).Or(
            Literal(new List<string> { "Bust", "!" }, TokenType.Bust).Or(
            Literal(new List<string> { "Dice", "Roll", "!" }, TokenType.DiceRoll).Or(
            Literal(new List<string> { "Magic", "Burst", "!" }, TokenType.MagicBurst).Or(
            Literal(new List<string> { "Additional", "Effect", ":" }, TokenType.AdditionalEffect).Or(
            Literal(new List<string> { "Skillchain", ":" }, TokenType.Skillchain)))))));


        internal static P<TokenType, string> Cover =
            Literal("Cover", TokenType.Cover).FollowedBy(Literal("!", TokenType.Cover));

        internal static P<TokenType, string> Cover2 =
            Literal("Cover", TokenType.Cover).NotFollowedBy(Literal("?", TokenType.Cover), "?");

        internal static P<TokenType, string> IsYouToo = Literal<TokenType>("You", TokenType.ProperNoun);

        internal static P<TokenType, string> ProperName =
            Try(Literal("You", TokenType.ProperNoun)).Or(Literal("Motenten", TokenType.ProperNoun));



        internal static List<TokenTuple> Tokenize(List<string> wordList)
        {
            List<TokenTuple> tokenList = new List<TokenTuple>();

            ParserState<string> startingState = new ParserState<string>(0, wordList);

            var coverCheck = Cover(startingState).AppendToken(tokenList, startingState);

            var youCheck = ProperName(coverCheck.ParseResult.RemainingInput).AppendToken(tokenList, coverCheck);

            var prefixCheck = Prefixes(startingState).AppendToken(tokenList, startingState);
            
            return tokenList;
        }




        #endregion

        #region Additional utility functions
       
        /// <summary>
        /// If the new state consumed input, create a token from the consumed input and token
        /// type and add it to the supplied token list.
        /// </summary>
        /// <param name="newState">The state after the parsing of the token.</param>
        /// <param name="tokenList">The list of tokens to add the new Token to.</param>
        /// <param name="startingState">The state prior to the parsing of the token.</param>
        /// <returns>Returns the newState object to allow for sequential function flow.</returns>
        private static Consumed<TokenType, string> AppendToken(this Consumed<TokenType, string> newState,
            List<TokenTuple> tokenList, ParserState<string> startingState)
        {
            if (newState.HasConsumedInput)
            {
                tokenList.Add(
                    new TokenTuple()
                    {
                        TokenType = newState.ParseResult.Result,
                        Text = ConstructString(newState.ConsumedInputSince(startingState))
                    });
            }

            return newState;
        }

        /// <summary>
        /// Same as above, but allowing for a ParseResult as the starting state.
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="tokenList"></param>
        /// <param name="startingState"></param>
        /// <returns></returns>
        private static Consumed<TokenType, string> AppendToken(this Consumed<TokenType, string> newState,
            List<TokenTuple> tokenList, ParseResult<TokenType, string> startingState)
        {
            return newState.AppendToken(tokenList, startingState.RemainingInput);
        }

        /// <summary>
        /// Same as above, but allowing for a Consumed object as the starting state.
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="tokenList"></param>
        /// <param name="startingState"></param>
        /// <returns></returns>
        private static Consumed<TokenType, string> AppendToken(this Consumed<TokenType, string> newState,
            List<TokenTuple> tokenList, Consumed<TokenType, string> startingState)
        {
            return newState.AppendToken(tokenList, startingState.ParseResult.RemainingInput);
        }

        /// <summary>
        /// Takes a string list of unknown length and constructs a single
        /// string out of it.  Elements that start with punctuation (eg: "'s", "!", etc)
        /// are appended directly to the string.  Other words are separated by spaces.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static string ConstructString(List<string> list)
        {
            if (list.Count == 0)
                throw new InvalidOperationException();

            StringBuilder fullString = new StringBuilder();

            fullString.Append(list.First());

            if (list.Count > 1)
            {
                foreach (var str in list.Skip(1))
                {
                    if (!Regex.Match(str, @"^\W").Success)
                        fullString.Append(" ");

                    fullString.Append(str);
                }
            }

            return fullString.ToString();
        }
        #endregion

    }

}
