using memory.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace memory.Services
{
    public class MemoryRepository : IMemoryRepository
    {
        private readonly ApplicationDbContext _context;

        public MemoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MemoryItem>> GetAllAsync()
        {
            return await _context.Memories.ToListAsync();
        }

        public async Task<IEnumerable<MemoryItem>> GetByPersonIdAsync(Guid personId)
        {
            return await _context.Memories
                .Where(m => m.PersonId == personId)
                .ToListAsync();
        }

        public async Task<MemoryItem> GetByIdAsync(Guid id)
        {
            return await _context.Memories.FindAsync(id);
        }

        public async Task AddAsync(MemoryItem memory)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));
            await _context.Memories.AddAsync(memory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MemoryItem memory)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));
            _context.Memories.Update(memory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var memoryItem = await _context.Memories.FindAsync(id); // 変数名をmemoryからmemoryItemに変更
            if (memoryItem != null)
            {
                _context.Memories.Remove(memoryItem);
                await _context.SaveChangesAsync();
            }
        }
    }
} 