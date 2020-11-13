using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SlackNet.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebPrefer.SlackBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) 
            => Configuration = configuration;
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSlackNet(c => c.UseApiToken(Configuration["Slack:ApiToken"]));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSlackNet(c => c.UseSigningSecret(Configuration["Slack:SigningSecret"]));

            app.UseRouting();

            var collection = new FontCollection();
            var fontFamily = collection.Install(env.GetFontPath("OpenSans-Bold.ttf"));
            var font = fontFamily.CreateFont(50.0f, FontStyle.Bold);

            var images = Directory.GetFiles(env.GetMemesPath(), "*.jpg").Select(Path.GetFileName).ToArray();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/meme/{image}", async context =>
                {
                    var imageName = (string)context.Request.RouteValues["image"] + ".jpg";
                    if (!images.Contains(imageName))
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("invalid image");
                        return;
                    }

                    var imagePath = env.GetMemePath(imageName);
                    using var stream = File.OpenRead(imagePath);
                    using var image = await Image.LoadAsync(stream);

                    var topText = context.Request.Query["top"];
                    var bottomText = context.Request.Query["bottom"];

                    if (!string.IsNullOrWhiteSpace(topText))
                        DrawText(topText, false);

                    if (!string.IsNullOrWhiteSpace(bottomText))
                        DrawText(bottomText, true);

                    context.Response.ContentType = "image/jpg";
                    await image.SaveAsync(context.Response.Body, new JpegEncoder());

                    image.SaveAsJpeg(context.Response.Body);

                    void DrawText(string text, bool bottom)
                    {
                        var margin = 5.0f;

                        var rect = TextMeasurer.Measure(text, new RendererOptions(font));
                        
                        var position = new PointF(
                            image.Width / 2.0f - rect.Width / 2.0f,
                            bottom ? image.Height - margin - rect.Height : margin);

                        image.Mutate(x => x.DrawText(text, font, Color.White, position));
                    }
                });
            });
        }
    }
}
