namespace Rentals.Application.Abstractions;

public interface IImageConverter
{
    byte[] ConvertToBmp(byte[] originalBytes);
}
