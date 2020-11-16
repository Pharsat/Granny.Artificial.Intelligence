using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Granny.Email.Application;
using Granny.Email.Application.Classification;
using Granny.Email.Application.EmailInformationExtractor;
using Granny.Email.Application.Integration;
using Granny.Email.Application.Preparation;
using Granny.Email.Application.Repository;
using Granny.Email.Infrastructure;
using Granny.Email.Infrastructure.Integration;
using Granny.Email.Infrastructure.Repository.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Granny.Email.WebApp
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
            services.AddControllersWithViews();

            services.Configure<EmailReceiverConfiguration>(Configuration.GetSection("EmailReceiver"));

            string mongoDbConnectionString = this.Configuration.GetConnectionString("GrannyMongo");

            services.AddSingleton<IMailRepository, MailKitMailRepository>();
            services.AddScoped<IClassificationService, ClassificationService>();
            services.AddScoped<IInformationExtractorService, InformationExtractorService>();
            services.AddScoped<IStopWordsServices, StopWordsServices>();
            services.AddScoped<ITextPreparerService, TextPreparerService>();
            services.AddScoped<IGrannyRepository>(_ => new GrannyMongoRepository(mongoDbConnectionString));
            services.AddScoped<IGrannyModelAccessorService, GrannyPythonModelAccessorService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=MailReceiver}/{action=Index}/{id?}");
            });
        }
    }
}
