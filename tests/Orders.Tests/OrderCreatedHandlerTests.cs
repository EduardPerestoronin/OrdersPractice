using Moq;
using Orders.Abstractions;
using Orders.Handlers;
using Orders.Models;

namespace Orders.Tests;

public class OrderCreatedHandlerTests
{
    readonly Mock<INotificationSender> _sender = new();
    readonly OrderCreatedHandler _handler;

    public OrderCreatedHandlerTests() =>
        _handler = new(_sender.Object);

    // 19
    [Fact]
    public async Task HandleAsync_ValidMessage_SendsNotification()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        await _handler.HandleAsync(new OrderCreated(id));

        // Assert
        _sender.Verify(s => s.SendAsync(
            It.Is<string>(t => t.Contains(id.ToString()))), Times.Once());
    }

    // 20
    [Fact]
    public async Task HandleAsync_EmptyOrderId_Throws()
    {
        // Arrange
        var msg = new OrderCreated(Guid.Empty);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.HandleAsync(msg));

        // Assert
        Assert.Contains("OrderId", ex.Message);
        _sender.Verify(s => s.SendAsync(It.IsAny<string>()), Times.Never());
    }

    // 21
    [Fact]
    public async Task HandleAsync_NullMessage_Throws()
    {
        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
    }
}
