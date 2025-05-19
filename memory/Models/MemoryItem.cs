using System;

namespace memory.Models
{
    public class MemoryItem // クラス名を Memory から MemoryItem に変更
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImagePath { get; set; }
    }
} 