using Eirene.BLL.Models.Model_Result;
using Eirene.BLL.Models.Treatment.Task;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Abstraction.Treatment;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Microsoft.Extensions.Logging;

public class PatientTaskServices : IPatientTaskServices
{
    private readonly ILogger<PatientTaskServices> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPatientTaskRepository _taskRepository;
    private readonly ITreatmentPlanRepository _treatmentPlanRepository;
    private readonly IUserContext _userContext;
    public PatientTaskServices(
        ILogger<PatientTaskServices> logger,
        IUnitOfWork unitOfWork,
        IPatientTaskRepository taskRepository,
        ITreatmentPlanRepository treatmentPlanRepository,
        IUserContext userContext)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _taskRepository = taskRepository;
        _treatmentPlanRepository = treatmentPlanRepository;
        _userContext = userContext;
    }

    public async Task<bool> AddTasksFromModelAsync(AITaskResponse modelResult, string userId)
    {
        try
        {
            var taskTexts = modelResult.TasksForUser?
                .Select(t => t.Task)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            if (taskTexts == null || taskTexts.Count == 0)
            {
                _logger.LogError("No valid tasks found in model result for user {UserId}", userId);
                return false;
            }

            var treatmentPlan = new Eirene.DAL.Entities.Treatment.TreatmentPlan { UserId = userId };
            await _treatmentPlanRepository.AddAsync(treatmentPlan);
            await _unitOfWork.SaveChangesAsync();

            foreach (var taskText in taskTexts)
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
                taskTexts.Count, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tasks for user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<PatientTaskResponseDTO>> GetTasksForUserAsync(string userId)
    {
        var tasks = await _taskRepository.FindAsync(t => t.PatientId == userId);
        return tasks.Select(t => new PatientTaskResponseDTO
        {
            Id = t.Id,
            Description = t.Description,
            IsCompleted = t.IsCompleted,
            CreatedAt = t.CreatedAt,
            PatientId = t.PatientId,
        }).OrderByDescending(t => t.CreatedAt);
    }

    public async Task<PatientTaskResponseDTO?> GetTaskByIdAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) return null;

        return new PatientTaskResponseDTO
        {
            Id = task.Id,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            PatientId = task.PatientId
        };
    }

    public async Task<bool> UpdateTaskStatusAsync(Guid taskId, bool isCompleted)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) return false;

        var userId = _userContext.UserId;

        if (userId != task.PatientId) return false;
        task.IsCompleted = isCompleted;
        var updated = await _taskRepository.UpdateAsync(task);
        if (updated)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        return updated;
    }
}