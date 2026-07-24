using clinic.Repositories.Interfaces;
using clinic.Services.Interfaces;

namespace clinic.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _repo;

        public PermissionService(IPermissionRepository repo) => _repo = repo;

        public async Task<bool> HasPermissionAsync(string roleName, string moduleName, string action)
        {
            var permissions = await _repo.GetForRoleAsync(roleName);
            var permission = permissions.FirstOrDefault(p => p.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

            if (permission == null)
                return false;

            return action switch
            {
                "View" => permission.CanView,
                "Create" => permission.CanCreate,
                "Edit" => permission.CanEdit,
                "Delete" => permission.CanDelete,
                _ => false
            };
        }

        public async Task<IEnumerable<object>> GetForRoleAsync(string roleName)
        {
            var permissions = await _repo.GetForRoleAsync(roleName);
            return permissions.Select(p => new
            {
                p.ModuleName,
                p.CanView,
                p.CanCreate,
                p.CanEdit,
                p.CanDelete
            }).ToList();
        }
    }
}
