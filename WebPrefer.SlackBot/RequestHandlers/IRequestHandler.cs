using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebPrefer.SlackBot.RequestHandlers
{
    public interface IRequestHandler
    {
        Task Handle(HttpContext context);
    }
}