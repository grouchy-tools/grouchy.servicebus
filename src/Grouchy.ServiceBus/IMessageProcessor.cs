namespace Grouchy.ServiceBus
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMessageProcessor
    {
        Task ProcessAsync<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : class;
    }
}
