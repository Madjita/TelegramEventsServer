using CommonTypes.RabbitDto;

namespace Processing;

public interface IMainProcessing
{
    public Task MainProcessAsync(IBaseRabbitDto data);
}