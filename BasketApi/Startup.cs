using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BasketApi.Domain;
using BasketApi.Infrastructure;
using BasketApi.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BasketApi
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddTransient<BasketService>();
            services.AddSingleton<IBasketRepository, InMemoryBasketRepository>();
            services.AddSingleton<IProductReadOnlyRepository, ProductReadOnlyStubRepository>();
            services.AddSingleton<IAuthenticationService, AuthenticationServiceStub>();
            services.AddSingleton<IMapper>(AutoMapperConfig.InitializeMapper());

            ConfigureAuthentication(services);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});

            app.UseAuthentication();
            app.UseMvc();
        }
        private static void ConfigureAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer =  AuthenticationServiceStub.Issuer,
                            ValidAudience = AuthenticationServiceStub.Audience,
                            IssuerSigningKey =
                                JwtSecurityKey.Create(AuthenticationServiceStub.SecretValue)
                        };
                    options.Events = new JwtBearerEvents
                    {
                        
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("OnAuthenticationFailed: " +
                                              context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("OnTokenValidated: " +
                                              context.SecurityToken);
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
