namespace PipesApp.DTOs
{
    public class PipeDto
    {
        public int Id { get; set; } // Номер трубы
        public bool Quality { get; set; } // Качество
        public int Length { get; set; } // Размеры: длинна
        public double Thickness { get; set; } // Размеры: толщина
        public double Diameter { get; set; } // Размеры: диаметр

        public double Weight { get; set; } // Вес

        public int SteelGradeId { get; set; } //Марка стали
        public int? PackageId { get; set; } // Номер пакета
    }
}
