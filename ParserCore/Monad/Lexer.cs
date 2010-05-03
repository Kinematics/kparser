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
    internal class Lexer : BaseParser<string>
    {
        #region Externally available functions to call
        internal List<TokenTuple> Tokenize(List<string> wordList)
        {
            ParserState<string> startingState = new ParserState<string>(0, wordList);
            List<TokenTuple> tokenList = new List<TokenTuple>();


            return tokenList;
        }
        #endregion

        #region Constructor and Rule building
        internal Lexer()
        {
            CreateRules();
        }

        // Rules

        internal P<string, TokenType> Prefixes;
        internal P<string, TokenType> Cover;

        private void CreateRules()
        {
            Prefixes =
                Literal(new List<string> { "Cover", "!" }, TokenType.Cover).Or(
                Literal(new List<string> { "Resist", "!" }, TokenType.Resist).Or(
                Literal(new List<string> { "Bust", "!" }, TokenType.Bust).Or(
                Literal(new List<string> { "Dice", "Roll", "!" }, TokenType.DiceRoll).Or(
                Literal(new List<string> { "Magic", "Burst", "!" }, TokenType.MagicBurst).Or(
                Literal(new List<string> { "Additional", "Effect", ":" }, TokenType.AdditionalEffect).Or(
                Literal(new List<string> { "Skillchain", ":" }, TokenType.Skillchain)))))));


            Cover = Literal("Cover", TokenType.Cover)
                .FollowedBy(Literal("!", TokenType.Cover));

        }


        // Predicates
        static Predicate<string> PIsCapitalized = (s => Regex.Match(s, @"^[A-Z]").Success);
        static Predicate<string> PIsPosessive = (s => s == "'s");
        static Predicate<string> PIsYou = (s => s == "You");
        static Predicate<string> PIsThe = (s => string.Compare(s, "the", true) == 0);
        static Predicate<string> PIsA = (s => Regex.Match(s, @"^an?", RegexOptions.IgnoreCase).Success);



        #endregion
    }

    internal static class LexerUtilityExtensions
    {
        /// <summary>
        /// If the new state consumed input, create a token from the consumed input and token
        /// type and add it to the supplied token list.
        /// </summary>
        /// <param name="newState">The state after the parsing of the token.</param>
        /// <param name="tokenList">The list of tokens to add the new Token to.</param>
        /// <param name="startingState">The state prior to the parsing of the token.</param>
        /// <returns>Returns the newState object to allow for sequential function flow.</returns>
        private static Consumed<string, TokenType> AppendToken(this Consumed<string, TokenType> newState,
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
        private static Consumed<string, TokenType> AppendToken(this Consumed<string, TokenType> newState,
            List<TokenTuple> tokenList, ParseResult<string, TokenType> startingState)
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
        private static Consumed<string, TokenType> AppendToken(this Consumed<string, TokenType> newState,
            List<TokenTuple> tokenList, Consumed<string, TokenType> startingState)
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
    }

}
