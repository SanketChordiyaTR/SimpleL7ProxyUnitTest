﻿using Moq;
using NUnit.Framework;
using SimpleL7Proxy.PriorityQueue;
using System;

[TestFixture]
public class PriorityQueueTests
{
    private Mock<PriorityQueue<int>> _mockQueue;

    [SetUp]
    public void Setup()
    {
        _mockQueue = new Mock<PriorityQueue<int>>();
    }

    [Test]
    public void Enqueue_ShouldAddItemToQueue()
    {
        // Arrange
        int item = 1;
        int priority = 1;
        DateTime timestamp = DateTime.Now;
        var queueItem = new PriorityQueueItem<int>(item, priority, timestamp);
        // Act
        _mockQueue.Object.Enqueue(queueItem);

        // Assert
        Assert.AreEqual(1, _mockQueue.Object.Count);
        Assert.AreEqual("1 ", _mockQueue.Object.GetItemsAsCommaSeparatedString());
    }

    [Test]
    public void Enqueue_ShouldInsertItemsInCorrectOrder()
    {
        var queueItem = new PriorityQueueItem<int>(3, 3, DateTime.Now);
        // Arrange
        _mockQueue.Object.Enqueue(queueItem);
        queueItem = new PriorityQueueItem<int>(1, 1, DateTime.Now);
        _mockQueue.Object.Enqueue(queueItem);
        queueItem = new PriorityQueueItem<int>(2, 2, DateTime.Now);
        _mockQueue.Object.Enqueue(queueItem);

        // Act
        var result = _mockQueue.Object.GetItemsAsCommaSeparatedString();

        // Assert
        Assert.AreEqual("1 , 2 , 3 ", result);
    }

    [Test]
    public void Dequeue_ShouldRemoveAndReturnHighestPriorityItem()
    {
        var queueItem = new PriorityQueueItem<int>(1, 1, DateTime.Now);
        // Arrange
        _mockQueue.Object.Enqueue(queueItem);
        // Arrange
        queueItem = new PriorityQueueItem<int>(2, 2, DateTime.Now);
        // Arrange
        _mockQueue.Object.Enqueue(queueItem);

        queueItem = new PriorityQueueItem<int>(3, 3, DateTime.Now);
        // Arrange
        _mockQueue.Object.Enqueue(queueItem);

        // Act
        int result = _mockQueue.Object.Dequeue();

        // Assert
        Assert.AreEqual(1, result);
        Assert.AreEqual(2, _mockQueue.Object.Count);
        Assert.AreEqual("2 , 3 ", _mockQueue.Object.GetItemsAsCommaSeparatedString());
    }

    [Test]
    public void Dequeue_ShouldThrowExceptionWhenQueueIsEmpty()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _mockQueue.Object.Dequeue());
        Assert.AreEqual("The queue is empty.", ex.Message);
    }

    [Test]
    public void Count_ShouldReturnCorrectNumberOfItems()
    {
       var queueItem = new PriorityQueueItem<int>(1, 1, DateTime.Now);
        // Arrange
        _mockQueue.Object.Enqueue(queueItem);
        // Arrange
        queueItem = new PriorityQueueItem<int>(2, 2, DateTime.Now);
        // Arrange
        _mockQueue.Object.Enqueue(queueItem);

        // Act
        int count = _mockQueue.Object.Count;

        // Assert
        Assert.AreEqual(2, count);
    }
}
