using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PipesApp.Contexts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов в контейнер.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Настройка аутентификации с помощью куки
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; 
    });


var app = builder.Build();

// Настройка HTTP запросов.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Добавляем middleware для аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async (HttpContext context) =>
{
    // Читаем содержимое файла login.html
    var htmlContent = await File.ReadAllTextAsync("wwwroot/login.html");

    // Отправляем содержимое файла как ответ на запрос
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(htmlContent);
});

app.MapPost("/login", async (string? returnUrl, HttpContext context) =>
{
    var login = context.Request.Form["login"].ToString();
    var password = context.Request.Form["password"].ToString();
    using (var dbContext = context.RequestServices.GetRequiredService<ApplicationContext>())
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Login) };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            if(user.Role == "Admin")
            {
                return Results.Redirect(returnUrl??"/admin.html");
            }
            else
            {
                return Results.Redirect(returnUrl??"/user.html");
            }
            
        }
        else
        {
            return Results.Redirect(returnUrl??"/login");
        }
    }
});
// Для маршрута "/admin"
app.MapGet("/admin", [Authorize] async (HttpContext context) =>
{
    // Читаем содержимое файла admin.html
    var htmlContent = await File.ReadAllTextAsync("wwwroot/admin.html");

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(htmlContent);
});

// Для маршрута "/user"
app.MapGet("/user", [Authorize] async (HttpContext context) =>
{
    // Читаем содержимое файла user.html
    var htmlContent = await File.ReadAllTextAsync("wwwroot/user.html");

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(htmlContent);
});

app.MapControllers();

app.Run();
