using System.ComponentModel.DataAnnotations.Schema;

namespace PipesApp.Models
{
    public class Pipe
    {
        // Обязательные данные о трубе
        public int Id { get; set; } // Номер трубы
        public bool Quality { get; set; } // Качество
        public int Length { get; set; } // Размеры: длинна
        public double Thickness { get; set; } // Размеры: толщина
        public double Diameter { get; set; } // Размеры: диаметр

        public double Weight { get; set; } // Вес

        // Внешний ключ для связи с Package
        [ForeignKey("Package")]
        public int? PackageId { get; set; }
        public Package? Package { get; set; }

        // Внешний ключ для связи с SteelGrade
        [ForeignKey("SteelGrade")]
        public int SteelGradeId { get; set; }
        public SteelGrade? SteelGrade { get; set; }


    }
}
