namespace TestApi.Dtos;

public class UserDto
{
    public string Name { get; init; }
    public UserDto(string name)
    {
        Name = name;
    }
}
