using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<RolePermission>> GetAllAsync();
        Task<IEnumerable<RolePermission>> GetForRoleAsync(string roleName);
        Task<bool> UpdateAsync(int id, bool canView, bool canCreate, bool canEdit, bool canDelete);
    }
}
