using DAL.Database;
using DAL.Entities.Communication;
using DAL.Repository.Abstraction.Communication;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Communication
{
    public class ChatRepository : IChatRepository
    {
        private readonly EireneDBContext _dbContext;

        public ChatRepository(EireneDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Conversation> CreateConversationAsync(string doctorId, string patientId)
        {
            var existingConversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c =>
                    c.DoctorId == doctorId &&
                    c.PatientId == patientId);

            if (existingConversation != null)
                return existingConversation;

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                PatientId = patientId,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Conversations.AddAsync(conversation);
            await _dbContext.SaveChangesAsync();

            return conversation;
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId)
        {
            return await _dbContext.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ChatMessage> SaveMessageAsync(Guid conversationId, string senderId, string message)
        {
            var conversationExists = await _dbContext.Conversations
                .AnyAsync(c => c.Id == conversationId);

            if (!conversationExists)
                throw new InvalidOperationException("Conversation not found.");

            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = senderId,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            await _dbContext.ChatMessages.AddAsync(chatMessage);
            await _dbContext.SaveChangesAsync();

            return chatMessage;
        }
    }
}
