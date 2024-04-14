namespace PipesApp.DTOs
{
    public class PackageDto
    {
        public int Id { get; set; } // Номер пакета
        public string? Remark { get; set; } // Пояснение 
        public DateTime PackageDate { get; set; } // Дата создания пакета
    }
}
