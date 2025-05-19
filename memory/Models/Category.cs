using System;
using System.Collections.Generic;
using memory.Services;

namespace memory.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public ICollection<PersonCategory> PersonCategories { get; set; } = new List<PersonCategory>();
    }
} 