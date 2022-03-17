/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UnrealReplayServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddConsole();
                    builder.AddTraceSource("debug");
                    builder.AddTraceSource("information");
                    builder.AddTraceSource("warning");
                    builder.AddTraceSource("error");
                });
    }
}
