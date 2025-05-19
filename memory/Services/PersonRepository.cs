using memory.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace memory.Services
{
    public class PersonRepository : IPersonRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.Persons
                .Include(p => p.SnsLinks) // SnsLinkをEager Load
                .Include(p => p.PersonCategories) // PersonCategoryをEager Load
                    .ThenInclude(pc => pc.Category) // PersonCategoryからさらにCategoryをEager Load
                .ToListAsync();
        }

        public async Task<IEnumerable<Person>> GetByGroupIdAsync(Guid groupId)
        {
            return await _context.Persons
                .Where(p => p.GroupId == groupId)
                .Include(p => p.SnsLinks)
                .Include(p => p.PersonCategories)
                    .ThenInclude(pc => pc.Category)
                .ToListAsync();
        }

        public async Task<Person> GetByIdAsync(Guid id)
        {
            return await _context.Persons
                .Include(p => p.SnsLinks)
                .Include(p => p.PersonCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id); // SingleOrDefaultAsync も検討
        }

        public async Task AddAsync(Person person)
        {
            if (person == null) throw new ArgumentNullException(nameof(person));
            await _context.Persons.AddAsync(person);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Person person)
        {
            if (person == null) throw new ArgumentNullException(nameof(person));
            _context.Persons.Update(person);
            // 注意: Updateメソッドは関連エンティティの変更を自動的に追跡しないことがある。
            // SnsLinksやPersonCategoriesコレクションの変更は別途処理が必要な場合がある。
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person != null)
            {
                // 関連エンティティの削除はカスケード設定に依存する。
                // OnModelCreatingで適切に設定されていない場合、手動で削除するか、エラーになる可能性がある。
                // SnsLinksやPersonCategoriesはPersonIdにFKを持つため、Person削除時に自動で削除されるように設定可能。
                _context.Persons.Remove(person);
                await _context.SaveChangesAsync();
            }
        }
    }
} 