using BLL.Services.Abstraction.Communication;
using DAL.Entities.Communication;
using DAL.Repository.Abstraction.Communication;

namespace BLL.Services.Implementation.Communication
{
    public class ChatServices : IChatServices
    {
        private readonly IChatRepository _chatRepository;

        public ChatServices(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<Conversation> CreateConversationAsync(string doctorId, string patientId)
        {
            if (string.IsNullOrWhiteSpace(doctorId))
                throw new ArgumentException("DoctorId is required.", nameof(doctorId));

            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("PatientId is required.", nameof(patientId));

            if (doctorId == patientId)
                throw new InvalidOperationException("Doctor and patient cannot be the same user.");

            return await _chatRepository.CreateConversationAsync(doctorId, patientId);
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId)
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

            // - Check if sender belongs to conversation
            // - Check if conversation is closed
            // - Check if sender is blocked

            return await _chatRepository.SaveMessageAsync(conversationId, senderId, message);
        }
    }
}
