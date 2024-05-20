using CC_Backend.Data;
using CC_Backend.Handlers;
using CC_Backend.Models;
using CC_Backend.Repositories.Friends;
using CC_Backend.Repositories.Stamps;
using CC_Backend.Repositories.User;
using CC_Backend.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

namespace CC_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            DotNetEnv.Env.Load();

            var services = builder.Services;
            var configuration = builder.Configuration;

            // Register controllers
            builder.Services.AddControllers();

            // Add services to the container.

            builder.Services.AddAuthorization();
            string connectionString = builder.Configuration.GetConnectionString("CONNECTION_STRING");
            //string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            builder.Services.AddDbContext<NatureAIContext>(opt => opt.UseSqlServer(connectionString));

            // Add Identity services
            builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();
            builder.Services.AddAuthorizationBuilder();

            builder.Services.AddIdentityCore<ApplicationUser>()
                .AddEntityFrameworkStores<NatureAIContext>()
                .AddApiEndpoints();


            var AllowLocalhostOrigin = "_allowLocalhostOrigin";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    policy =>
                    {
                        policy.WithOrigins("http://127.0.0.1:5500/")
                         .AllowAnyHeader()
                         .AllowAnyMethod()
                         .AllowCredentials();
                    });
            });

            
           // Set up Google SSO.
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENTID");
                googleOptions.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENTSECRET");
            });


            // JWT Authentication Configuration
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRETKEY");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });


           

            // Dependency injection:
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
            builder.Services.AddSingleton<IOpenAIService>(x => new OpenAIService(apiKey));
            builder.Services.AddSingleton<MimeKit.MimeMessage>();
            builder.Services.AddScoped<IStampsRepo, StampsRepo>();
            builder.Services.AddScoped<IFriendsRepo, FriendsRepo>();
            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<IStampHandler, StampHandler>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IJwtAuthManager>(provider =>
            {
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                return new JwtAuthManager(userManager, configuration, secretKey);
            });
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ILogger, Logger<AccountService>>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var routeTemplate = apiDesc.RelativePath;
                    var endpointsToHide = new List<string>
                    {
                        "refresh",
                        "confirmEmail",
                        "resendConfirmationEmail",
                        "forgotPassword",
                        "resetPassword",
                        "manage/2fa",
                        "manage/info",
                        "manage/info",
                        "register",
                        "login"
                    };
                    foreach (var endpoint in endpointsToHide)
                    {
                        if (routeTemplate == endpoint)
                            return false;
                    }
                    return true;
                });
            });


            var app = builder.Build();

            app.MapIdentityApi<ApplicationUser>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapControllerRoute(
            name: "logout",
            pattern: "logout",
            defaults: new { controller = "Logout", action = "Logout" });


            app.UseHttpsRedirection();


            app.UseCors(AllowLocalhostOrigin);

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}