using CO4029_BE.Application;
using CO4029_BE.Infrastructure;
using CO4029_BE.API.Contracts.Examples;
using Microsoft.OpenApi.Models;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// 1. Add Services from Layers
// ----------------------------
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

});

// ----------------------------
// 2. Add Controllers & Swagger
// ----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    options.SchemaFilter<ExampleSchemaFilter>();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Please enter token",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});


var app = builder.Build();

// ----------------------------
// 3. Middleware
// ----------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // Để Swagger là trang mặc định khi vào link web
});

if (!app.Environment.IsDevelopment()) 
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAnyOrigin");
app.UseAuthorization();

app.MapControllers();
app.Run();