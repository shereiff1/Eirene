

using BLL.Models.Treatment.Task;
using BLL.ModelVMs.Treatment;

namespace BLL.Services.Abstraction.Treatment;

public interface IPatientTaskServices
{

    Task<bool> AddTasksFromModelAsync(string modelResult, string userId);

}
