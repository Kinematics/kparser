using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            P<string> IsEndOfLine = Check(endOfLine);

            
        }

        P<string> Word = Parser.Item();
        P<string> Fail = Parser.Fail<string>();
        Predicate<string> endOfLine = s => s == "." || s == "!";

        public P<string> Check(Predicate<string> pred)
        {
            return Word.Then(w => pred(w) ? Parser.Return(w) : Fail);
        }

        #endregion

        #region Testing
        private List<List<string>> GetTestStrings()
        {
            List<List<string>> stringList = new List<List<string>>(TestStrings.TestStringArray.Length);

            foreach (string testString in TestStrings.TestStringArray)
            {
                stringList.Add(Parser.Scan(testString));
            }

            return stringList;
        }

        public void RunTests()
        {
            List<List<string>> scannedStrings = GetTestStrings();

            foreach (List<string> scannedString in scannedStrings)
            {
                List<string> tokenizedString = Parser.Tokenize(scannedString);

                Parser.Parse(tokenizedString);
            }
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
        "Motenten hits the Greater Colibri for 128 points of damage."
    };
}

