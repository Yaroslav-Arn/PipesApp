using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipesApp.Contexts;
using PipesApp.DTOs;
using PipesApp.Models;


namespace PipesApp.Controllers
{
    [ApiController]
    [Route("api/Packages")]

    public class PackagesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        public PackagesController(ApplicationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<List<PackageDto>> GetPackages()
        {
            List<Package> Packages = _context.Packages.ToList();
            List<PackageDto> PackageDtos = new List<PackageDto>();

            foreach (var Package in Packages)
            {
                var PackageDto = _mapper.Map<PackageDto>(Package);
                PackageDtos.Add(PackageDto);
            }

            return PackageDtos;
        }


        [HttpGet("{id}")]
        public IActionResult GetPackage(int id)
        {
            var Package = _context.Packages.Find(id);
            if (Package == null)
            {
                return NotFound();
            }
            var PackageDto = _mapper.Map<PackageDto>(Package);
            return Ok(PackageDto);
        }

        [HttpPost]
        public IActionResult CreatePackage(PackageDto PackageDto)
        {
            if (ModelState.IsValid)
            {
                var Package = _mapper.Map<Package>(PackageDto);
                _context.Packages.Add(Package);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetPackage), new { id = Package.Id }, Package);
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public IActionResult UpdatePackage(PackageDto PackageDto)
        {
            var Package = _mapper.Map<Package>(PackageDto);
            var existingPackage = _context.Packages.FirstOrDefault(u => u.Id == Package.Id);
            if (existingPackage == null)
            {
                ModelState.AddModelError("Id", "Такой марки не существует");
                return BadRequest(ModelState);
            }

            existingPackage.Remark = Package.Remark;
            existingPackage.PackageDate = Package.PackageDate;

            _context.SaveChanges();

            return CreatedAtAction("GetPackage", new { id = existingPackage.Id }, existingPackage);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePackage(int id)
        {
            var Package = _context.Packages.Find(id);
            if (Package == null)
            {
                return NotFound();
            }

            _context.Packages.Remove(Package);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
