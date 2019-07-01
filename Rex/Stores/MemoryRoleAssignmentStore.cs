using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rex.Models;

namespace Rex.Stores
{
    public class MemoryRoleAssignmentStore : IRoleAssignmentStore
    {
        private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

        private Dictionary<Guid, Dictionary<Guid, Models.RoleAssignment>> _state = new Dictionary<Guid, Dictionary<Guid, Models.RoleAssignment>>();

        public async Task<RoleAssignment> GetRoleAssignment(Guid collectionId, Guid userId)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.GetValueOrDefault(collectionId)?.GetValueOrDefault(userId);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async IAsyncEnumerable<RoleAssignment> GetRoleAssignments(Guid collectionId)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                foreach (var assignment in this._state.GetValueOrDefault(collectionId)?.Values?.ToArray() ?? Array.Empty<RoleAssignment>())
                    yield return assignment;
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<bool> RemoveRoleAssignmentAsync(Guid collectionId, Guid userId)
        {
            try
            {
                this.lockSlim.EnterReadLock();

                return this._state.GetValueOrDefault(collectionId)?.Remove(userId) ?? false;
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<RoleAssignment> StoreRoleAssignmentAsync(RoleAssignment assignment)
        {
            try
            {
                this.lockSlim.EnterWriteLock();
                this._state[assignment.CollectionId] = this._state.GetValueOrDefault(assignment.CollectionId) ?? new Dictionary<Guid, Models.RoleAssignment>();
                this._state[assignment.CollectionId][assignment.PrincipalId] = assignment;
                return assignment;
            }
            finally
            {
                this.lockSlim.ExitWriteLock();
            }
        }
    }
}