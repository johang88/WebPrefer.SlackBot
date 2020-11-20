using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebPrefer.SlackBot.Helpers;
using System.IO;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;

namespace WebPrefer.SlackBot.RequestHandlers
{
    [Route("GET", "/meme/{image}")]
    public class MemeRequestHandler : IRequestHandler
    {
        private readonly FontManager _fontManager;
        private readonly MemeDatabase _memeDatabase;

        public MemeRequestHandler(FontManager fontManager, MemeDatabase memeDatabase)
        {
            _fontManager = fontManager ?? throw new ArgumentNullException(nameof(fontManager));
            _memeDatabase = memeDatabase ?? throw new ArgumentNullException(nameof(memeDatabase));
        }

        public async Task Handle(HttpContext context)
        {
            var imageName = (string)context.Request.RouteValues["image"] + ".jpg";
            if (!_memeDatabase.TryGetPath(imageName, out var imagePath))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("invalid image");
                return;
            }

            using var stream = File.OpenRead(imagePath);
            using var image = await Image.LoadAsync(stream);

            var topText = context.Request.Query["top"];
            var bottomText = context.Request.Query["bottom"];

            if (context.Request.Query.ContainsKey("text"))
                bottomText = context.Request.Query["text"];

            image.DrawText(_fontManager.DefaultFont, topText, false);
            image.DrawText(_fontManager.DefaultFont, bottomText, true);

            context.Response.ContentType = "image/jpg";
            await image.SaveAsync(context.Response.Body, new JpegEncoder());
        }
    }
}
