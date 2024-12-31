namespace BuildingBlocks.Contracts.Services.Users;

public interface IUserService
{
    Ulid? UserId { get; }
    void SetUserId(Ulid userId);
}