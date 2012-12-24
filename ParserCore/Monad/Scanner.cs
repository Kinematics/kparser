using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser.Monad
{
    /// <summary>
    /// The scanner takes the initial input string and transforms it into a
    /// series of words (strings).
    /// Current timing: approx 0.01 ms per word.
    /// </summary>
    internal static class Scanner
    {
        // The regex to define what comprises a word.
        // Embedded periods, apostraphes and dashs are allowed. (eg: Lamia No.09, Da'Dha Hundredmask, Koru-moru)
        // Other punctuation (including "'s" for possessives) is separated out into their own 'words'
        // EG: "Cover! The Mandragora ..." >> "Cover", "!", "The", "Mandragora"...
        static Regex ScanWordRegex =
            new Regex(@"\s*(?<Word>!(?= |$)|\.(?= |$)|'s(?= )|(?(?=\S+'s )\S+(?='s )|\S+\w))(?<Remainder>.+)?");

        /// <summary>
        /// A scanner to break down the provided input string into a list of words.
        /// The recursive version is essentially identical in speed to the non-recursive
        /// version, so using this methodology.
        /// </summary>
        /// <param name="input">The string to break down.</param>
        /// <returns>A list of strings comprised of the individual words of the input string.</returns>
        internal static List<string> Scan(string input)
        {
            Match match;

            // At the end of the recursion, return an empty list.
            if (string.IsNullOrEmpty(input))
                return new List<string>();

            // Match against the regex.
            match = ScanWordRegex.Match(input);
            if (match.Success)
            {
                // Have to break this down into separate steps because Insert returns void
                // and can't be chained with the previous function.

                // Get the list of words from the recursive call on the remainder
                // until it returns an empty list.
                List<string> words = Scan(match.Groups["Remainder"].Value);
                // Add the currently parsed word to the start of the list.
                words.Insert(0, match.Groups["Word"].Value);

                return words;
            }
            else
            {
                throw new ArgumentException(
                    string.Format("Unable to complete parsing of input string: {0}", input));
            }
        }
    }
}
