using Rentals.Application.Abstractions;

namespace Rentals.Infrastructure.Images;

using System.Drawing;
using System.Drawing.Imaging;

public class ImageConverter : IImageConverter
{
    public byte[] ConvertToBmp(byte[] originalBytes)
    {
        using var inputStream = new MemoryStream(originalBytes);
        using var image = Image.FromStream(inputStream);

        using var outputStream = new MemoryStream();
        image.Save(outputStream, ImageFormat.Bmp);

        return outputStream.ToArray();
    }
}
