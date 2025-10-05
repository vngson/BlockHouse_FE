namespace BlockHouse.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ServiceCreateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
    }

    public class ServiceUpdateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
    }

    public class ServiceFilterRequest
    {
        public int page { get; set; } = 1;
        public int page_size { get; set; } = 15;
        public string? keyword { get; set; }
    }
}