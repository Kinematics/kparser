using System;
using System.IO;
using System.Net;
using System.Web;
using System.Globalization;
using System.Web.Script.Serialization;

namespace WaywardGamers.KParser.Utility
{
    public class GoogleAjaxResponse<T>
    {
        public T responseData = default(T);
    }

    public class TranslationResponse
    {
        public string translatedText = string.Empty;
        public object responseDetails = null;
        public HttpStatusCode responseStatus = HttpStatusCode.OK;
    }

    public class Translator
    {
        private static readonly string KParserReferrer = "http://code.google.com/p/kparser/";
        private static readonly string KParserAPIKey =
            "ABQIAAAAnGpFGfvq0hdJJvg_9gSQohSANRzkQPcAI3LX6I2D878iPtT7FRQGrSJPac7JahfqZV2QtwDvCsN-pA";

        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        // Shortened forms

        public static string TranslateText(string inputText, string fromLanguage, string toLanguage)
        {
            return TranslateText(inputText, fromLanguage, toLanguage, KParserReferrer, KParserAPIKey);
        }

        public static string TranslateText(string inputText, CultureInfo fromLanguage, CultureInfo toLanguage)
        {
            return TranslateText(inputText, fromLanguage.TwoLetterISOLanguageName,
                toLanguage.TwoLetterISOLanguageName, KParserReferrer, KParserAPIKey);
        }

        public static string TranslateText(string inputText, CultureInfo fromLanguage, CultureInfo toLanguage, string referrer, string key)
        {
            return TranslateText(inputText, fromLanguage.TwoLetterISOLanguageName, toLanguage.TwoLetterISOLanguageName, referrer, key);
        }

        // Actual call

        public static string TranslateText(string inputText, string fromLanguage,
            string toLanguage, string referrer, string key)
        {
            string requestUrl = string.Format(
                "http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&q={0}&langpair={1}|{2}&key={3}",
                System.Web.HttpUtility.UrlEncode(inputText),
                fromLanguage.ToLowerInvariant(),
                toLanguage.ToLowerInvariant(),
                key
            );

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
            req.Referer = referrer;

            try
            {
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                string responseJson = new StreamReader(res.GetResponseStream()).ReadToEnd();

                GoogleAjaxResponse<TranslationResponse> translation = serializer.Deserialize<GoogleAjaxResponse<TranslationResponse>>(responseJson);

                if (translation != null && translation.responseData != null && translation.responseData.responseStatus == HttpStatusCode.OK)
                {
                    string decodedTranslation = HttpUtility.HtmlDecode(translation.responseData.translatedText);

                    return decodedTranslation;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch
            {
                //TODO: Add error handling code here.

                return string.Empty;
            }
        }
    }
}

