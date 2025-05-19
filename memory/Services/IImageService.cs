using System.Threading.Tasks;

namespace memory.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(string sourceFilePath, string uniqueIdentifier);
        Task DeleteImageAsync(string imagePath);
    }
}