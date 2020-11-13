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
using WebPrefer.SlackBot.Helpers;
using WebPrefer.SlackBot.Middleware;

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

            services.AddSingleton<MemeDatabase>();
            services.AddSingleton<FontManager>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseSlackNet(c => c
                .UseSigningSecret(Configuration["Slack:SigningSecret"])
                .MapToPrefix("slack")
            );

            app.UseRouting();

            var fontManager = app.ApplicationServices.GetService<FontManager>();
            var memes = app.ApplicationServices.GetService<MemeDatabase>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/meme/{image}", async context =>
                {
                    var imageName = (string)context.Request.RouteValues["image"] + ".jpg";
                    if (!memes.TryGetPath(imageName, out var imagePath))
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

                    image.DrawText(fontManager.DefaultFont, topText, false);
                    image.DrawText(fontManager.DefaultFont, bottomText, true);

                    context.Response.ContentType = "image/jpg";
                    await image.SaveAsync(context.Response.Body, new JpegEncoder());
                });
            });
        }
    }
}
