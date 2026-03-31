using System.Linq.Expressions;
using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Hangfire;

namespace Eirene.BLL.Services.Implementation.Background_Jobs;

public class BackgroundJobServices:IBackgroundJobService
{
    public void Enqueue(Expression<Func<Task>> job)
    {
        BackgroundJob.Enqueue(job);
    }
}