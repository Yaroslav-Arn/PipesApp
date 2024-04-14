using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipesApp.Contexts;
using PipesApp.DTOs;
using PipesApp.Models;

namespace PipesApp.Controllers
{
    [ApiController]
    [Route("api/Pipes")]
    public class PipesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        public PipesController(ApplicationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<List<PipeDto>> GetPipes()
        {
            List<Pipe> Pipes = _context.Pipes.ToList();
            List<PipeDto> PipeDtos = new List<PipeDto>();

            foreach (var Pipe in Pipes)
            {
                var PipeDto = _mapper.Map<PipeDto>(Pipe);

                PipeDtos.Add(PipeDto);
            }

            return PipeDtos;
        }


        [HttpGet("{id}")]
        public IActionResult GetPipe(int id)
        {
            var Pipe = _context.Pipes.Find(id);
            if (Pipe == null)
            {
                return NotFound();
            }
            var PipeDto = _mapper.Map<PipeDto>(Pipe);
            return Ok(PipeDto);
        }

        [HttpPost]
        public IActionResult CreatePipe(PipeDto pipeDto)
        {
            if (ModelState.IsValid)
            {

                if (pipeDto.SteelGradeId == 0 || !_context.SteelGrades.Any(sg => sg.Id == pipeDto.SteelGradeId))
                {
                    ModelState.AddModelError("SteelGradeId", "Неверный идентификатор марки стали");
                    return BadRequest(ModelState);
                }

                // Проверяем существование PackageId (если он не равен 0) и его корректность
                if (pipeDto.PackageId != 0 && !_context.Packages.Any(p => p.Id == pipeDto.PackageId))
                {
                    ModelState.AddModelError("PackageId", "Неверный идентификатор пакета");
                    return BadRequest(ModelState);
                }

                var pipe = _mapper.Map<Pipe>(pipeDto);
                if(pipe.PackageId == 0)
                {
                    pipe.PackageId = null;
                }
                _context.Pipes.Add(pipe);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetPipe), new { id = pipe.Id }, pipe);
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public IActionResult UpdatePipe(PipeDto pipeDto)
        {
            var pipe = _mapper.Map<Pipe>(pipeDto);
            if (pipe.PackageId == 0)
            {
                pipe.PackageId = null;
            }
            var existingPipe = _context.Pipes.FirstOrDefault(u => u.Id == pipe.Id);
            if (existingPipe == null)
            {
                ModelState.AddModelError("Id", "Такой трубы не существует");
                return BadRequest(ModelState);
            }

            // Проверяем, находится ли труба в пакете
            if (existingPipe.PackageId != null && pipe.PackageId != null)
            {
                ModelState.AddModelError("Пакет", "Редактировать можно только трубу без пакета");
                return BadRequest(ModelState);
            }

            // Обновляем свойства существующей трубы
            existingPipe.Quality = pipe.Quality;
            existingPipe.Diameter = pipe.Diameter;
            existingPipe.SteelGradeId = pipe.SteelGradeId;
            existingPipe.Length = pipe.Length;
            existingPipe.PackageId = pipe.PackageId;
            existingPipe.Thickness = pipe.Thickness;
            existingPipe.Weight = pipe.Weight;

            _context.SaveChanges();

            return CreatedAtAction("GetPipe", new { id = existingPipe.Id }, existingPipe);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePipe(int id)
        {
            var pipe = _context.Pipes.Find(id);
            if (pipe == null)
            {
                return NotFound();
            }

            // Проверяем, находится ли труба в пакете
            if (pipe.PackageId != null)
            {
                // Если труба находится в пакете, возвращаем ошибку
                return BadRequest("Нельзя удалить трубу, которая находится в пакете.");
            }

            _context.Pipes.Remove(pipe);
            _context.SaveChanges();

            return NoContent();
        }
    }
}