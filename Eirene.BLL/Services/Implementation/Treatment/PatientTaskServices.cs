using Eirene.BLL.AIModel;
using Eirene.BLL.Services.Abstraction.Treatment;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class PatientTaskServices : IPatientTaskServices
{
    private readonly ILogger<PatientTaskServices> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPatientTaskRepository _taskRepository;
    private readonly ITreatmentPlanRepository _treatmentPlanRepository;

    public PatientTaskServices(
        ILogger<PatientTaskServices> logger,
        IUnitOfWork unitOfWork,
        IPatientTaskRepository taskRepository,
        ITreatmentPlanRepository treatmentPlanRepository)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _taskRepository = taskRepository;
        _treatmentPlanRepository = treatmentPlanRepository;
    }

    public async Task<bool> AddTasksFromModelAsync(string modelResult, string userId)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            AnalysisDTO? analysisDto = null;

            try
            {
                analysisDto = JsonSerializer.Deserialize<AnalysisDTO>(modelResult, options);
                if (analysisDto?.Tasks_For_User != null && analysisDto.Tasks_For_User.Any())
                {
                    _logger.LogInformation("Parsed as direct AnalysisDTO");
                }
                else
                {
                    analysisDto = null;
                }
            }
            catch (JsonException)
            {
                // Ignore and try next method
            }


            if (analysisDto == null)
            {
                var modelDto = JsonSerializer.Deserialize<ModelResultDTO>(modelResult, options);

                if (modelDto != null && !string.IsNullOrWhiteSpace(modelDto.Analysis))
                {
                    analysisDto = JsonSerializer.Deserialize<AnalysisDTO>(modelDto.Analysis, options);
                    _logger.LogInformation("Parsed as wrapped ModelResultDTO");
                }
            }


            if (analysisDto?.Tasks_For_User == null || !analysisDto.Tasks_For_User.Any())
            {
                _logger.LogError("No valid tasks found in model result for user {UserId}", userId);
                return false;
            }

            var treatmentPlan = new Eirene.DAL.Entities.Treatment.TreatmentPlan { UserId = userId };
            await _treatmentPlanRepository.AddAsync(treatmentPlan);
            await _unitOfWork.SaveChangesAsync();

            foreach (var taskText in analysisDto.Tasks_For_User.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                await _taskRepository.AddAsync(new Eirene.DAL.Entities.Treatment.PatientTask
                {
                    Description = taskText,
                    PatientId = userId,
                    TreatmentPlanId = treatmentPlan.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false
                });
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Successfully added {Count} tasks for user {UserId}",
                analysisDto.Tasks_For_User.Count, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tasks for user {UserId}", userId);
            return false;
        }
    }
}