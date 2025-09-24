using Core.Configuration;
using Core.Model.Settings;
using LOKE.Models.Dto;
using LOKE.Models.Dto.ApplicationDto;
using LOKE.Models.Model;
using LOKE.Models.Model.ApplicationModel;

var builder = WebApplication.CreateBuilder(args);

// Tách phần cấu hình service ra hàm riêng
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Tách phần middleware pipeline ra hàm riêng
ConfigureMiddleware(app);

app.Run();


// ====================== CONFIG METHODS =======================

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // OpenAPI
    services.AddOpenApi();

    // Controllers & Auth
    services.AddControllers();
    services.AddAuthorization();
    services.AddCoreAuth<ApplicationUser, ApplicationRole>(configuration);

    // AutoMapper
    services.AutoMap(
        (typeof(ApplicationUser), typeof(ApplicationUserDto)),
        (typeof(ContactInfo), typeof(ContactInfoDto)),
        (typeof(PostModel), typeof(PostDto)),
        (typeof(CommentModel), typeof(CommentDto))
    );

    // Cors + File Storage
    services.AddCorsService();
    services.AddFileStorage(configuration);

    // MongoDB
    var dbSetting = configuration.GetSection("MongoSettings:DatabaseLoke1")
                                 .Get<MongoDbSettings>();

    services.AddMongoService(
        new MongoDbModelMapping
        {
            DbSettings = dbSetting,
            Models = new Type[] { typeof(FriendModel), typeof(PostModel) }
        }
    );

    services.AddFileStorage(configuration);


    // Swagger
    services.AddSwaggerService();
}

void ConfigureMiddleware(WebApplication app) 
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseCors("AllowAll");
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
}
