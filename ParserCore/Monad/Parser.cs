using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser.Monad
{
    // Representation type for parsers
    public delegate ParseResult<T> P<T>(List<string> input);
    //public delegate Consumed<T, Token> P<T, Token>(ParserState<Token> input);


    public static class Parser
    {
        // core functions that know the underlying parser representation
        public static P<T> Fail<T>()
        {
            return input => null;
        }

        public static P<T> Return<T>(T x)
        {
            return input => new ParseResult<T>(x, input);
        }

        public static P<U> Then<T, U>(this P<T> p1, Func<T, P<U>> f)
        {
            return input =>
            {
                ParseResult<T> result1 = p1(input);

                if (result1 == null)
                    return null;
                else
                    return f(result1.Result)(result1.RemainingInput);
            };
        }

        public static P<T> Or<T>(this P<T> p1, P<T> p2)
        {
            return input =>
            {
                ParseResult<T> result1 = p1(input);

                if (result1 == null)
                    return p2(input);
                else
                    return result1;
            };
        }

        public static P<string> Item()
        {
            return input =>
            {
                if ((input == null) || (input.Count == 0))
                    return null;

                return new ParseResult<string>(input[0], input.Skip(1).ToList<string>());
            };
        }

        // other handy functions
        public static P<U> Then_<T, U>(this P<T> p1, P<U> p2)
        {
            return p1.Then(dummy => p2);
        }


        #region Scanner
        // The regex to define what comprises a word.
        // Embedded periods, apostraphes and dashs are allowed. (eg: Lamia No.09, Da'Dha Hundredmask, ??-??)
        // Other punctuation is separated out into their own 'words'
        // EG: Cover! The Mandragora >> "Cover", "!", "The", "Mandragora"...
        static Regex WordRegex = new Regex(@"(?<FirstWord>(\w|[.'-]\w)+|[:,!.])( (?<Remainder>.+))?");

        /// <summary>
        /// A scanner to break down the provided input string into a list of words.
        /// The recursive version is essentially identical in speed to the non-recursive
        /// version, so using this methodology.
        /// </summary>
        /// <param name="input">The string to break down.</param>
        /// <returns>A list of strings comprised of the individual words of the input string.</returns>
        public static List<string> Scan(string input)
        {
            Match match;

            // At the end of the recursion, return an empty list.
            if (string.IsNullOrEmpty(input))
                return new List<string>();

            // Match against the regex.
            match = WordRegex.Match(input);
            if (match.Success)
            {
                // Have to break this down into separate steps because Insert returns void.
                List<string> words = Scan(match.Groups["Remainder"].Value);
                words.Insert(0, match.Groups["FirstWord"].Value);
                return words;
            }
            else
            {
                throw new ArgumentException(
                    string.Format("Unable to complete parsing of input string: {0}", input));
            }
        }
        #endregion

        #region Tokenizer
        // The Tokenizer section takes the scanned list of words and groups certain ones together.
        // It returns a list of word 'clusters'.
        
        static Predicate<string> IsCapitalized = (s => Regex.Match(s, @"^[A-Z]").Success);
        static Predicate<string> IsPosessive = (s => Regex.Match(s, @"'s$").Success);
        static Predicate<string> IsYou = (s => Regex.Match(s, @"You").Success);

        static string Combine(string word1, string word2)
        {
            return word1 + word2;
        }

        static string CombineWords(string word1, string word2)
        {
            return word1 + " " + word2;
        }

        public static List<string> Tokenize(List<string> wordList)
        {
            return wordList;
        }

        #endregion

        #region Parser
        internal static Message Parse(List<string> input)
        {
            return null;
        }
        #endregion

    }
}
