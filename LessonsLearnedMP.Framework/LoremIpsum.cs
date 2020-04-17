using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suncor.LessonsLearnedMP.Framework
{
    public static class LoremIpsum
    {
        private static string[] words = new string[] { "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod",
            "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua",
            "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita",
            "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet",
            "lorem", "ipsum", "dolor", "sit", "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod",
            "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua",
            "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita",
            "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet",
            "lorem", "ipsum", "dolor", "sit", "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod",
            "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua",
            "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita",
            "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet", "duis",
            "autem", "vel", "eum", "iriure", "dolor", "in", "hendrerit", "in", "vulputate", "velit", "esse", "molestie",
            "consequat", "vel", "illum", "dolore", "eu", "feugiat", "nulla", "facilisis", "at", "vero", "eros", "et",
            "accumsan", "et", "iusto", "odio", "dignissim", "qui", "blandit", "praesent", "luptatum", "zzril", "delenit",
            "augue", "duis", "dolore", "te", "feugait", "nulla", "facilisi", "lorem", "ipsum", "dolor", "sit", "amet",
            "consectetuer", "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet",
            "dolore", "magna", "aliquam", "erat", "volutpat", "ut", "wisi", "enim", "ad", "minim", "veniam", "quis",
            "nostrud", "exerci", "tation", "ullamcorper", "suscipit", "lobortis", "nisl", "ut", "aliquip", "ex", "ea",
            "commodo", "consequat", "duis", "autem", "vel", "eum", "iriure", "dolor", "in", "hendrerit", "in", "vulputate",
            "velit", "esse", "molestie", "consequat", "vel", "illum", "dolore", "eu", "feugiat", "nulla", "facilisis", "at",
            "vero", "eros", "et", "accumsan", "et", "iusto", "odio", "dignissim", "qui", "blandit", "praesent", "luptatum",
            "zzril", "delenit", "augue", "duis", "dolore", "te", "feugait", "nulla", "facilisi", "nam", "liber", "tempor",
            "cum", "soluta", "nobis", "eleifend", "option", "congue", "nihil", "imperdiet", "doming", "id", "quod", "mazim",
            "placerat", "facer", "possim", "assum", "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing",
            "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam",
            "erat", "volutpat", "ut", "wisi", "enim", "ad", "minim", "veniam", "quis", "nostrud", "exerci", "tation",
            "ullamcorper", "suscipit", "lobortis", "nisl", "ut", "aliquip", "ex", "ea", "commodo", "consequat", "duis",
            "autem", "vel", "eum", "iriure", "dolor", "in", "hendrerit", "in", "vulputate", "velit", "esse", "molestie",
            "consequat", "vel", "illum", "dolore", "eu", "feugiat", "nulla", "facilisis", "at", "vero", "eos", "et", "accusam",
            "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita", "kasd", "gubergren", "no", "sea",
            "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet", "lorem", "ipsum", "dolor", "sit",
            "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod", "tempor", "invidunt", "ut",
            "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua", "at", "vero", "eos", "et",
            "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita", "kasd", "gubergren", "no",
            "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet", "lorem", "ipsum", "dolor", "sit",
            "amet", "consetetur", "sadipscing", "elitr", "at", "accusam", "aliquyam", "diam", "diam", "dolore", "dolores",
            "duo", "eirmod", "eos", "erat", "et", "nonumy", "sed", "tempor", "et", "et", "invidunt", "justo", "labore",
            "stet", "clita", "ea", "et", "gubergren", "kasd", "magna", "no", "rebum", "sanctus", "sea", "sed", "takimata",
            "ut", "vero", "voluptua", "est", "lorem", "ipsum", "dolor", "sit", "amet", "lorem", "ipsum", "dolor", "sit",
            "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod", "tempor", "invidunt", "ut",
            "labore", "et", "dolore", "mag\na", "aliquyam", "erat", "consetetur", "sadipscing", "elitr", "sed", "diam",
            "nonumy", "eirmod", "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed",
            "diam", "voluptua", "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea",
            "rebum", "stet", "clita", "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum" };

        private static string[] linkSuffix = new string[] { "com", "net", "org" };

        public const string ParagraphOpenTag = "&lt;P&gt;";
        public const string ParagraphCloseTag = "&lt;/P&gt;";
        public const string AnchorTag = "&lt;A href='http://www.{0}.{1}'&gt;http://www.{0}.{1}&lt;/A&gt;";
        private static Random random = new Random();

        public static string GetWords(int paragraphs, int wordsPerParagraph, bool includeLinks, bool useHtml = true)
        {
            var result = new StringBuilder();

            bool nextWordUpper = false;

            result.Append((useHtml ? ParagraphOpenTag : "") + "Lorem ipsum");

            for (int i = 0; i < paragraphs; i++)
            {
                bool linkAppended = false;

                int nextParagraphSize = random.Next(wordsPerParagraph - 5) + 5;
                for (int j = 0; j < nextParagraphSize; j++)
                {
                    string word = " " + words[random.Next(words.Length)];

                    if (nextWordUpper)
                    {
                        nextWordUpper = false;
                        word = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[random.Next(words.Length)]);
                    }

                    result.Append(word);

                    if (random.Next(wordsPerParagraph) % 3 == 0)
                    {
                        result.Append(",");
                    }
                    else if (random.Next(wordsPerParagraph) % 7 == 0)
                    {
                        nextWordUpper = true;
                        result.Append(".  ");
                    }
                    else if (!linkAppended && includeLinks)
                    {
                        linkAppended = true;
                        if (useHtml)
                        {
                            result.Append(" " + string.Format(AnchorTag, words[random.Next(words.Length)], linkSuffix[random.Next(linkSuffix.Length)]));
                        }
                        else
                        {
                            result.Append(" " + string.Format("http://www.{0}.{1}", words[random.Next(words.Length)], linkSuffix[random.Next(linkSuffix.Length)]));
                        }
                    }
                }

                if (result[result.Length - 1] == ',' || result[result.Length - 1] == '.')
                {
                    result.Remove(result.Length - 1, 1);
                }

                nextWordUpper = true;
                result.Append(". ");

                if (useHtml)
                {
                    result.Append(ParagraphCloseTag);
                }
            }

            return result.ToString();
        }

        public static DateTime RandomDay(DateTime startDate, int maxDays)
        {
            return startDate.AddDays(random.Next(maxDays)).AddHours(random.Next(12)).AddMinutes(random.Next(59)).AddSeconds(random.Next(59));
        }

        public static DateTime RandomDay()
        {
            DateTime start = new DateTime(1995, 1, 1);
            return RandomDay(start, ((TimeSpan)(DateTime.Today - start)).Days);
        }

        public static int RandomNumber(int max)
        {
            int result = random.Next(max);
            return result == 0 ? 1 : result;
        }
    }
}
