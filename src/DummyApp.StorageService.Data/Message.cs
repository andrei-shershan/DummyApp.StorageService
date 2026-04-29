namespace DummyApp.StorageService.Data;

public sealed class Message
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public int MessageTypeId { get; set; }
    public MessageType MessageType { get; set; } = null!;
}
