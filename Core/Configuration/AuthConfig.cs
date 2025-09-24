using AspNetCore.Identity.Mongo;
using Core.Extention;
using Core.Model.Aplication;
using Core.Model.Settings;
using Core.Service.DeployService;
using Core.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Core.Configuration
{
    public static class AuthConfig
    {
        public static IServiceCollection AddCoreAuth<TUser, TRole>(
            this IServiceCollection services, IConfiguration configuration)
            where TUser : BaseUser, new()
            where TRole : BaseRole
        {
            var dbSetting1 = configuration.GetSection("MongoSettings:DatabaseLoke1")
                                 .Get<MongoDbSettings>();

            if (dbSetting1 == null || string.IsNullOrEmpty(dbSetting1!.ConnectionString))
                throw new ArgumentNullException("MongoDb connection string is not configured");

            services.AddIdentityMongoDbProvider<TUser, TRole,string>(
                    mongoOptions =>
                    {
                        mongoOptions.ConnectionString = $"{dbSetting1.ConnectionString}/{dbSetting1.DatabaseName}";
                        mongoOptions.UsersCollection = "Users";
                        mongoOptions.RolesCollection = "Roles";
                    });

            services.Configure<IdentityOptions>(options => {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            });


            // Cấu hình thêm User options nếu cần
            services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = true;
            });

            // JWT
            var jwtSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSection);
            var jwtSettings = jwtSection.Get<JwtSettings>()!;
            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

            services.AddAuthentication(options =>
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
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                // Cho SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddScoped<IAuthService<TUser>, AuthService<TUser>>();
            services.AddScoped<IUserService<TUser>, UserService<TUser, TRole>>();

            return services;
        }
    }
}