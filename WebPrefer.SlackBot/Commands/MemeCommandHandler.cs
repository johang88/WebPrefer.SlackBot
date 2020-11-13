using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.Interaction;
using SlackNet.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPrefer.SlackBot.Commands
{
    public class MemeCommandHandler : ISlashCommandHandler
    {
        private readonly ILogger<MemeCommandHandler> _logger;

        public MemeCommandHandler(ILogger<MemeCommandHandler> logger)
            => _logger = logger;

        public async Task<SlashCommandResponse> Handle(SlashCommand command)
        {
            _logger.LogInformation($"Processing command: {command.Command} {command.Text} from: {command.UserName}");

            var commands = ParseCommand(command.Text);

            if (!commands.Any())
            {
                return new SlashCommandResponse
                {
                    ResponseType = ResponseType.Ephemeral,
                    Message = new Message
                    {
                        Text = @"Usage /meme <type> <top text> <bottom text>"
                    }
                };
            }

            var meme = commands[0];
            var top = commands.Count >= 2 ? commands[1] : "";
            var bottom = commands.Count >= 23 ? commands[2] : "";

            return new SlashCommandResponse
            {
                ResponseType = ResponseType.InChannel,

                Message = new Message
                {
                    AsUser = true,
                    Username = command.UserName,
                    Attachments = new List<Attachment>()
                    {
                        new Attachment
                        {
                            ImageUrl = $"https://webpreferslackbot.azurewebsites.net/meme/{meme}?top={Uri.EscapeUriString(top)}&bottom={Uri.EscapeUriString(bottom)}"
                        }
                    },
                }
            };

            static List<string> ParseCommand(string text)
            {
                var result = new List<string>();

                // TODO: Make parsing not crappy
                // should probably make a more generic command tokenizer + parser
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
                            AddResult(text[index..i]);
                            index = i + 1;
                            inQuote = false;
                            break;
                        case '"' when !inQuote:
                            index = i + 1;
                            inQuote = true;
                            break;
                    }
                }

                AddResult(text[index..]);

                return result;

                void AddResult(string res)
                {
                    res = res.Trim();
                    if (res.Length > 0)
                        result.Add(res);
                }
            }
        }
    }
}
