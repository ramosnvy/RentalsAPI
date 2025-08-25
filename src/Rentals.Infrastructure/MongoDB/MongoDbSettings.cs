namespace Rentals.Infrastructure.MongoDB;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string MotorcyclesCollectionName { get; set; } = "motorcycles";
    public string NotificationsCollectionName { get; set; } = "motorcycle_notifications";
}
