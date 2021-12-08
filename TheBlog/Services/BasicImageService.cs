using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace TheBlog.Services
{
    public class BasicImageService : IImageService
    {
        public async Task<byte[]> EncodeImageAsync(IFormFile file)
        {
            if (file is null) return null;

            //ms => memory stream
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> EncodeImageAsync(string fileName)
        {
            var file = $"{Directory.GetCurrentDirectory()}/wwwroot/assets/images/{fileName}";
            return await File.ReadAllBytesAsync(file);
        }

        public string DecodeImage(byte[] data, string type) // out of the database
        {
            if (data is null || type is null) return null;
            return $"data:image/{type};base64,{Convert.ToBase64String(data)}";
        }

        public string ContentType(IFormFile file)
        {
            return file?.ContentType;
        }

        public int Size(IFormFile file)
        {
            return Convert.ToInt32(file?.Length);
        }
    }
}
