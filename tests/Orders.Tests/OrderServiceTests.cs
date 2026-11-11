using Moq;
using Orders.Abstractions;
using Orders.Models;
using Orders.Services;

namespace Orders.Tests;

public class OrderServiceTests
{
    readonly Mock<IOrderRepository> _repo = new();
    readonly Mock<IMessagePublisher> _pub = new();
    readonly OrderService _sut;

    public OrderServiceTests() =>
        _sut = new(_repo.Object, _pub.Object, new DiscountCalculator());

    static NewOrder Cmd() =>
        new(new List<OrderItem> { new("Книга", 300m, 2) });

    static Order OrderWithTotal(decimal total) =>
        new(new List<OrderItem> { new("Товар", total, 1) });

    // 10
    [Fact]
    public async Task CreateAsync_ValidOrder_SavesOrder()
    {
        // Act
        await _sut.CreateAsync(Cmd());

        // Assert
        _repo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once());
    }

    // 11
    [Fact]
    public async Task CreateAsync_ValidOrder_PublishesEventWithSameId()
    {
        // Act
        var id = await _sut.CreateAsync(Cmd());

        // Assert
        _pub.Verify(p => p.PublishAsync(
            It.Is<OrderCreated>(e => e.OrderId == id)), Times.Once());
    }

    // 12
    [Fact]
    public async Task CreateAsync_SaveFails_DoesNotPublish()
    {
        // Arrange
        _repo.Setup(r => r.SaveAsync(It.IsAny<Order>()))
             .ThrowsAsync(new TimeoutException());

        // Act
        await Assert.ThrowsAsync<TimeoutException>(() => _sut.CreateAsync(Cmd()));

        // Assert
        _pub.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Never());
    }

    // 13
    [Fact]
    public async Task CreateAsync_NullCommand_Throws()
    {
        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
    }

    // 14
    [Fact]
    public async Task CreateAsync_EmptyItems_ThrowsAndDoesNotSave()
    {
        // Arrange
        var cmd = new NewOrder(new List<OrderItem>());

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(cmd));

        // Assert
        _repo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Never());
    }

    // 15
    [Fact]
    public async Task CreateAsync_PublishFails_ExceptionPropagates()
    {
        // Arrange
        var error = new InvalidOperationException("Брокер недоступен");
        _pub.Setup(p => p.PublishAsync(It.IsAny<OrderCreated>()))
            .ThrowsAsync(error);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateAsync(Cmd()));

        // Assert
        Assert.Same(error, ex);
    }

    // 16
    [Fact]
    public async Task GetPayableAsync_TotalBelowThreshold_NoDiscount()
    {
        // Arrange
        var order = OrderWithTotal(500m);
        _repo.Setup(r => r.FindAsync(order.Id)).ReturnsAsync(order);

        // Act
        var payable = await _sut.GetPayableAsync(order.Id);

        // Assert
        Assert.Equal(500m, payable);
    }

    // 17
    [Fact]
    public async Task GetPayableAsync_TotalAboveThreshold_AppliesDiscount()
    {
        // Arrange
        var order = OrderWithTotal(1500m);
        _repo.Setup(r => r.FindAsync(order.Id)).ReturnsAsync(order);

        // Act
        var payable = await _sut.GetPayableAsync(order.Id);

        // Assert
        Assert.Equal(1350m, payable);
    }

    // 18
    [Fact]
    public async Task GetPayableAsync_OrderNotFound_Throws()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repo.Setup(r => r.FindAsync(id)).ReturnsAsync((Order?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetPayableAsync(id));
    }
}
