using System;
using System.Collections.Generic;
using memory.Services; // SnsLink, PersonCategory を参照するために追加

namespace memory.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public bool IsFavorite { get; set; }
        
        public ICollection<SnsLink> SnsLinks { get; set; } = new List<SnsLink>(); 
        
        public ICollection<PersonCategory> PersonCategories { get; set; } = new List<PersonCategory>();
    }
} 