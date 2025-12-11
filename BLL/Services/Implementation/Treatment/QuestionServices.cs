
using AutoMapper;
using BLL.ModelVMs.Treatment;
using BLL.Services.Abstraction.Treatment;
using DAL.Entities.Treatment;
using DAL.Repository.Abstraction.Treatment;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Treatment
{
    public class QuestionServices : IQuestionServices
    {
        private readonly ILogger<QuestionServices> _logger;
        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        public QuestionServices(ILogger<QuestionServices> logger, IMapper mapper, IQuestionRepository questionRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _questionRepository = questionRepository;
        }
        public async Task<(bool IsSuccess, QuestionDTO? AddedQuestion)> CreateAsync(AddQuestion model)
        {
            try
            {
                var questionEntity = _mapper.Map<Question>(model);
                var addedQuestion = await _questionRepository.AddAsync(questionEntity);
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

        public async Task<bool> DeleteAsync(int id)
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
                var getAllQuestions = await _questionRepository.GetAllAsync();
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

        public async Task<(bool IsSuccess, QuestionDTO? question)> GetByIdAsync(int id)
        {
            try
            {
                var questionEntity = await _questionRepository.GetByIdAsync(id);
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
                var existingQuestion = await _questionRepository.GetByIdAsync(model.Id);
                if (existingQuestion == null)
                {
                    _logger.LogError("Question not found");
                    return (false, null);
                }

                _mapper.Map(model, existingQuestion);

                var result = await _questionRepository.UpdateAsync(existingQuestion);
                if (!result)
                {
                    _logger.LogError("Failed to update question");
                    return (false, null);
                }

                var updatedDto = _mapper.Map<EditQuestion>(existingQuestion);
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
