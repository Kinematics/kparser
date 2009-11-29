using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Monad
{
    // The output class for lexer
    public class LexResult<T>
    {
        public readonly T Result;
        public readonly string RemainingInput;

        public LexResult(T r, string ri) { this.Result = r; this.RemainingInput = ri; }

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

    // The output class for parsers
    public class ParseResult<T>
    {
        public readonly T Result;
        public readonly List<string> RemainingInput;

        public ParseResult(T r, List<string> ri) { this.Result = r; this.RemainingInput = ri; }

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
}
