using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<CartService>();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ShopException ex)
    {
        context.Response.StatusCode = ex.StatusCode;
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
});

app.MapControllers();
app.Run();
