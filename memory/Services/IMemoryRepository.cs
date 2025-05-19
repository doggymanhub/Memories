using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using memory.Models; // MemoryItem を使用

namespace memory.Services
{
    public interface IMemoryRepository
    {
        Task<IEnumerable<MemoryItem>> GetAllAsync();
        Task<IEnumerable<MemoryItem>> GetByPersonIdAsync(Guid personId);
        Task<MemoryItem> GetByIdAsync(Guid id);
        Task AddAsync(MemoryItem memory);
        Task UpdateAsync(MemoryItem memory);
        Task DeleteAsync(Guid id);
    }
} 