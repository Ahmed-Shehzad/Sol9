using Transponder.Core.Abstractions;

namespace TestApi.IntegrationEvents
{
    public class UserCreatedEvent : IIntegrationEvent
    {
        public string Name { get; init; }
    
        public UserCreatedEvent(string name)
        {
            Name = name;
        }
    }
}