using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rex.Stores
{
    public interface IRoleAssignmentStore
    {
        Task<Models.RoleAssignment?> GetRoleAssignment(Guid collectionId, Guid userId);

        IAsyncEnumerable<Models.RoleAssignment> GetRoleAssignments(Guid collectionId);

        Task<bool> RemoveRoleAssignmentAsync(Guid collectionId, Guid userId);

        Task<Models.RoleAssignment> StoreRoleAssignmentAsync(Models.RoleAssignment assignment);
    }
}