using Media.Control.App.Api.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// HTTPS ��� ���� (ASP.NET Core 6 �̻�)
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 5050; // ���ϴ� ��Ʈ�� ����

});

builder.WebHost.UseUrls("http://0.0.0.0:5000", "http://0.0.0.0:5050");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5050); // HTTP
    //options.ListenAnyIP(5050, listenOptions => listenOptions.UseHttps()); // HTTPS
});
// SignalR ���� ���
builder.Services.AddSignalR();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


var app = builder.Build();

app.MapHub<LogHub>("/loghub");  // Hub ��� ����

app.MapHub<MediaHub>("/mediahub");  // Hub ��� ����

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
    endpoints.MapControllers(); // ��Ʈ�ѷ� ����
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
