using System;
using System.Text;
using System.Data;
using System.Drawing;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Class for collecting modifications to text to be displayed in
    /// various rich text boxes.  An object of this class specifies
    /// a segment of an overall text string (usually held in a
    /// StringBuilder object) by start position and length, and notes
    /// whether it should be Bold or Underlined (underline will only
    /// work with bold already in place), and what Color to give it.
    /// </summary>
    public class StringMods
    {
        private Color underlyingColor = Color.Black;

        public int Start { get; set; }
        public int Length { get; set; }
        public bool Bold { get; set; }
        public bool Underline { get; set; }

        public Color Color
        {
            get
            {
                return underlyingColor;
            }
            set
            {
                underlyingColor = Color.FromArgb(value.R, value.G, value.B);
            }
        }
    }
}
