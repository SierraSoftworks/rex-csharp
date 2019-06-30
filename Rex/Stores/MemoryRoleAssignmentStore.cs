using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rex.Models;

namespace Rex.Stores
{
    public class MemoryRoleAssignmentStore : IRoleAssignmentStore
    {
        private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

        private Dictionary<Guid, Dictionary<Guid, Models.RoleAssignment>> _state = new Dictionary<Guid, Dictionary<Guid, Models.RoleAssignment>>();

        public async Task<RoleAssignment> GetRoleAssignment(Guid userId, Guid collectionId)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.GetValueOrDefault(userId)?.GetValueOrDefault(collectionId);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async IAsyncEnumerable<RoleAssignment> GetRoleAssignments(Guid userId)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                foreach (var assignment in this._state.GetValueOrDefault(userId)?.Values)
                    yield return assignment;
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<bool> RemoveRoleAssignmentAsync(Guid userId, Guid collectionId)
        {
            try
            {
                this.lockSlim.EnterReadLock();

                return this._state.GetValueOrDefault(userId)?.Remove(collectionId) ?? false;
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
                this._state[assignment.PrincipalId] = this._state.GetValueOrDefault(assignment.PrincipalId) ?? new Dictionary<Guid, Models.RoleAssignment>();
                this._state[assignment.PrincipalId][assignment.CollectionId] = assignment;
                return assignment;
            }
            finally
            {
                this.lockSlim.ExitWriteLock();
            }
        }
    }
}