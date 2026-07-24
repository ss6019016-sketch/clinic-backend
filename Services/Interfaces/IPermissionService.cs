namespace clinic.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string roleName, string moduleName, string action);
        Task<IEnumerable<object>> GetForRoleAsync(string roleName);
    }
}
