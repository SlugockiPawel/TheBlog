using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TheBlog.Services
{
    public class BasicImageService : IImageService
    {
        public Task<byte[]> EncodeImageAsync(IFormFile file)
        {
            throw new System.NotImplementedException();
        }

        public Task<byte[]> EncodeImageAsync(string fileName)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }
    }
}
