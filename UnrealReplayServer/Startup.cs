/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using UnrealReplayServer.Databases;
using Microsoft.EntityFrameworkCore;
using System;

namespace UnrealReplayServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.OutputFormatters.Insert(0, new BinaryOutputFormatter());
            });
            services.AddOptions();

            string envConnString = Environment.GetEnvironmentVariable("DB_CON_URL");
            string connectionString = !string.IsNullOrEmpty(envConnString) ? envConnString : Configuration.GetValue<string>("ApplicationDefaults:ConnectionString");

            services.AddDbContextPool<DatabaseContext>(
                options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            services.AddScoped<ISessionDatabase, SessionDatabase>();
            services.AddScoped<IEventDatabase, EventDatabase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Task.Run(async () => {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    context.Database.Migrate();
                    context.Database.EnsureCreated();

                    var sessionDatabase = scope.ServiceProvider.GetRequiredService<ISessionDatabase>();
                    await sessionDatabase.DoWorkOnStartup();
                }
            });
        }
    }
}
