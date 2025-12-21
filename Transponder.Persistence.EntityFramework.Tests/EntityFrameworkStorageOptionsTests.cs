namespace Transponder.Persistence.EntityFramework.Tests;

public sealed class EntityFrameworkStorageOptionsTests
{
    [Fact]
    public void Constructor_Validates_Table_Names()
    {
        Assert.Throws<ArgumentException>(() => new EntityFrameworkStorageOptions(outboxTableName: " "));
        Assert.Throws<ArgumentException>(() => new EntityFrameworkStorageOptions(inboxTableName: " "));
        Assert.Throws<ArgumentException>(() => new EntityFrameworkStorageOptions(scheduledMessagesTableName: " "));
        Assert.Throws<ArgumentException>(() => new EntityFrameworkStorageOptions(sagaStatesTableName: " "));
    }

    [Fact]
    public void Constructor_Uses_Null_For_Empty_Schema()
    {
        var options = new EntityFrameworkStorageOptions(schema: " ");

        Assert.Null(options.Schema);
    }
}
