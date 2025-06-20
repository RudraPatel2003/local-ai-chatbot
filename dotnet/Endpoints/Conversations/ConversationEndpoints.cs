using Carter;
using Dotnet.Database.Context;
using Dotnet.Database.Models;
using Dotnet.Endpoints.Conversations.Requests;
using Dotnet.Services.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.Endpoints.Conversations;

public class ConversationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder conversationsGroup = app.MapGroup("/conversations");

        _ = conversationsGroup.MapGet("/", GetAllConversations);
        _ = conversationsGroup.MapGet("/{id:guid}", GetConversationById);
        _ = conversationsGroup.MapPost("/", CreateConversation);
        _ = conversationsGroup.MapPost("/{id:guid}/messages", AddMessage);
        _ = conversationsGroup.MapPut("/{id:guid}", UpdateConversation);
        _ = conversationsGroup.MapDelete("/{id:guid}", DeleteConversation);
    }

    private static async Task<Ok<List<Conversation>>> GetAllConversations(
        ChatbotContext dbContext,
        ILoggingService loggingService
    )
    {
        await loggingService.Log("GET /api/dotnet/conversations", null);

        List<Conversation> conversations = await dbContext
            .Conversations.Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return TypedResults.Ok(conversations);
    }

    private static async Task<Results<Ok<Conversation>, NotFound>> GetConversationById(
        Guid id,
        ChatbotContext dbContext,
        ILoggingService loggingService
    )
    {
        await loggingService.Log($"GET /api/dotnet/conversations/{id}", null);

        Conversation? conversation = await dbContext
            .Conversations.Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id);

        return conversation is null ? TypedResults.NotFound() : TypedResults.Ok(conversation);
    }

    private static async Task<Created<Conversation>> CreateConversation(
        CreateConversationRequest request,
        ChatbotContext dbContext,
        ILoggingService loggingService
    )
    {
        await loggingService.Log("POST /api/dotnet/conversations", request);

        Conversation conversation = new() { Name = request.Name, Messages = [] };

        _ = dbContext.Conversations.Add(conversation);
        _ = await dbContext.SaveChangesAsync();

        return TypedResults.Created($"/conversations/{conversation.Id}", conversation);
    }

    private static async Task<Results<Ok<Message>, NotFound>> AddMessage(
        Guid id,
        AddMessageRequest request,
        ChatbotContext dbContext,
        ILoggingService loggingService
    )
    {
        await loggingService.Log($"POST /api/dotnet/conversations/{id}/messages", request);

        Conversation? conversation = await dbContext
            .Conversations.Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conversation is null)
        {
            return TypedResults.NotFound();
        }

        Message newMessage = new()
        {
            Role = request.Role,
            Content = request.Content,
            CreatedAt = request.CreatedAt,
        };

        conversation.Messages.Add(newMessage);

        _ = await dbContext.SaveChangesAsync();

        return TypedResults.Ok(newMessage);
    }

    private static async Task<Results<Ok<Conversation>, NotFound>> UpdateConversation(
        Guid id,
        UpdateConversationRequest request,
        ChatbotContext dbContext,
        ILoggingService loggingService
    )
    {
        await loggingService.Log($"PUT /api/dotnet/conversations/{id}", request);

        Conversation? conversation = await dbContext.Conversations.FindAsync(id);

        if (conversation is null)
        {
            return TypedResults.NotFound();
        }

        conversation.Name = request.Name;

        _ = await dbContext.SaveChangesAsync();

        return TypedResults.Ok(conversation);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteConversation(
        Guid id,
        ChatbotContext dbContext,
        ILoggingService loggingService
    )
    {
        await loggingService.Log($"DELETE /api/dotnet/conversations/{id}", null);

        Conversation? conversation = await dbContext
            .Conversations.Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conversation is null)
        {
            return TypedResults.NotFound();
        }

        _ = dbContext.Conversations.Remove(conversation);
        _ = await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
