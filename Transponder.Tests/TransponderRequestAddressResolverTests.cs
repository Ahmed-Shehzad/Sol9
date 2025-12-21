using Transponder;

namespace Transponder.Tests;

public sealed class TransponderRequestAddressResolverTests
{
    private sealed class TestMessage
    {
    }

    private sealed class Outer
    {
        public sealed class Inner
        {
        }
    }

    [Fact]
    public void Create_Builds_Address_With_Custom_Prefix()
    {
        var busAddress = new Uri("https://localhost/api");
        Func<Type, Uri?> resolver = TransponderRequestAddressResolver.Create(
            busAddress,
            "custom",
            _ => "sample-message");

        Uri? resolved = resolver(typeof(TestMessage));

        Assert.Equal(new Uri("https://localhost/api/custom/sample-message"), resolved);
    }

    [Fact]
    public void DefaultPathFormatter_Sanitizes_Type_Name()
    {
        string segment = TransponderRequestAddressResolver.DefaultPathFormatter(typeof(Outer.Inner));

        Assert.DoesNotContain('.', segment);
        Assert.DoesNotContain('+', segment);
        Assert.Contains("Outer", segment);
        Assert.Contains("Inner", segment);
    }
}
