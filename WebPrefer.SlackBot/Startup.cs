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
using WebPrefer.SlackBot.Commands;

namespace WebPrefer.SlackBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSlackNet(c => c
                .UseApiToken(Configuration["Slack:ApiToken"])
                .RegisterSlashCommandHandler<MemeCommandHandler>("/meme")
            ); 
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSlackNet(c => c
                .UseSigningSecret(Configuration["Slack:SigningSecret"])
                .MapToPrefix("slack")
            );

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

                    if (context.Request.Query.ContainsKey("text"))
                        bottomText = context.Request.Query["text"];

                    DrawText(topText, false);
                    DrawText(bottomText, true);

                    context.Response.ContentType = "image/jpg";
                    await image.SaveAsync(context.Response.Body, new JpegEncoder());

                    void DrawText(string text, bool bottom)
                    {
                        if (string.IsNullOrWhiteSpace(text))
                            return;

                        var margin = 5.0f;

                        var dpi = 72.0f;
                        var rect = TextMeasurer.Measure(text, new RendererOptions(font, dpi)
                        {
                            WrappingWidth = image.Width,
                        });

                        var position = new PointF(
                            0.0f,
                            bottom ? image.Height - margin - rect.Height : margin);

                        image.Mutate(x => x.DrawText(new TextGraphicsOptions
                        {
                            TextOptions = new TextOptions
                            {
                                DpiX = dpi,
                                DpiY = dpi,
                                WrapTextWidth = image.Width,
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }, text, font, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 2.0f), position));
                    }
                });
            });
        }
    }
}
