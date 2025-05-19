using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using memory.Models;

namespace memory.Services
{
    public interface IPersonRepository
    {
        Task<IEnumerable<Person>> GetAllAsync();
        Task<IEnumerable<Person>> GetByGroupIdAsync(Guid groupId);
        Task<Person> GetByIdAsync(Guid id);
        Task AddAsync(Person person);
        Task UpdateAsync(Person person);
        Task DeleteAsync(Guid id);
    }
} 