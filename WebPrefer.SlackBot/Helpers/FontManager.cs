using Microsoft.AspNetCore.Hosting;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPrefer.SlackBot.Helpers
{
    public class FontManager
    {
        public Font DefaultFont { get; }

        public FontManager(IWebHostEnvironment env)
        {
            FontCollection collection = new FontCollection();
            var fontFamily = collection.Install(env.GetFontPath("OpenSans-Bold.ttf"));
            DefaultFont = fontFamily.CreateFont(50.0f, FontStyle.Bold);
        }
    }
}
