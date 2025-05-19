using System;
using System.IO;
using System.Threading.Tasks;

namespace memory.Services
{
    public class ImageService : IImageService
    {
        private readonly string _basePath;

        public ImageService()
        {
            _basePath = Path.Combine(AppContext.BaseDirectory, "Images");
            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveImageAsync(string sourceFilePath, string uniqueIdentifier)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                throw new ArgumentException("画像ファイルパスが無効です。", nameof(sourceFilePath));
            }

            string extension = Path.GetExtension(sourceFilePath);
            string destinationFileName = $"{uniqueIdentifier}{extension}"; 
            string destinationPath = Path.Combine(_basePath, destinationFileName);

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
            
            await Task.Run(() => File.Copy(sourceFilePath, destinationPath));

            return destinationPath;
        }

        public async Task DeleteImageAsync(string imagePath)
        {
            await Task.Run(() =>
            {
                if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            });
        }
    }
} 