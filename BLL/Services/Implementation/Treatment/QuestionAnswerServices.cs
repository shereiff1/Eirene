using BLL.Services.Abstraction.Treatment;
using DAL.Entities.Treatment;
using DAL.Repository.Abstraction.Treatment;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Treatment
{
    public class QuestionAnswerServices : IQuestionAnswerServices
    {
        private readonly ILogger<QuestionAnswerServices> _logger;
        private readonly IQuestionAnswerRepository _questionAnswerRepository;

        public QuestionAnswerServices(ILogger<QuestionAnswerServices> logger, IQuestionAnswerRepository questionAnswerRepository)
        {
            _logger = logger;
            _questionAnswerRepository = questionAnswerRepository;
        }

        public async Task<(bool IsSuccess, IEnumerable<QuestionAnswer> Answers)> GetAnswersForUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogError("UserId cannot be null or empty");
                    return (false, Enumerable.Empty<QuestionAnswer>());
                }

                var answers = await _questionAnswerRepository.GetAnswersByUserIdAsync(userId);

                if (answers == null || !answers.Any())
                {
                    _logger.LogWarning("No answers found for user {UserId}", userId);
                    return (false, Enumerable.Empty<QuestionAnswer>());
                }

                return (true, answers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving answers for user {UserId}", userId);
                return (false, Enumerable.Empty<QuestionAnswer>());
            }
        }

        public async Task<(bool IsSuccess, QuestionAnswer Answer)> AddAnswerAsync(string userId, int questionId, string answer)
        {
            try
            {
                var questionAnswer = new QuestionAnswer
                {
                    UserId = userId,
                    QuestionId = questionId,
                    Answer = answer
                };

                var addedAnswer = await _questionAnswerRepository.AddAsync(questionAnswer);

                if (addedAnswer == null)
                {
                    _logger.LogError("Failed to add answer for user {UserId}", userId);
                    return (false, null!);
                }

                return (true, addedAnswer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding answer for user {UserId}", userId);
                return (false, null!);
            }
        }

        public async Task<(bool IsSuccess, IEnumerable<QuestionAnswer> Answers)> AddMultipleAnswersAsync(string userId, List<(int QuestionId, string Answer)> answers)
        {
            try
            {
                var questionAnswers = answers.Select(a => new QuestionAnswer
                {
                    UserId = userId,
                    QuestionId = a.QuestionId,
                    Answer = a.Answer
                }).ToList();

                var addedAnswers = new List<QuestionAnswer>();

                foreach (var qa in questionAnswers)
                {
                    var added = await _questionAnswerRepository.AddAsync(qa);
                    if (added != null)
                    {
                        addedAnswers.Add(added);
                    }
                }

                return (addedAnswers.Count == questionAnswers.Count, addedAnswers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding multiple answers for user {UserId}", userId);
                return (false, Enumerable.Empty<QuestionAnswer>());
            }
        }
    }
}