using Moq;
using NUnit.Framework;
using SimpleL7Proxy.BackendHost;
using SimpleL7Proxy.PriorityQueue;
using System;
using System.Reflection;

[TestFixture]
public class PriorityQueueItemComparerTests
{
    private Mock<PriorityQueueItemComparer<int>> _mockComparer;

    [SetUp]
    public void Setup()
    {
        _mockComparer = new Mock<PriorityQueueItemComparer<int>>();
    }

    [Test]
    public void Compare_ShouldReturnZero_WhenItemsAreNull()
    {
        // Act
        int result = _mockComparer.Object.Compare(null, null);

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void Compare_ShouldReturnPositive_WhenSecondItemIsNull()
    {
        DateTime dTime = DateTime.Now;
        // Arrange
        var item1 = new PriorityQueueItem<int>(1, 1,dTime);

        // Act
        int result = _mockComparer.Object.Compare(item1, null);

        // Assert
        Assert.AreEqual(1, result);
    }

    [Test]
    public void Compare_ShouldReturnNegative_WhenFirstItemHasLowerPriority()
    {
        DateTime dTime = DateTime.Now;
        // Arrange
        var item1 = new PriorityQueueItem<int>(1, 1, dTime);
        var item2 = new PriorityQueueItem<int>(1, 2, dTime);

        // Act
        int result = _mockComparer.Object.Compare(item1, item2);

        // Assert
        Assert.Less(result, 0);
    }

    [Test]
    public void Compare_ShouldReturnPositive_WhenFirstItemHasHigherPriority()
    {
        DateTime dTime = DateTime.Now;
        // Arrange
        var item1 = new PriorityQueueItem<int>(1, 2, dTime);
        var item2 = new PriorityQueueItem<int>(1, 1, dTime);

        // Act
        int result = _mockComparer.Object.Compare(item1, item2);

        // Assert
        Assert.Greater(result, 0);
    }

    [Test]
    public void Compare_ShouldReturnNegative_WhenItemsHaveSamePriorityButFirstIsOlder()
    {
        DateTime dTime = DateTime.Now;
        // Arrange
        var item1 = new PriorityQueueItem<int>(1, 1, dTime);
        Thread.Sleep(100);
        dTime = DateTime.Now;
        var item2 = new PriorityQueueItem<int>(1, 1, dTime);

        // Act
        int result = _mockComparer.Object.Compare(item1, item2);

        // Assert
        Assert.Less(result, 0);
    }

    [Test]
    public void Compare_ShouldReturnPositive_WhenItemsHaveSamePriorityButFirstIsNewer()
    {
        // Arrange
        DateTime dTime = DateTime.Now;
        var item2 = new PriorityQueueItem<int>(1, 1, dTime);
        Thread.Sleep(100);
        dTime = DateTime.Now;
        var item1 = new PriorityQueueItem<int>(1, 1, dTime);

        // Act
        int result = _mockComparer.Object.Compare(item1, item2);

        // Assert
        Assert.Greater(result, 0);
    }
}
