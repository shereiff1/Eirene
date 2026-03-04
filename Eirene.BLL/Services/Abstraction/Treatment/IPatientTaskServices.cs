

using Eirene.BLL.Models.Treatment.Task;
using Eirene.BLL.ModelVMs.Treatment;

namespace Eirene.BLL.Services.Abstraction.Treatment;

public interface IPatientTaskServices
{

    Task<bool> AddTasksFromModelAsync(string modelResult, string userId);

}
