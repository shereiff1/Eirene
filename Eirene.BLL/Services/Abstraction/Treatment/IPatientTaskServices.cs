using Eirene.BLL.Models.Model_Result;
using Eirene.BLL.Models.Treatment.Task;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eirene.BLL.Services.Abstraction.Treatment;

public interface IPatientTaskServices
{
    Task<bool> AddTasksFromModelAsync(AITaskResponse modelResult, string userId);
    Task<IEnumerable<PatientTaskResponseDTO>> GetTasksForUserAsync(string userId);
    Task<PatientTaskResponseDTO?> GetTaskByIdAsync(Guid taskId);
    Task<bool> UpdateTaskStatusAsync(Guid taskId, bool isCompleted);
}
