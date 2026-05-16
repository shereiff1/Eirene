using System.Linq.Expressions;

namespace Eirene.BLL.Services.Abstraction.Background_Jobs;

public interface IBackgroundJobService
{
    void Enqueue(Expression<Func<Task>> job);
}