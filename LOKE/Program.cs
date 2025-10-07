using Core.Configuration;
using Core.Model.Settings;
using LOKE.Models.Dto;
using LOKE.Models.Dto.ApplicationDto;
using LOKE.Models.Model;
using LOKE.Models.Model.ApplicationModel;
using System.Text.Json;

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
    services.AddControllers().AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
    services.AddAuthorization();
    services.AddCoreAuth<ApplicationUser, ApplicationRole>(configuration);

    // AutoMapper
    services.AutoMap(
        (typeof(ApplicationUser), typeof(ApplicationUserDto)),
        (typeof(ContactInfo), typeof(ContactInfoDto)),
        (typeof(PostModel), typeof(PostDto)),
        (typeof(CommentModel), typeof(CommentDto)),
        (typeof(FriendModel), typeof(FriendDto)),
        (typeof(ConversationModel), typeof(ConversationDto)),
        (typeof(MessageContent), typeof(MessageContentDto)),
        (typeof(UserPostModel), typeof(UserPostDto))
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
            Models = [typeof(FriendModel), typeof(PostModel),typeof(ConversationModel), typeof(UserPostModel)]
        }
    );

    services.AddFileStorage(configuration);


    // Swagger
    services.AddSwaggerService();

    services.AddRealtime(opt =>
    {
        opt.UseRedis = false; // true nếu bạn scale-out nhiều server
        opt.HubPath = "/hubs/app";
        opt.RedisConnectionString = "localhost:6379"; // chỉ cần khi UseRedis = true
    });
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
    app.MapRealtimeHub();
}
