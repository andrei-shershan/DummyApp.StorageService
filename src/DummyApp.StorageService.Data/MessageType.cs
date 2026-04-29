namespace DummyApp.StorageService.Data;

public sealed class MessageType
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
