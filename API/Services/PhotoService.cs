using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Interfaces
{
    public class PhotoService: IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        public PhotoService(IOptions<CloudinarySettings> config)
        {
            Account acc = new Account {
                Cloud = config.Value.CloudName,
                ApiKey = config.Value.ApiKey,
                ApiSecret = config.Value.ApiSecret
            };
            _cloudinary = new Cloudinary(acc);
        }
        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file) {
           var uploadResult = new ImageUploadResult();
           if(file.Length > 0)
           {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face"),
                Folder = "da-net"
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
           }
           return uploadResult;
        }
        public Task<DeletionResult> DeletePhotoAsync(string publicId) {
            DeletionParams deleteParams = new DeletionParams(publicId);
            return _cloudinary.DestroyAsync(deleteParams);
        }
    }
}