using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rex.Models;

namespace Rex.Stores
{
    [SuppressMessage("Await.Warning", "CS1998", Justification = "This in-memory implementation doesn't await anything.")]
    public sealed class MemoryRoleAssignmentStore : IRoleAssignmentStore, IDisposable
    {
        private SemaphoreSlim lockSlim = new SemaphoreSlim(1);

        private Dictionary<Guid, Dictionary<Guid, Models.RoleAssignment>> _state = new Dictionary<Guid, Dictionary<Guid, Models.RoleAssignment>>();

        public async Task<RoleAssignment?> GetRoleAssignment(Guid collectionId, Guid userId)
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                return this._state.GetValueOrDefault(collectionId)?.GetValueOrDefault(userId);
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public async IAsyncEnumerable<RoleAssignment> GetRoleAssignments(Guid collectionId)
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                foreach (var assignment in this._state.GetValueOrDefault(collectionId)?.Values?.ToArray() ?? Array.Empty<RoleAssignment>())
                    yield return assignment;
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public async Task<bool> RemoveRoleAssignmentAsync(Guid collectionId, Guid userId)
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);

                return this._state.GetValueOrDefault(collectionId)?.Remove(userId) ?? false;
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public async Task<RoleAssignment> StoreRoleAssignmentAsync(RoleAssignment assignment)
        {
            if (assignment is null)
            {
                throw new ArgumentNullException(nameof(assignment));
            }

            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                this._state[assignment.CollectionId] = this._state.GetValueOrDefault(assignment.CollectionId) ?? new Dictionary<Guid, Models.RoleAssignment>();
                this._state[assignment.CollectionId][assignment.PrincipalId] = assignment;
                return assignment;
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public void Dispose()
        {
            this.lockSlim.Dispose();
        }
    }
}