namespace PipesApp.Models
{
    public class SteelGrade
    {
        public int Id { get; set; } // id марки стали

        public string Grade { get; set; } // марка стали

        // Навигационное свойство для обратной связи с Pipe
        public List<Pipe>? Pipes { get; set; }

    }
}
