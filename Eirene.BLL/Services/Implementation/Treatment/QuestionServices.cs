
using AutoMapper;
using Eirene.BLL.ModelVMs.Treatment;
using Eirene.BLL.Services.Abstraction.Treatment;
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Eirene.DAL.Repository.Abstraction;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Treatment
{
    public class QuestionServices : IQuestionServices
    {
        private readonly ILogger<QuestionServices> _logger;
        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuestionChoiceRepository _questionChoiceRepository;
        private readonly IUnitOfWork _unitOfWork;
        public QuestionServices(ILogger<QuestionServices> logger, IMapper mapper,
            IQuestionRepository questionRepository, IQuestionChoiceRepository questionChoiceRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _mapper = mapper;
            _questionRepository = questionRepository;
            _questionChoiceRepository = questionChoiceRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<(bool IsSuccess, QuestionDTO? AddedQuestion)> CreateAsync(AddQuestion model)
        {
            try
            {
                var questionEntity = _mapper.Map<Question>(model);

                // Map and attach choices
                foreach (var choiceItem in model.Choices)
                {
                    var choice = _mapper.Map<QuestionChoice>(choiceItem);
                    questionEntity.Choices.Add(choice);
                }

                var addedQuestion = await _questionRepository.AddAsync(questionEntity);
                await _unitOfWork.SaveChangesAsync();
                if (addedQuestion == null)
                {
                    _logger.LogError("Failed to add question");
                    return (false, null);
                }
                var questionDTO = _mapper.Map<QuestionDTO>(addedQuestion);
                return (true, questionDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new question.");
                return (false, null);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var questionEntity = await _questionRepository.GetByIdAsync(id);
                if (questionEntity == null)
                {
                    _logger.LogError("Question not found");
                    return false;
                }
                await _questionRepository.DeleteAsync(questionEntity);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting a question.");
                return false;
            }
        }
        public async Task<(bool IsSuccess, List<QuestionDTO>? questions)> GetAllAsync()
        {
            try
            {
                var getAllQuestions = await _questionRepository.GetAllWithChoicesAsync();
                if (getAllQuestions == null || !getAllQuestions.Any())
                {
                    _logger.LogError("No questions found");
                    return (false, null);
                }
                var questionDTOs = _mapper.Map<List<QuestionDTO>>(getAllQuestions);
                return (true, questionDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all questions.");
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, QuestionDTO? question)> GetByIdAsync(Guid id)
        {
            try
            {
                var questionEntity = await _questionRepository.GetByIdWithChoicesAsync(id);
                if (questionEntity == null)
                {
                    _logger.LogError("Question not found");
                    return (false, null);
                }
                var questionDTO = _mapper.Map<QuestionDTO>(questionEntity);
                return (true, questionDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving a question by ID.");
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, EditQuestion? editedQuestion)> UpdateAsync(EditQuestion model)
        {
            try
            {
                var existingQuestion = await _questionRepository.GetByIdWithChoicesAsync(model.Id);
                if (existingQuestion == null)
                {
                    _logger.LogError("Question not found");
                    return (false, null);
                }

                // Update question content
                existingQuestion.QuestionContent = model.QuestionContent;

                // Reconcile choices: add new, update existing, delete removed
                var incomingChoiceIds = model.Choices
                    .Where(c => c.Id.HasValue)
                    .Select(c => c.Id!.Value)
                    .ToHashSet();

                // Delete choices not present in the request
                var choicesToDelete = existingQuestion.Choices
                    .Where(c => !incomingChoiceIds.Contains(c.Id))
                    .ToList();

                foreach (var choice in choicesToDelete)
                {
                    await _questionChoiceRepository.DeleteAsync(choice);
                }

                // Update existing and add new choices
                foreach (var choiceItem in model.Choices)
                {
                    if (choiceItem.Id.HasValue)
                    {
                        // Update existing choice
                        var existingChoice = existingQuestion.Choices
                            .FirstOrDefault(c => c.Id == choiceItem.Id.Value);
                        if (existingChoice != null)
                        {
                            existingChoice.ChoiceText = choiceItem.ChoiceText;
                        }
                    }
                    else
                    {
                        // Add new choice
                        var newChoice = new QuestionChoice
                        {
                            ChoiceText = choiceItem.ChoiceText,
                            QuestionId = existingQuestion.Id
                        };
                        existingQuestion.Choices.Add(newChoice);
                    }
                }

                var result = await _questionRepository.UpdateAsync(existingQuestion);
                await _unitOfWork.SaveChangesAsync();
                if (!result)
                {
                    _logger.LogError("Failed to update question");
                    return (false, null);
                }

                // Map back to EditQuestion for response
                var updatedDto = new EditQuestion
                {
                    Id = existingQuestion.Id,
                    QuestionContent = existingQuestion.QuestionContent,
                    Choices = existingQuestion.Choices.Select(c => new EditQuestionChoiceItem
                    {
                        Id = c.Id,
                        ChoiceText = c.ChoiceText
                    }).ToList()
                };
                return (true, updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating a question.");
                return (false, null);
            }
        }


    }
}
