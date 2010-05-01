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
    /// <typeparam name="T">The type of object the state list contains.</typeparam>
    public class ParserState<T>
    {
        public Position Position;
        public List<T> Input;

        public ParserState(Position position, List<T> input)
        {
            this.Position = position; this.Input = input;
        }

        public override string ToString()
        {
            // place a '^' before the current location in the string; this is helpful when looking at
            // values inside the debugger
            return this.Input.Take(this.Position).ToString() + "^" +
                   this.Input.Skip(this.Position).ToString();
        }
    }

    public class ErrorInfo
    {
        public Position Position;
        public IEnumerable<string> Expectations;
        public string Message;

        public ErrorInfo(Position position) : this(position, Enumerable.Empty<string>(), "unknown error") { }

        public ErrorInfo(Position position, IEnumerable<string> expectations, string message)
        {
            this.Position = position; this.Expectations = expectations; this.Message = message;
        }

        public ErrorInfo Merge(ErrorInfo other)
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

        public ErrorInfo SetExpectation(string label)
        {
            if (string.IsNullOrEmpty(label))
                return new ErrorInfo(this.Position, Enumerable.Empty<string>(), this.Message);
            else
                return new ErrorInfo(this.Position, new List<string>() {label}, this.Message);
        }
    }

    /// <summary>
    /// The output result for the parser.
    /// Works on lists of type Token.
    /// </summary>
    /// <typeparam name="T">The type of result stored.</typeparam>
    /// <typeparam name="U">The type of object used to represent the input.  The type of parser state.</typeparam>
    public class ParseResult<T, U>
    {
        public bool Succeeded;

        // if Succeeded, all fields below are used, else only ErrorInfo is used
        public ErrorInfo ErrorInfo;

        readonly T result;
        readonly ParserState<U> startingInput;
        readonly ParserState<U> remainingInput;

        public T Result
        {
            get { if (!this.Succeeded) throw new InvalidOperationException(); return this.result; }
        }

        public ParserState<U> StartingInput
        {
            get { if (!this.Succeeded) throw new InvalidOperationException(); return this.startingInput; }
        }

        public ParserState<U> RemainingInput
        {
            get { if (!this.Succeeded) throw new InvalidOperationException(); return this.remainingInput; }
        }

        public ParseResult(T result, ParserState<U> startingInput, ParserState<U> remainingInput)
        {
            this.result = result;
            this.startingInput = startingInput;
            this.remainingInput = remainingInput;
        }

        public ParseResult(T result, ParserState<U> startingInput, ParserState<U> remainingInput, ErrorInfo errorInfo)
        {
            this.result = result;
            this.startingInput = startingInput;
            this.remainingInput = remainingInput;
            this.ErrorInfo = errorInfo;
            this.Succeeded = true;
        }

        public ParseResult(ErrorInfo errorInfo)
        {
            this.ErrorInfo = errorInfo;
        }

        public ParseResult<T, U> MergeError(ErrorInfo otherError)
        {
            if (this.Succeeded)
                return new ParseResult<T, U>(this.Result, this.StartingInput, this.RemainingInput, this.ErrorInfo.Merge(otherError));
            else
                return new ParseResult<T, U>(this.ErrorInfo.Merge(otherError));
        }

        public ParseResult<T, U> SetExpectation(string label)
        {
            if (this.Succeeded)
                return new ParseResult<T, U>(this.Result, this.StartingInput, this.RemainingInput, this.ErrorInfo.SetExpectation(label));
            else
                return new ParseResult<T, U>(this.ErrorInfo.SetExpectation(label));
        }

        public override bool Equals(object obj)
        {
            ParseResult<T, U> other = obj as ParseResult<T, U>;

            if (other == null)
                return false;
            if (!object.Equals(other.Result, this.Result))
                return false;
            if (!object.Equals(other.RemainingInput, this.RemainingInput))
                return false;
            if (!object.Equals(other.StartingInput, this.StartingInput))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return this.RemainingInput.GetHashCode() ^ this.StartingInput.GetHashCode();
        }
    }

    /// <summary>
    /// Parse result with info about whether it consumed any input.
    /// </summary>
    /// <typeparam name="T">The type of result object the ParseResult stores.</typeparam>
    /// <typeparam name="U">The type of object used to represent the ParserState.</typeparam>
    public class Consumed<T, U>
    {
        public bool HasConsumedInput;
        public ParseResult<T, U> ParseResult;

        public Consumed(bool hasConsumedInput, ParseResult<T, U> result)
        {
            this.HasConsumedInput = hasConsumedInput;
            this.ParseResult = result;
        }

        public Consumed<T, U> Tag(string label)
        {
            if (this.HasConsumedInput)
                return this;

            return new Consumed<T, U>(this.HasConsumedInput, this.ParseResult.SetExpectation(label));
        }

        public override string ToString()
        {
            return this.ParseResult.ToString();
        }
    }


    // Representation type for parsers
    public delegate Consumed<T, U> P<T, U>(ParserState<U> input);

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
