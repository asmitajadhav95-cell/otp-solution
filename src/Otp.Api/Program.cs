using Otp.Api.Middleware;
using Otp.Application;
using Otp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddCors(options => options.AddPolicy("ReactClient", policy =>
    policy.WithOrigins(builder.Configuration["ClientUrl"] ?? "http://localhost:5173")
          .AllowAnyHeader()
          .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "OTP API v1"));
}
app.UseCors("ReactClient");
app.MapControllers();
app.Run();