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
        public static async Task Main(string[] args)
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
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("LibrarianOnly", policy =>
                    policy.RequireRole("Librarian"));
                options.AddPolicy("StudentOnly", policy =>
                    policy.RequireRole("Student"));
            });


            builder.Services.AddIdentity<User, IdentityRole>(options =>
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
                options.SignIn.RequireConfirmedEmail = false;
            

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

            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    try
            //    {
            //        var context = services.GetRequiredService<LibraryAppContext>();
            //        await context.Database.MigrateAsync();  // Apply migrations

            //        // Call the seeding method
            //        await SeedAdminUserAndRolesAsync(services);
            //    }
            //    catch (Exception ex)
            //    {
            //        var logger = services.GetRequiredService<ILogger<Program>>();
            //        logger.LogError(ex, "An error occurred during seeding.");
            //    }
            //}


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

            async Task SeedAdminUserAndRolesAsync(IServiceProvider services)
            {
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // Create roles if they don't exist
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                if (!await roleManager.RoleExistsAsync("Student"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Student"));
                }

                // Check if the admin user already exists
                var adminUser = await userManager.FindByEmailAsync("admin@library.com");
                if (adminUser == null)
                {
                    // Create the admin user
                    var newAdmin = new User
                    {
                        UserName = "sindi@sastricollege.com",
                        FirstName = "Sindi",
                        LastName = "Ngcobo",
                        Email = "sindi@sastricollege.com",
                        EmailConfirmed = true,
                        Role = "Admin",
                        UserIdNumber = "8605760980763"
                    };
                    try
                    {
                        var result = await userManager.CreateAsync(newAdmin, "Admin@123");
                        if(result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(newAdmin, "Admin");
                        }
                      
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                     


                }
            }
        }
    }
}