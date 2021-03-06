﻿using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.ReDoc
{
    public class ReDocIndexMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ReDocOptions _options;

        public ReDocIndexMiddleware(RequestDelegate next, ReDocOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!RequestingReDocIndex(httpContext.Request))
            {
                await _next(httpContext);
                return;
            }

            RespondWithIndexHtml(httpContext.Response);
        }

        private bool RequestingReDocIndex(HttpRequest request)
        {
            return (request.Method == "GET" && request.Path == $"/{_options.RoutePrefix}/");
        }

        private async void RespondWithIndexHtml(HttpResponse response)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";

            using (var stream = _options.IndexStream())
            {
                // Inject parameters before writing to response
                var htmlBuilder = new StringBuilder(new StreamReader(stream).ReadToEnd());
                foreach (var entry in GetIndexParameters())
                {
                    htmlBuilder.Replace(entry.Key, entry.Value);
                }

                await response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
            }
        }

        private IDictionary<string, string> GetIndexParameters()
        {
            return new Dictionary<string, string>()
            {
                { "%(DocumentTitle)", _options.DocumentTitle },
                { "%(HeadContent)", _options.HeadContent },
                { "%(SpecUrl)", _options.SpecUrl }
            };
        }
    }
}
