using Rentals.Domain.Abstractions;

namespace Rentals.Domain.Drivers;

public sealed class CnhImage : ValueObject
{
    public string BlobPath { get; }
  
    /// <summary>
    /// image/png  image/bmp
    /// </summary>
    public string ContentType { get; }

    private CnhImage(string blobPath, string contentType)
    {
        BlobPath = blobPath;
        ContentType = contentType;
    }
    
    public static CnhImage Create(string blobPath, string contentType)
    {
        if (string.IsNullOrWhiteSpace(blobPath))
        {
            throw new ArgumentNullException("É obrigatório informar o caminho do arquivo.", nameof(blobPath));
        }

        if (contentType is not ("image/png" or "image/bmp"))
        {
            throw new ArgumentException("Formato inválido. Use PNG ou BMP.", nameof(contentType));
        }
        return new CnhImage(blobPath, contentType);
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ContentType;
        yield return BlobPath; 
    }
}