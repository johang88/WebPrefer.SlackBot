using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebPrefer.SlackBot
{
    public static class IWebHostEnvironmentExtensions
    {
        public static string GetFontsPath(this IWebHostEnvironment env)
            => Path.Combine(env.WebRootPath, "fonts/");

        public static string GetFontPath(this IWebHostEnvironment env, string name)
            => Path.Combine(env.GetFontsPath(), name);

        public static string GetMemesPath(this IWebHostEnvironment env)
            => Path.Combine(env.WebRootPath, "memes/");

        public static string GetMemePath(this IWebHostEnvironment env, string name)
            => Path.Combine(env.GetMemesPath(), name);
    }
}
