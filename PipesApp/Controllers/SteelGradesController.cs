using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipesApp.Contexts;
using PipesApp.Models;
using PipesApp.DTOs;
using System.IO.Pipelines;
using AutoMapper;
using Microsoft.Extensions.Hosting;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PipesApp.Controllers
{
    [ApiController]
    [Route("api/SteelGrades")] // Изменили маршрут
    public class SteelGradesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        public SteelGradesController(ApplicationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<List<SteelGradeDto>> GetSteelGrades()
        {
            List<SteelGrade> steelGrades = _context.SteelGrades.ToList();
            List<SteelGradeDto> steelGradeDtos = new List<SteelGradeDto>();

            foreach (var steelGrade in steelGrades)
            {
                var steelGradeDto = _mapper.Map<SteelGradeDto>(steelGrade);
                steelGradeDtos.Add(steelGradeDto);
            }

            return steelGradeDtos;
        }


        [HttpGet("{id}")]
        public IActionResult GetSteelGrade(int id)
        {
            var steelGrade = _context.SteelGrades.Find(id);
            if (steelGrade == null)
            {
                return NotFound();
            }
            var steelGradeDto = _mapper.Map<SteelGradeDto>(steelGrade);
            return Ok(steelGradeDto);
        }

        [HttpPost]
        public IActionResult CreateSteelGrade(SteelGradeDto steelGradeDto)
        {
            if (ModelState.IsValid)
            {
                var steelGrade = _mapper.Map<SteelGrade>(steelGradeDto);
                _context.SteelGrades.Add(steelGrade);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetSteelGrade), new { id = steelGrade.Id }, steelGrade);
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public IActionResult UpdateSteelGrade(SteelGradeDto steelGradeDto)
        {
            var steelGrade = _mapper.Map<SteelGrade>(steelGradeDto);
            var existingSteel = _context.SteelGrades.FirstOrDefault(u => u.Id == steelGrade.Id);
            if (existingSteel == null)
            {
                ModelState.AddModelError("Id", "Такой марки не существует");
                return BadRequest(ModelState);
            }

            var steelWithSameGrade = _context.SteelGrades.FirstOrDefault(u => u.Grade == steelGrade.Grade && u.Id != steelGrade.Id);
            if (steelWithSameGrade != null)
            {
                ModelState.AddModelError("Login", "Такая марка стали есть");
                return BadRequest(ModelState);
            }

            existingSteel.Grade = steelGrade.Grade;


            _context.SaveChanges();

            return CreatedAtAction("GetSteelGrade", new { id = existingSteel.Id }, existingSteel);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteSteelGrade(int id)
        {
            var steelGrade = _context.SteelGrades.Find(id);
            if (steelGrade == null)
            {
                return NotFound();
            }

            _context.SteelGrades.Remove(steelGrade);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
