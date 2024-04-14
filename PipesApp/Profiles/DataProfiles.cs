using AutoMapper;
using PipesApp.Models;
using PipesApp.DTOs;

namespace PipesApp.Profiles
{
    public class DataProfiles : Profile
    {
        public DataProfiles()
        {
            CreateMap<SteelGradeDto, SteelGrade>();
            CreateMap<SteelGrade, SteelGradeDto>();

            CreateMap<PackageDto, Package>();
            CreateMap<Package, PackageDto>();

            CreateMap<PipeDto, Pipe>();
            CreateMap<Pipe, PipeDto>();

        }
    }
}
