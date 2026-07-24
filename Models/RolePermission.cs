namespace clinic.Models
{
    public class RolePermission
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int ModuleId { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public string ModuleName { get; set; } = string.Empty;
    }
}
