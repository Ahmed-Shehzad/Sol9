using Intercessor.Abstractions;

namespace TestApi.Commands;

public class CreateUserCommand : ICommand
{
    public string Name { get; init; }

    public CreateUserCommand(string name)
    {
        Name = name;
    }
}