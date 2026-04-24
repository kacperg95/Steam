using FuzzySharp;
using System.Text;
using System.Text.RegularExpressions;

namespace Services
{
    public class SmartCensor
    {
        public string Censor(string text, string forbiddenPhrase, int threshold = 90)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(forbiddenPhrase))
                return text;

            string cleanTarget = Regex.Replace(forbiddenPhrase, @"[:\-\[\]]", " ").ToLower().Trim();
            string[] reviewWords = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            bool[] toCensor = new bool[reviewWords.Length];


            //Short names
            if (cleanTarget.Length <= 5)
            {
                for (int i = 0; i < reviewWords.Length; i++)
                {
                    string cleanWord = Regex.Replace(reviewWords[i], @"[^\w]", "").ToLower();
                    if (cleanWord == cleanTarget)
                    {
                        toCensor[i] = true;
                    }
                }
            }
            else //Longer names
            {
                int targetWordsCount = cleanTarget.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                //Try to match fragments of the review with the target, starting from single words up to the full phrase
                for (int size = 1; size <= targetWordsCount + 1; size++)
                {
                    for (int i = 0; i <= reviewWords.Length - size; i++)
                    {
                        string currentFragment = string.Join(" ", reviewWords.Skip(i).Take(size));
                        string cleanFragment = Regex.Replace(currentFragment, @"[^\w\s]", "").ToLower();

                        int score = Fuzz.WeightedRatio(cleanTarget, cleanFragment);

                        if (score >= threshold)
                            for (int j = 0; j < size; j++) toCensor[i + j] = true;
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < reviewWords.Length; i++)
            {
                if (toCensor[i])
                    sb.Append(new string('*', reviewWords[i].Length));
                else
                    sb.Append(reviewWords[i]);

                sb.Append(' ');
            }

            return sb.ToString().Trim();
        }
    }
}
