namespace BuildingBlocks.Contracts.Services.Users;

public class UserService : IUserService
{
    public Ulid? UserId { get; private set; }

    public void SetUserId(Ulid userId)
    {
        UserId = userId;
    }
}