﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Areas.Identity.Services;
using ILA_Server.Data;
using ILA_Server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.SwaggerGeneration.Processors.Security;

namespace ILA_Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IJwtHandler, JwtHandler>();
            services.AddTransient<TokenManagerMiddleware>();
            services.AddTransient<ITokenManager, TokenManager>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (HostingEnvironment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                // TODO: in production Redis Cache
            }

            var jwtSection = Configuration.GetSection("jwt");
            var jwtOptions = new JwtOptions();
            jwtSection.Bind(jwtOptions);

            services.AddDbContext<ILADbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = false,
                        ValidateLifetime = true
                    };
                });

            services.AddIdentity<ILAUser, IdentityRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
                o.Password.RequiredLength = 7;
            }).AddEntityFrameworkStores<ILADbContext>()
                .AddDefaultTokenProviders();
            services.Configure<JwtOptions>(jwtSection);

            services.AddMvc()
                .AddJsonOptions(
                    options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                )
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Register the Swagger services
            services.AddSwaggerDocument(config =>
            {
                config.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT"));
                config.DocumentProcessors.Add(new SecurityDefinitionAppender("JWT", new List<string>(), new SwaggerSecurityScheme
                {
                    Type = SwaggerSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    BearerFormat = "JWT ",
                    In = SwaggerSecurityApiKeyLocation.Header,
                }));
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "FOrg.Server API";
                    document.Info.Description = "FOrg is an software helps you to organize and make some things easier.";
                };
            });
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUi3();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseAuthentication();
            app.UseMiddleware<TokenManagerMiddleware>();
            app.UseMvc();

            string forceHttps = Environment.GetEnvironmentVariable("FORCEHTTPS");
            if (string.IsNullOrWhiteSpace(forceHttps) || forceHttps != "0")
            {
                app.UseHttpsRedirection();
                app.UseSwagger(cfg => cfg.PostProcess = (settings, _) => settings.Schemes = new[] { SwaggerSchema.Https });
            }
            else
            {
                app.UseSwagger();
            }
        }
    }
}