using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PipesApp.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов в контейнер.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление обслуживания статических файлов
builder.Services.AddRazorPages(); // Этот метод добавляет обслуживание файлов, таких как Razor страницы, но также включает и обслуживание статических файлов из wwwroot.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var app = builder.Build();

// Конфигурация конвейера HTTP-запросов.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

// Включение обслуживания статических файлов из папки wwwroot.
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path == "/login" && context.Request.Method == "POST") // Проверка, обращается ли пользователь к странице входа и отправляет ли POST-запрос
    {
        var login = context.Request.Form["login"].ToString();
        var password = context.Request.Form["password"].ToString();

        // Проверка логина и пароля в базе данных (предположим, что у нас есть контекст данных ApplicationContext)
        using (var dbContext = context.RequestServices.GetRequiredService<ApplicationContext>())
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                if (user.Role == "User")
                {
                    // Аутентификация прошла успешно, перенаправляем на index.html
                    context.Response.Redirect("/user.html");
                }
                else if (user.Role == "Admin")
                {
                    // Аутентификация прошла успешно, перенаправляем на index.html
                    context.Response.Redirect("/admin.html");
                }
                return;
            }
        }

        // Если аутентификация не удалась, перенаправляем обратно на страницу входа
        // Пользователь не найден, отправляем сообщение об ошибке на фронтенд
        context.Response.Redirect("/login.html");
        return;
    }
    await next();
});

app.MapControllers();

app.Run();