using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using memory.Models;

namespace memory.Services
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAllAsync();
        Task<Group> GetByIdAsync(Guid id);
        Task AddAsync(Group group);
        Task UpdateAsync(Group group);
        Task DeleteAsync(Guid id);
    }
} 