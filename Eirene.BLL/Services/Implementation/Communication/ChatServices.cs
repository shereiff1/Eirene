using Eirene.BLL.Services.Abstraction.Communication;
using Eirene.DAL.Entities.Communication;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Communication;

namespace Eirene.BLL.Services.Implementation.Communication
{
    public class ChatServices : IChatServices
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChatServices(IChatRepository chatRepository, IUnitOfWork unitOfWork)
        {
            _chatRepository = chatRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Conversation> CreateConversationAsync(string doctorId, string patientId)
        {
            if (string.IsNullOrWhiteSpace(doctorId))
                throw new ArgumentException("DoctorId is required.", nameof(doctorId));

            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("PatientId is required.", nameof(patientId));

            if (doctorId == patientId)
                throw new InvalidOperationException("Doctor and patient cannot be the same user.");

            var conversation = await _chatRepository.CreateConversationAsync(doctorId, patientId);
            await _unitOfWork.SaveChangesAsync();
            return conversation;
        }

        public async Task<Conversation?> GetConversationAsync(Guid conversationId)
        {
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));

            return await _chatRepository.GetConversationAsync(conversationId);
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(Guid conversationId)
        {
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));

            return await _chatRepository.GetMessagesAsync(conversationId);
        }

        public async Task<ChatMessage> SaveMessageAsync(Guid conversationId, string senderId, string message)
        {
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));

            if (string.IsNullOrWhiteSpace(senderId))
                throw new ArgumentException("SenderId is required.", nameof(senderId));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty.", nameof(message));

            // Validate sender belongs to the conversation
            var conversation = await _chatRepository.GetConversationAsync(conversationId);
            if (conversation == null)
                throw new InvalidOperationException("Conversation not found.");

            if (conversation.DoctorId != senderId && conversation.PatientId != senderId)
                throw new UnauthorizedAccessException("You are not a participant in this conversation.");

            var chatMessage = await _chatRepository.SaveMessageAsync(conversationId, senderId, message);
            await _unitOfWork.SaveChangesAsync();
            return chatMessage;
        }
    }
}
