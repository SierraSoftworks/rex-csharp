using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rex.Stores
{
    public interface IRoleAssignmentStore
    {
        Task<Models.RoleAssignment> GetRoleAssignment(Guid userId, Guid collectionId);

        IAsyncEnumerable<Models.RoleAssignment> GetRoleAssignments(Guid userId);

        Task<bool> RemoveRoleAssignmentAsync(Guid userId, Guid collectionId);

        Task<Models.RoleAssignment> StoreRoleAssignmentAsync(Models.RoleAssignment assignment);
    }
}