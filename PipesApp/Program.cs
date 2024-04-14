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

// ���������� �������� � ���������.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ���������� ������������ ����������� ������
builder.Services.AddRazorPages(); // ���� ����� ��������� ������������ ������, ����� ��� Razor ��������, �� ����� �������� � ������������ ����������� ������ �� wwwroot.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var app = builder.Build();

// ������������ ��������� HTTP-��������.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

// ��������� ������������ ����������� ������ �� ����� wwwroot.
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path == "/login" && context.Request.Method == "POST") // ��������, ���������� �� ������������ � �������� ����� � ���������� �� POST-������
    {
        var login = context.Request.Form["login"].ToString();
        var password = context.Request.Form["password"].ToString();

        // �������� ������ � ������ � ���� ������ (�����������, ��� � ��� ���� �������� ������ ApplicationContext)
        using (var dbContext = context.RequestServices.GetRequiredService<ApplicationContext>())
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                if (user.Role == "User")
                {
                    // �������������� ������ �������, �������������� �� index.html
                    context.Response.Redirect("/user.html");
                }
                else if (user.Role == "Admin")
                {
                    // �������������� ������ �������, �������������� �� index.html
                    context.Response.Redirect("/admin.html");
                }
                return;
            }
        }

        // ���� �������������� �� �������, �������������� ������� �� �������� �����
        // ������������ �� ������, ���������� ��������� �� ������ �� ��������
        context.Response.Redirect("/login.html");
        return;
    }
    await next();
});

app.MapControllers();

app.Run();