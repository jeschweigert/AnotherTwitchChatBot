using NTextCat;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Reflection;

namespace ATCB.Library.Models.Misc
{
    public class Translator
    {
        public TranslatedText Result { get; private set; }
        public bool IsComplete { get; private set; }

        public Translator()
        {
            Result = null;
            IsComplete = false;
        }

        public void Translate(string inputText, string languagePair)
        {
            IsComplete = false;
            Result = null;
            var url = String.Format("https://translate.google.com/#{0}/{1}", languagePair.Replace('|', '/'), inputText);
            var th = new Thread(() => {
                WebBrowser wb = new WebBrowser();
                wb.ScriptErrorsSuppressed = true;
                wb.Navigate(url);
                while (wb.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();
                var textResult = WaitForContents(WaitForChildren(wb.Document.GetElementById("result_box")).FirstChild).InnerHtml;
                var bunchaButtons = wb.Document.GetElementById("gt-sl-sugg").FirstChild;
                var langResult = bunchaButtons.Children[bunchaButtons.Children.Count - 1].InnerText.Replace(" - detected", "");
                Result = new TranslatedText(textResult, langResult);
                IsComplete = true;
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        public string DetectLanguage(string inputText)
        {
            var factory = new RankedLanguageIdentifierFactory();
            var identifier = factory.Load(LanguageIdentifier.GetCore14());

            var languages = identifier.Identify(inputText);
            var mostCertainLanguage = languages.FirstOrDefault();
            if (mostCertainLanguage != null)
                return mostCertainLanguage.Item1.Iso639_3;
            else
                return String.Empty;
        }

        private HtmlElement WaitForChildren(HtmlElement element)
        {
            while (element.Children.Count == 0) { }
            return element;
        }

        private HtmlElement WaitForContents(HtmlElement element)
        {
            while (element.InnerHtml == null) { }
            return element;
        }
    }

    public class TranslatedText
    {
        public TranslatedText(string text, string language)
        {
            Text = text;
            Language = language;
        }

        public string Text { get; private set; }

        public string Language { get; private set; }
    }

    public static class LanguageIdentifier
    {
        public static Stream GetCore14()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("ATCB.Library.additional.Core14.profile.xml");
        }
    }
}
