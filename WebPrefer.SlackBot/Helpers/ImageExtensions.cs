using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPrefer.SlackBot.Helpers
{
    public static class ImageExtensions
    {
        public const float DPI = 72;
        public const float Margin = 5;

        public static void DrawText(this Image image, Font font, string text, bool bottom)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var rect = TextMeasurer.Measure(text, new RendererOptions(font, DPI)
            {
                WrappingWidth = image.Width,
            });

            var position = new PointF(
                0.0f,
                bottom ? image.Height - Margin - rect.Height : Margin);

            image.Mutate(x => x.DrawText(new TextGraphicsOptions
            {
                TextOptions = new TextOptions
                {
                    DpiX = DPI,
                    DpiY = DPI,
                    WrapTextWidth = image.Width,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            }, text, font, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 2.0f), position));
        }
    }
}
