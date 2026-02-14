using Leo.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baby.AudioData.InterfaceWeb
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

            services.AddControllersWithViews().AddNewtonsoftJson();

            //添加HttpClient
            services.AddHttpClient();

            //添加接口
            services.AddInterfaceResult();

            //添加HttpContext
            services.AddHttpContextAccessor();

            //健康检查不能去掉
            services.AddHealthChecks();

            //添加内存缓存
            services.AddMemoryCache();

            //添加HttpContext操作
            services.AddHttpContextOP();
            //添加跨域请求
            if (EnvironmentHelper.IsLocalhostOrDevelopment)
            {
                services.AddCors(options => options.AddDefaultPolicy(
                policy => policy.SetIsOriginAllowed((x) => true)
                 .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
            }
            else
            {
                services.AddCors(options => options.AddDefaultPolicy(
            policy => policy.SetIsOriginAllowedToAllowWildcardSubdomains()
            .WithOrigins("https://*.xxxx.com", "http://*.xxxx.com")
            .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
            }

            //全局日志
            services.AddGlobalExceptionFilter();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //修正双//地址
            app.UseCorrectPath();

            app.UseHttpContext();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (env.IsProduction())
            {
                app.UseAppStoppingSleep(20000);
            }


            app.UseStaticFiles();

            //监听请求性能一定要放在UseStaticFiles后面，避免静态文件也监听进去
            app.UseTelemetryUrl();
            app.UseRouting();
            app.UseCors();


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


                //健康检查不能去掉
                endpoints.MapHealthChecks("/health");


            });
        }
    }
}
