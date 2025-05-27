using Media.Control.App.Api.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// HTTPS 사용 설정 (ASP.NET Core 6 이상)
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 5050; // 원하는 포트로 설정

});

builder.WebHost.UseUrls("http://0.0.0.0:5000", "http://0.0.0.0:5050");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5050); // HTTP
    //options.ListenAnyIP(5050, listenOptions => listenOptions.UseHttps()); // HTTPS
});
// SignalR 서비스 등록
builder.Services.AddSignalR();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


var app = builder.Build();

app.MapHub<LogHub>("/loghub");  // Hub 경로 설정

app.MapHub<MediaHub>("/mediahub");  // Hub 경로 설정

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Media Control App API V1");
        c.RoutePrefix = "swagger";  // http://IP:PORT/swagger
    });
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // 컨트롤러 매핑
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
