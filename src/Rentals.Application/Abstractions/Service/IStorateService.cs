namespace Rentals.Application.Abstractions;

public interface IStorageService
{
    Task UploadAsync(string fileName, byte[] content);
}