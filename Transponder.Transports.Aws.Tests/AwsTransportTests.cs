using Transponder.Transports.Abstractions;
using Transponder.Transports.Aws;

namespace Transponder.Transports.Aws.Tests;

public sealed class AwsTransportTests
{
    private sealed class StubTransportHostSettings : ITransportHostSettings
    {
        public StubTransportHostSettings(Uri address)
        {
            Address = address;
        }

        public Uri Address { get; }
        public IReadOnlyDictionary<string, object?> Settings { get; } = new Dictionary<string, object?>();
    }

    private sealed class SampleMessage
    {
    }

    [Fact]
    public void AwsTopology_Uses_Path_For_Queue_Name()
    {
        var topology = new AwsTopology();
        var address = new Uri("aws://example/queue-name");

        string queueName = topology.GetQueueName(address);

        Assert.Equal("queue-name", queueName);
    }

    [Fact]
    public void AwsTopology_Uses_Type_Name_For_Topic_Name()
    {
        var topology = new AwsTopology();

        string topicName = topology.GetTopicName(typeof(SampleMessage));

        Assert.Equal("SampleMessage", topicName);
    }

    [Fact]
    public void AwsTransportHostSettings_Requires_Region()
    {
        Assert.Throws<ArgumentException>(() => new AwsTransportHostSettings(new Uri("aws://example"), " "));
    }

    [Fact]
    public void AwsTransportFactory_Throws_For_Wrong_Settings()
    {
        var factory = new AwsTransportFactory();
        var settings = new StubTransportHostSettings(new Uri("aws://example"));

        Assert.Throws<ArgumentException>(() => factory.CreateHost(settings));
    }
}
