using Intercessor.Abstractions;
using TestApi.Dtos;

namespace TestApi.Commands;

public class UpdateUserCommand : ICommand<UserDto>
{
    public Guid Id { get; set; }
    public string Name { get; init; }

    public UpdateUserCommand(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}