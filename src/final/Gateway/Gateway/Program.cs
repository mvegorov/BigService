using Gateway;
using Gateway.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

WebApplicationBuilder builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.Configure<ServiceConnectionSettings>(builder.Configuration.GetSection("ServiceConnectionSettings"));
builder.Services.Configure<ServiceConnectionSettings>(builder.Configuration.GetSection("Lab5ToolsConnectionSettings"));
builder.Services.AddGrpcClients();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();

app.UseMiddleware<GrpcExceptionHandlingMiddleware>();
app.UseRouting();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
