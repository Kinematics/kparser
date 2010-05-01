using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Monad2
{
    // Type definition to specify location within the parse stream.
    // In this case, it represents the index of the string/token
    // within the List<string> or List<Token> object.
    using Position = System.Int32;

    public class LexerState
    {
        public Position Position;
        public List<string> Input;
    }

    public class ParserState
    {
        public Position Position;
        public List<Token> Input;
    }

    public class ErrorInfo
    {
        public Position Position;
        public IEnumerable<string> Expectations;
        public string Message;
    }


    /// <summary>
    /// The output class for the scanner (lexical analyzer).
    /// Works on lists of type string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LexResult<T>
    {
        public bool Succeeded;

        // if Succeeded, all fields below are used, else only ErrorInfo is used
        public ErrorInfo ErrorInfo;

        public readonly T Result;
        public readonly List<string> RemainingInput;

        public LexResult(T r, List<string> ri) { this.Result = r; this.RemainingInput = ri; }

        public override bool Equals(object obj)
        {
            LexResult<T> other = obj as LexResult<T>;

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
    }

    /// <summary>
    /// The output result for the parser.
    /// Works on lists of type Token.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParseResult<T>
    {
        public bool Succeeded;

        // if Succeeded, all fields below are used, else only ErrorInfo is used
        public ErrorInfo ErrorInfo;

        public readonly T Result;
        public readonly List<Token> RemainingInput;

        public ParseResult(T r, List<Token> ri) { this.Result = r; this.RemainingInput = ri; }

        public override bool Equals(object obj)
        {
            ParseResult<T> other = obj as ParseResult<T>;

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
    }

    public class LexConsumed<T>
    {
        public bool HasConsumedInput;
        public LexResult<T> LexResult;
    }

    public class ParseConsumed<T>
    {
        public bool HasConsumedInput;
        public ParseResult<T> ParseResult;
    }


    // Representation type for parsers
    public delegate LexResult<T> PLex<T>(List<string> input);
    public delegate ParseResult<T> P<T>(List<Token> input);

    /// <summary>
    /// Token tuple.
    /// </summary>
    public class Token
    {
        TokenType TokenType;
        string Text;
    }

    /// <summary>
    /// Enumerated token types
    /// </summary>
    public enum TokenType
    {
        CoverPrefix,
        MagicBurstPrefix,
        BustPrefix,
        ResistPrefix,
        SkillchainPrefix,
        DiceRollPrefix,
        AdditionalEffectPrefix,
        ProperNoun,
        DrainType,
        Verb,
        Word,
    };
}
