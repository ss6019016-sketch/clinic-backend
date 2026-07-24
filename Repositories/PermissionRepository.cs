using Dapper;
using clinic.Data;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly DapperContext _context;

        public PermissionRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<RolePermission>> GetAllAsync()
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT rp.Id, rp.RoleName, rp.ModuleId, rp.CanView, rp.CanCreate, rp.CanEdit, rp.CanDelete,
                       m.Name AS ModuleName
                FROM RolePermissions rp
                JOIN Modules m ON rp.ModuleId = m.Id
                ORDER BY rp.RoleName, m.Name";

            return await db.QueryAsync<RolePermission>(sql);
        }

        public async Task<IEnumerable<RolePermission>> GetForRoleAsync(string roleName)
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT rp.Id, rp.RoleName, rp.ModuleId, rp.CanView, rp.CanCreate, rp.CanEdit, rp.CanDelete,
                       m.Name AS ModuleName
                FROM RolePermissions rp
                JOIN Modules m ON rp.ModuleId = m.Id
                WHERE rp.RoleName = @RoleName
                ORDER BY m.Name";

            return await db.QueryAsync<RolePermission>(sql, new { RoleName = roleName });
        }

        public async Task<bool> UpdateAsync(int id, bool canView, bool canCreate, bool canEdit, bool canDelete)
        {
            using var db = _context.CreateConnection();
            var rows = await db.ExecuteAsync(@"
                UPDATE RolePermissions
                SET CanView = @CanView,
                    CanCreate = @CanCreate,
                    CanEdit = @CanEdit,
                    CanDelete = @CanDelete
                WHERE Id = @Id",
                new { Id = id, CanView = canView, CanCreate = canCreate, CanEdit = canEdit, CanDelete = canDelete });
            return rows > 0;
        }
    }
}
