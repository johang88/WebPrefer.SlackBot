using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebPrefer.SlackBot.Helpers
{
    public class MemeDatabase : IEnumerable<string>
    {
        public IReadOnlyList<string> Images { get; }
        private IWebHostEnvironment _env;

        public MemeDatabase(IWebHostEnvironment env)
        {
            _env = env;
            Images = Directory.GetFiles(env.GetMemesPath(), "*.jpg").Select(Path.GetFileName).ToList();
        }

        public bool TryGetPath(string name, out string path)
        {
            if (Images.Contains(name))
            {
                path = _env.GetMemePath(name);
                return true;
            }
            else
            {
                path = null;
                return false;
            }
        }

        public IEnumerator<string> GetEnumerator()
            => Images.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Images.GetEnumerator();
    }
}
