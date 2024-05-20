using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Core.Services
{
    public class FilesService : IFilesService
    {
        private const string imageFolder = "images";
        int[] sizes = { 320, 600, 1200 };

        public async Task DeleteImage(string name)
        {
            await Task.Run(() =>
            {
                try
                {
                    string root = Directory.GetCurrentDirectory();

                    foreach (int size in sizes)
                    {
                        string fullFileName = $"{size}_{name}";
                        string imagePath = Path.Combine(root, imageFolder, fullFileName);
                        if (File.Exists(imagePath))
                        {
                            File.Delete(imagePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при видалені файлу {ex.Message}");
                }
            });
        }

        public async Task<string> SaveImage(IFormFile file)
        {
            try
            {
                string root = Directory.GetCurrentDirectory();
                string newNameFile = Guid.NewGuid().ToString();
                string fileName = $"{newNameFile}.webp";

                foreach (int size in sizes)
                {
                    string fullFileName = $"{size}_{newNameFile}.webp";
                    string imagePath = Path.Combine(root, imageFolder, fullFileName);
                    {
                        using (var image = Image.Load(file.OpenReadStream()))
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Size = new Size(size, size),
                                Mode = ResizeMode.Max
                            }));
                            await image.SaveAsync(imagePath, new WebpEncoder());
                        }
                    }
                }
                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при збереженні файлу {ex.Message}");
                return ex.Message;
            }
        }
    }
}
