using Sastri_Library_Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Sastri_Library_Backend.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Sastri_Library_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<LibraryAppContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // Add Identity services
            builder.Services.AddSingleton<JwtHelper>();

            // JWT configuration
            var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"]);

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
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\": \"Unauthorized\"}");
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\": \"Forbidden\"}");
                    }
                };
            });

            // Add authorization services
            builder.Services.AddAuthorization();


            builder.Services.AddIdentity<Student, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;                     // Require at least one digit
                options.Password.RequireLowercase = true;                 // Require lowercase characters
                options.Password.RequireUppercase = true;                 // Require uppercase characters
                options.Password.RequireNonAlphanumeric = true;           // Require special characters
                options.Password.RequiredLength = 6;                      // Minimum password length
                options.Password.RequiredUniqueChars = 3;                 // Require at least 3 unique characters

                // User settings
                options.User.RequireUniqueEmail = true;                   // Email must be unique

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = false;              // Require confirmed email for sign-in

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Lockout time for 5 minutes
                options.Lockout.MaxFailedAccessAttempts = 5;              // Lockout after 5 failed attempts
                options.Lockout.AllowedForNewUsers = true;                // Allow lockout for new users
            }).AddEntityFrameworkStores<LibraryAppContext>()
            .AddDefaultTokenProviders();



            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost3000",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000")
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                    });
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Enable CORS
            app.UseCors("AllowLocalhost3000");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
            {
                if (context.Request.Cookies.TryGetValue("Bearer", out var token))
                {
                    // Add the token to the Authorization header in "Bearer [token]" format
                    context.Request.Headers.Add("Authorization", $"Bearer {token}");
                }

                await next();
            });


            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}