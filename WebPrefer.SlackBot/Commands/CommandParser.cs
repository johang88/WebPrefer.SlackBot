using System.Collections.Generic;

namespace WebPrefer.SlackBot.Commands
{
    public static class CommandParser
    {
        public static IReadOnlyList<string> Parse(string text)
        {
            var result = new List<string>();

            // TODO: Make parsing not crappy
            // should probably make a more generic command tokenizer + parser
            // Maybe try out span and stuff
            var inQuote = false;
            var index = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var token = text[i];

                switch (token)
                {
                    case ' ' when !inQuote:
                        AddResult(text[index..i]);
                        index = i + 1;
                        break;
                    case '"' when inQuote:
                        inQuote = false;
                        break;
                    case '"' when !inQuote:
                        inQuote = true;
                        break;
                }
            }

            AddResult(text[index..]);

            return result;

            void AddResult(string res)
                => result.Add(res.Trim('"'));
        }
    }
}
