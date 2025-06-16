namespace Dotnet.Endpoints.Conversations.Requests;

public class AddMessageRequest
{
    public required string Role { get; set; }
    public required string Content { get; set; }
    public required DateTime CreatedAt { get; set; }
}
