using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LessonsLearnedMP.Framework.Logging;
using Suncor.LessonsLearnedMP.Data;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Mvc.Razor;
using Suncor.LessonsLearnedMP.Framework;

namespace LessonsLearnedMP.Web
{
	//shwe test
	/**********************************************************************************************
                    Project: Cloud Surge Project
                    Author: Mital Chovatiya
                    Date: 13 April 2020
                    Purpose: Convert global.asax to startup.cs to dotnet core
                  * **********************************************************************************************/
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
			services.AddDbContext<LessonsLearnedMPEntities>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
			);

			services.AddDistributedMemoryCache();

			services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromHours(1);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			services.AddControllersWithViews();

			services.AddRazorPages();
			services.AddMvc(option => option.EnableEndpointRouting = true)
				.AddSessionStateTempDataProvider();

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddAuthentication(IISDefaults.AuthenticationScheme);

			//globalization Localization
			services.AddLocalization(options => options.ResourcesPath = "Resources");

			services.AddMvc()
				.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
				.AddDataAnnotationsLocalization();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration, ILoggerFactory loggerFactory)
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
			loggerFactory.AddLog4Net(configuration["Logging:log4net.Config"]);
			app.UseHttpsRedirection();

			app.UseStaticFiles();
			app.UseAuthentication();

			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(
					Path.Combine(Directory.GetCurrentDirectory(), "Content")),
					RequestPath = "/Content"
			});
			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(
					Path.Combine(Directory.GetCurrentDirectory(), "Scripts")),
				RequestPath = "/Scripts"
			});

			app.UseRouting();

			app.UseAuthorization();

			app.UseSession();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");

				endpoints.MapControllerRoute(
					   "Edit", // Route name
					   "Lesson/Edit/{lessonId}", // URL with parameters
					   new { controller = "Lesson", action = "Index", pageAction = Enumerations.PageAction.Edit } // Parameter defaults
				   );

				endpoints.MapControllerRoute(
					   "Submit", // Route name
					   "Lesson/Submit/{lessonId}", // URL with parameters
					   new { controller = "Lesson", action = "Index", pageAction = Enumerations.PageAction.Submit } // Parameter defaults
				   );

				endpoints.MapControllerRoute(
					"Search", // Route name
					"Lesson/Search", // URL with parameters
					new { controller = "Lesson", action = "Index", pageAction = Enumerations.PageAction.Search } // Parameter defaults

				);

				//.MapRoute(name: "api", template: "api/{controller}/{action}/{id?}");
				endpoints.MapControllerRoute(
					name: "Search",
					pattern: "GET/{Lesson}/{Index}/{id?}");

			});

			



		}

	}
}
