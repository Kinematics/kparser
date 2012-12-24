using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser.Utility
{
    public class RTFConverter
    {
        #region Tracking Variables
        List<string> colorsList = new List<string>();
        List<string> fontList = new List<string>();
        #endregion

        #region Public access functions for this utility class

        public string ConvertRTFToHTML(string rtfText)
        {
            return ConvertRTFToHTMLImpl(rtfText);
        }

        public string ConvertRTFToBBCode(string rtfText)
        {
            return ConvertRTFToBBCodeImpl(rtfText);
        }

        #endregion

        #region Private implementations >> HTML
        private string ConvertRTFToHTMLImpl(string rtfText)
        {
            rtfText = rtfText.Trim();

            if (rtfText == string.Empty)
                return string.Empty;

            ValidateRTF(rtfText);

            Regex content = new Regex(
                @"{\\rtf1[\\\w]+(?<fontInfo>{\\fonttbl{[^}]*}})?([\r\n]*)(?<colorInfo>{\\colortbl[\s\\\w\d;]*})?([\r\n]*)(?<innerText>.*)}([\r\n]*)$",
                RegexOptions.Singleline);

            Match contentMatch = content.Match(rtfText);
            string htmlString = string.Empty;

            if (contentMatch.Success == true)
            {
                LoadFontTable(contentMatch.Groups["fontInfo"].Value);
                LoadColorTable(contentMatch.Groups["colorInfo"].Value);

                htmlString += HTMLHeader();

                string innerText = contentMatch.Groups["innerText"].Value;
                htmlString += ConvertToHTML(innerText);

                htmlString += HTMLFooter();
            }

            return htmlString;
        }

        private string ConvertToHTML(string rtfText)
        {
            Regex r1 = new Regex(@"(.*?)\\", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            Regex r2 = new Regex(@"([\{a-z]+)([0-9]*) *[\r\n]*", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            Match m;
            string textString;
            char nextChar;
            bool firstSpanUsed = false;

            StringBuilder sb = new StringBuilder();

            int idx = 0;

            while (idx < rtfText.Length)
            {
                // Get any text up to a '\'. 
                m = r1.Match(rtfText, idx);
                if (m.Length == 0) break;

                // text will be empty if we have adjacent control words
                textString = m.Groups[1].Value;
                if (textString.Length > 0)
                    sb.Append(Escape(textString));

                idx += m.Length;

                // check for RTF escape characters. According to the spec, these are the only escaped chars.
                nextChar = rtfText[idx];
                if (nextChar == '{' || nextChar == '}' || nextChar == '\\')
                {
                    // Escaped char
                    sb.Append(nextChar);
                    idx++;
                    continue;
                }

                // Must be a control char. @todo- delimeter includes more than just space, right?
                m = r2.Match(rtfText, idx);
                string stCtrlWord = m.Groups[1].Value;
                string stCtrlParam = m.Groups[2].Value;

                switch (stCtrlWord)
                {
                    case "cf":
                        // Set font color.
                        int iColor = Int32.Parse(stCtrlParam);
                        if (firstSpanUsed)
                            sb.Append("</span>"); // close previous span, and start a new one for the given color.

                        sb.AppendFormat("<span style=\"color: {0}\">", colorsList[iColor]);
                        firstSpanUsed = true;
                        break;
                    case "f":
                        // Sets font.
                        break;
                    case "fs":
                        // Sets font size.
                        break;
                    case "par":
                        // This is a newline.
                        //sb.Append("<br>\n");
                        sb.Append("\n");
                        break;
                    case "b":
                        // Bold text
                        if (stCtrlParam == "0")
                            sb.Append("</b>");
                        else
                            sb.Append("<b>");
                        break;
                    case "i":
                        // Italic text
                        if (stCtrlParam == "0")
                            sb.Append("</i>");
                        else
                            sb.Append("<i>");
                        break;
                    case "ul":
                        // Underline text
                        sb.Append("<u>");
                        break;
                    case "ulnone":
                        // Stop underlining.
                        sb.Append("</u>");
                        break;
                    default:
                        break;
                }

                idx += m.Length;
            }

            return sb.ToString();
        }

        private void ValidateRTF(string rtfText)
        {
            string trimmedText = rtfText.Trim();

            if (trimmedText.StartsWith(@"{\rtf1") == false)
                throw new InvalidOperationException("RTF text does not start with proper tag.");

            if (trimmedText.EndsWith("}") == false)
                throw new InvalidOperationException("RTF text does not end with closing brace.");
        }

        private void LoadFontTable(string rtfText)
        {
            fontList.Clear();

            // {\fonttbl{\f0\fnil\fcharset0 Courier New;}}

            Regex fontTableRegex = new Regex(@"{\\fonttbl{\\f\d\\f\w+\\fcharset\d+ (?<font>\w+( \w+)*);[^}]*}}");

            Match fontMatch = fontTableRegex.Match(rtfText);

            if (fontMatch.Success == false)
                throw new InvalidOperationException("Font table failed to match.");

            fontList.Add(fontMatch.Groups["font"].Value);
        }

        private void LoadColorTable(string rtfText)
        {
            colorsList.Clear();
            colorsList.Add("#000000");

            // {\colortbl ;\red128\green0\blue128;\red0\green0\blue255;\red0\green0\blue0;\red0\green128\blue0;\red255\green0\blue255;\red128\green128\blue128;}

            Regex colorTableRegex = new Regex(@"{\\colortbl ;(?<colors>(\\red\d+\\green\d+\\blue\d+;)+)}");

            Match colorTableMatch = colorTableRegex.Match(rtfText);

            if (colorTableMatch.Success == false)
            {
                return;
            }

            string colorGroupMatch = colorTableMatch.Groups["colors"].Value;

            string delimStr = ";";
            char [] delimiter = delimStr.ToCharArray();

            string[] colorGroups = colorGroupMatch.Split(delimiter);

            Regex colorRegex = new Regex(@"\\red(?<red>\d+)\\green(?<green>\d+)\\blue(?<blue>\d+)");
            Match colorMatch;

            string hexCode;
            int redVal, greenVal, blueVal;

            foreach (string colorGroup in colorGroups)
            {
                colorMatch = colorRegex.Match(colorGroup);

                if (colorMatch.Success == true)
                {
                    redVal = int.Parse(colorMatch.Groups["red"].Value);
                    greenVal = int.Parse(colorMatch.Groups["green"].Value);
                    blueVal = int.Parse(colorMatch.Groups["blue"].Value);

                    hexCode = string.Format("#{0:x2}{1:x2}{2:x2}", redVal, greenVal, blueVal);
                    colorsList.Add(hexCode);
                }
            }
        }

        private string HTMLHeader()
        {
            string htmlHeader = "<!DOCTYPE html><html>\n";
            htmlHeader += string.Format("<pre style=\"font-family: '{0}'\">\n", fontList.First());

            return htmlHeader;
        }

        private string HTMLFooter()
        {
            string htmlFooter = "</pre></html>";
            return htmlFooter;
        }

        // Escape HTML chars
        private string Escape(string st)
        {
            st = st.Replace("&", "&amp;");
            st = st.Replace("<", "&lt;");
            st = st.Replace(">", "&gt;");
            return st;
        }

        #endregion

        #region Private implementations >> BBCode
        private string ConvertRTFToBBCodeImpl(string rtfText)
        {
            rtfText = rtfText.Trim();

            if (rtfText == string.Empty)
                return string.Empty;

            ValidateRTF(rtfText);

            Regex content = new Regex(
                @"{\\rtf1[\\\w]+(?<fontInfo>{\\fonttbl{[^}]*}})?([\r\n]*)(?<colorInfo>{\\colortbl[\s\\\w\d;]*})?([\r\n]*)(?<innerText>.*)}([\r\n]*)$",
                RegexOptions.Singleline);

            Match contentMatch = content.Match(rtfText);
            string htmlString = string.Empty;

            if (contentMatch.Success == true)
            {
                LoadFontTable(contentMatch.Groups["fontInfo"].Value);
                LoadColorTable(contentMatch.Groups["colorInfo"].Value);

                htmlString += BBCodeHeader();

                string innerText = contentMatch.Groups["innerText"].Value;
                htmlString += ConvertToBBCode(innerText);

                htmlString += BBCodeFooter();
            }

            return htmlString;
        }

        private string ConvertToBBCode(string rtfText)
        {
            Regex r1 = new Regex(@"(.*?)\\", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            Regex r2 = new Regex(@"([\{a-z]+)([0-9]*) *[\r\n]*", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            Match m;
            string textString;
            char nextChar;

            StringBuilder sb = new StringBuilder();

            int idx = 0;

            while (idx < rtfText.Length)
            {
                // Get any text up to a '\'. 
                m = r1.Match(rtfText, idx);
                if (m.Length == 0) break;

                // text will be empty if we have adjacent control words
                textString = m.Groups[1].Value;
                if (textString.Length > 0)
                    sb.Append(Escape(textString));

                idx += m.Length;

                // check for RTF escape characters. According to the spec, these are the only escaped chars.
                nextChar = rtfText[idx];
                if (nextChar == '{' || nextChar == '}' || nextChar == '\\')
                {
                    // Escaped char
                    sb.Append(nextChar);
                    idx++;
                    continue;
                }

                // Must be a control char. @todo- delimeter includes more than just space, right?
                m = r2.Match(rtfText, idx);
                string stCtrlWord = m.Groups[1].Value;
                string stCtrlParam = m.Groups[2].Value;

                switch (stCtrlWord)
                {
                    case "cf":
                        // Set font color.
                        // There is no consistant way to do this in BBCode. Omitting.
                        break;
                    case "f":
                        // Sets font.
                        break;
                    case "fs":
                        // Sets font size.
                        break;
                    case "par":
                        // This is a newline.
                        //sb.Append("<br>\n");
                        sb.Append("\n");
                        break;
                    case "b":
                        // Bold text
                        if (stCtrlParam == "0")
                            sb.Append("[/b]");
                        else
                            sb.Append("[b]");
                        break;
                    case "i":
                        // Italic text
                        if (stCtrlParam == "0")
                            sb.Append("[/i]");
                        else
                            sb.Append("[i]");
                        break;
                    case "ul":
                        // Underline text
                        sb.Append("[u]");
                        break;
                    case "ulnone":
                        // Stop underlining.
                        sb.Append("[/u]");
                        break;
                    default:
                        break;
                }

                idx += m.Length;
            }

            return sb.ToString();
        }

        private string BBCodeHeader()
        {
            return "[pre]";
        }

        private string BBCodeFooter()
        {
            return "[/pre]";
        }

        #endregion
    }
}
