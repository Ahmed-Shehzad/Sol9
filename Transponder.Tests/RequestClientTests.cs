using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Transponder.Abstractions;
using Transponder.Transports.Abstractions;

namespace Transponder.Tests;

public sealed class RequestClientTests
{
    private readonly TransponderBus _bus;
    private readonly IMessageSerializer _serializer;
    private readonly Mock<ITransportHostProvider> _hostProviderMock;
    private readonly ILogger<RequestClient<TestRequest>> _logger;
    private readonly Uri _destinationAddress;
    private readonly TimeSpan _timeout;

    public RequestClientTests()
    {
        _serializer = new JsonMessageSerializer();
        _hostProviderMock = new Mock<ITransportHostProvider>();
        _bus = TestHelpers.CreateTestBus(_serializer, _hostProviderMock.Object);
        _logger = NullLogger<RequestClient<TestRequest>>.Instance;
        _destinationAddress = new Uri("http://test/requests");
        _timeout = TimeSpan.FromSeconds(5);
    }

    [Fact]
    public async Task GetResponseAsync_Should_Timeout_When_No_Response_Received()
    {
        // Arrange
        var responseHostMock = new Mock<ITransportHost>();
        var responseEndpointMock = new Mock<IReceiveEndpoint>();
        responseEndpointMock.Setup(e => e.StartAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var destinationHostMock = new Mock<ITransportHost>();
        var sendTransportMock = new Mock<ISendTransport>();
        sendTransportMock.Setup(e => e.SendAsync(It.IsAny<ITransportMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        destinationHostMock.Setup(h => h.GetSendTransportAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sendTransportMock.Object);
        
        _hostProviderMock.Setup(h => h.GetHost(It.Is<Uri>(u => u.ToString().Contains("response"))))
            .Returns(responseHostMock.Object);
        _hostProviderMock.Setup(h => h.GetHost(It.Is<Uri>(u => u.ToString().Contains("requests"))))
            .Returns(destinationHostMock.Object);
        responseHostMock.Setup(h => h.ConnectReceiveEndpoint(It.IsAny<IReceiveEndpointConfiguration>()))
            .Returns(responseEndpointMock.Object);

        var client = new RequestClient<TestRequest>(
            _bus,
            _serializer,
            _hostProviderMock.Object,
            _destinationAddress,
            TimeSpan.FromMilliseconds(100),
            _logger);

        var request = new TestRequest();

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(async () =>
            await client.GetResponseAsync<TestResponse>(request));
    }

    [Fact]
    public async Task EnsureResponseEndpointAsync_Should_Only_Initialize_Once()
    {
        // Arrange
        var hostMock = new Mock<ITransportHost>();
        var endpointMock = new Mock<IReceiveEndpoint>();
        endpointMock.Setup(e => e.StartAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _hostProviderMock.Setup(h => h.GetHost(It.IsAny<Uri>()))
            .Returns(hostMock.Object);
        hostMock.Setup(h => h.ConnectReceiveEndpoint(It.IsAny<IReceiveEndpointConfiguration>()))
            .Returns(endpointMock.Object);

        var client = new RequestClient<TestRequest>(
            _bus,
            _serializer,
            _hostProviderMock.Object,
            _destinationAddress,
            _timeout,
            _logger);

        // Act - Call multiple times concurrently
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(async () =>
            {
                try
                {
                    await client.GetResponseAsync<TestResponse>(new TestRequest());
                }
                catch
                {
                    // Expected to timeout
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert - Endpoint should only be created once
        hostMock.Verify(h => h.ConnectReceiveEndpoint(It.IsAny<IReceiveEndpointConfiguration>()), Times.Once);
        endpointMock.Verify(e => e.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class TestRequest : IMessage
    {
        public Ulid? CorrelationId { get; set; }
    }

    private sealed class TestResponse : IMessage
    {
        public Ulid? CorrelationId { get; set; }
    }
}
