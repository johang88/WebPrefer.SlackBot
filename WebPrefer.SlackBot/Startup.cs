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
using System.Reflection;
using System.Threading.Tasks;
using WebPrefer.SlackBot.Commands;
using WebPrefer.SlackBot.Helpers;
using WebPrefer.SlackBot.Middleware;
using WebPrefer.SlackBot.RequestHandlers;

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

            foreach (var (type, routeAttribute) in FindRequestHandlers())
            {
                services.AddTransient(type);
            }
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

            app.UseEndpoints(endpoints =>
            {
                // Wait, why are we not just using like a regular controller ?? don't know :D
                // and this is more fun, everyone loves custom solutions
                foreach (var (type, routeAttribute) in FindRequestHandlers())
                {
                    var capturedType = type;
                    endpoints.MapMethods(routeAttribute.Route, new string[] { routeAttribute.Method }, async context =>
                    {
                        var handler = (IRequestHandler)app.ApplicationServices.GetRequiredService(capturedType);
                        await handler.Handle(context);
                    });
                }
            });
        }

        private static IEnumerable<(Type, RouteAttribute)> FindRequestHandlers()
            => Assembly.GetExecutingAssembly().GetTypes()
                .Where(typeof(IRequestHandler).IsAssignableFrom)
                .Where(x => x.GetCustomAttribute<RouteAttribute>() != null)
                .Select(x => (x, x.GetCustomAttribute<RouteAttribute>()));
    }
}
