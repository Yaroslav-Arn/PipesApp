using System.Security.Cryptography.X509Certificates;

namespace PipesApp.Models
{
    public class Package
    {
        public int Id { get; set; } // Номер пакета
        public string? Remark { get; set; } // Пояснение 
        public DateTime PackageDate { get; set; } // Дата создания пакета

        // Навигационное свойство для обратной связи с Pipe
        public List<Pipe>? Pipes { get; set; }


    }
}
