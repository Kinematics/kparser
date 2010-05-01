using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser.Monad
{
    public class MonadParser
    {
        public MonadParser()
        {
            BuildParsers();
        }

        #region Parsers
        private void BuildParsers()
        {
            P<string, string> IsPeriodOrEx = Check(endOfLine);
            P<string, string> IsNumber = Check(numeric);

            var failInt = Lexer.Fail<int>("failint");

            var resFailInt = failInt(new ParserState<string>(0, new List<string>(){"one", "2"}));

            if (resFailInt != null)
                throw new InvalidOperationException();
        }

        P<string, string> Word = Lexer.Item();
        P<string, string> Fail = Lexer.Fail<string>("failstring");
        Predicate<string> endOfLine = s => s == "." || s == "!";
        Predicate<string> numeric = s => Regex.Match(s, @"\d+").Success;


        public P<string, string> Check(Predicate<string> pred)
        {
            return Word.Then(w => pred(w) ? Lexer.Return(w) : Fail);
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
                List<Token> tokenizedString = Lexer.Tokenize(scannedString);

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

        private void TestMonadParserOn(List<string> stringTokenList)
        {
            throw new NotImplementedException();
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

