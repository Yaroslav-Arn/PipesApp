using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using PipesApp.Contexts;
using PipesApp.Models;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PipesApp.Controllers
{
    [ApiController]
    [Route("api/Users")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        public UsersController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public IActionResult CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли пользователь с таким логином
                if (_context.Users.Any(u => u.Login == user.Login))
                {
                    ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
                    return BadRequest(ModelState);
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _context.Users.Add(user);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public IActionResult UpdateUser(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null)
            {
                ModelState.AddModelError("Id", "Такого пользователя не существует");
                return BadRequest(ModelState);
            }

            // Проверяем, не занят ли указанный логин другим пользователем
            var userWithSameLogin = _context.Users.FirstOrDefault(u => u.Login == user.Login && u.Id != user.Id);
            if (userWithSameLogin != null)
            {
                ModelState.AddModelError("Login", "Такой логин занят");
                return BadRequest(ModelState);
            }

            // Генерируем хеш пароля
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // Обновляем свойства существующего пользователя
            existingUser.Login = user.Login;
            existingUser.Password = passwordHash;
            existingUser.Role = user.Role;

            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUser), new { id = existingUser.Id }, existingUser);
        }



        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok();
        }
    }
}
