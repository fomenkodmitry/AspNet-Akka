using Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using Actor;
using Akka.Actor;
using Api.ServiceScopeExtension;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Domain;
using Domain.DomainToModelProfile;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace Api
{
    public class Startup
    {
        private static readonly string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        private static readonly string AppSettings =
            string.IsNullOrEmpty(Env) ? "appsettings.json" : $"appsettings.{Env}.json";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc(p => p.EnableEndpointRouting = false);

            var key = Encoding.ASCII.GetBytes(Configuration["AppSettings:Secret"]);
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireExpirationTime = false,
                        ValidateLifetime = false
                    };
                });

            var securityScheme = new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                BearerFormat = "Bearer {authToken}",
                Description = "JWT Token",
                Type = SecuritySchemeType.ApiKey
            };
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Portal API", Version = "v1",
                        Description = Configuration["AppSettings:Environment"]
                    });

                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Api.xml"));
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Domain.xml"));

                c.AddSecurityDefinition(
                    "Bearer", securityScheme
                );
                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme, Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
            });

            #region akka

            services.AddSingleton(provider =>
            {
                var serviceScopeFactory = provider.GetService<IServiceScopeFactory>();
                var actorSystem = ActorSystem.Create("bookstore", ConfigurationLoader.Load());
                actorSystem.AddServiceScopeFactory(serviceScopeFactory);
                return actorSystem;
            });

            services.AddSingleton<BooksManagerActorProvider>(provider =>
            {
                var actorSystem = provider.GetService<ActorSystem>();
                var booksManagerActor = actorSystem?.ActorOf(Props.Create(() => new BooksManagerActor()));
                return () => booksManagerActor;
            });
            
            #endregion
            
            services
                .AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Auto Mapper Configurations
            builder.RegisterAutoMapper(p => p.AddProfile(new DomainToModelProfile()));

            #region DI Service

            builder
                .RegisterInstance(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile(AppSettings, optional: true, reloadOnChange: false)
                    .Build()
                )
                .As<IConfigurationRoot>()
                .SingleInstance();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime lifetime
        )
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"); });

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMiddleware<TokenMiddleware>();

            app.UseMvc();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });


            lifetime.ApplicationStarted.Register(() =>
            {
                app.ApplicationServices.GetService<ActorSystem>(); // start Akka.NET
            });
            lifetime.ApplicationStopping.Register(() =>
            {
                app.ApplicationServices.GetService<ActorSystem>()?.Terminate().Wait();
            });
            
            var logger = loggerFactory.CreateLogger("LoggerInStartup");
            logger.LogInformation($"\n\n{DateTime.Now} | Startup logger was launched");
        }

    }
}