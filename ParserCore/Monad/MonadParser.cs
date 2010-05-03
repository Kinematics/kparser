using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser.Monad
{
    public class MonadParser
    {
        #region Constructor/member variables
        Lexer lexer = new Lexer();

        /// <summary>
        /// Constructor
        /// </summary>
        public MonadParser()
        {
        }
        #endregion

        #region Testing
        /// <summary>
        /// Gets the Scanned string lists.  For each such list, we want to
        /// Tokenize the string list and then Parse it.
        /// </summary>
        public void RunTests()
        {
            List<List<string>> scannedStrings = GetScannedStrings();


            foreach (List<string> scannedString in scannedStrings)
            {
                List<TokenTuple> tokenizedString = lexer.Tokenize(scannedString);

                Parser.Parse(tokenizedString);
            }
        }

        /// <summary>
        /// Gets the array of text strings provided at the end of this class
        /// and calls Parser.Scan on each one, generating a list of lists of
        /// strings from the breakdown of the chat lines.
        /// </summary>
        /// <returns>Returns the list of Scanned chat lines as string lists.</returns>
        private List<List<string>> GetScannedStrings()
        {
            List<List<string>> stringList = new List<List<string>>(TestStrings.TestStringArray.Length);

            foreach (string testString in TestStrings.TestStringArray)
            {
                stringList.Add(Scanner.Scan(testString));
            }

            return stringList;
        }
        #endregion
    }
}

public static class TestStrings
{
    public static string[] TestStringArray = new string[1] {
        "Cover! Motenten hits the Greater Colibri for 128 points of damage."
    };
}

