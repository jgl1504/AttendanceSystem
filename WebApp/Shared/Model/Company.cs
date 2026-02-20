namespace WebApp.Shared.Model
{
    public class Company : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

}
