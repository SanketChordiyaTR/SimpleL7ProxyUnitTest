using NUnit.Framework;
using System;
using System.Threading;
using SimpleL7Proxy.PriorityQueue;
using SimpleL7ProxyTest.BackendHostTest;
using Moq;
using System.Timers;

namespace SimpleL7ProxyTest
{
    [TestFixture]
    public class BlockingPriorityQueueTests
    {
        private Mock<BlockingPriorityQueue<int>> _mockQueue;

        [SetUp]
        public void Setup()
        {
            _mockQueue = new Mock<BlockingPriorityQueue<int>>();
            _mockQueue.Object.MaxQueueLength = 3;
            
        }

        [Test]
        public void Enqueue_ShouldAddItemToQueue()
        {
            // Arrange
            int item = 1;
            int priority = 1;
            DateTime dtime = DateTime.Now;

            // Act
            bool result = _mockQueue.Object.Enqueue(item, priority, dtime);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, _mockQueue.Object.Count);
        }

        [Test]
        public void Enqueue_ShouldReturnFalseWhenQueueIsFull()
        {
            DateTime timeSpan = DateTime.Now;
            // Arrange
            _mockQueue.Object.Enqueue(1, 1, timeSpan);
            _mockQueue.Object.Enqueue(2, 2, timeSpan);
            _mockQueue.Object.Enqueue(3, 3, timeSpan);

            // Act
            bool result = _mockQueue.Object.Enqueue(4, 4,timeSpan);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(3, _mockQueue.Object.Count);
        }

        [Test]
        public void Dequeue_ShouldRemoveAndReturnHighestPriorityItem()
        {
            DateTime timeSpan = DateTime.Now;
            // Arrange
            _mockQueue.Object.Enqueue(1, 1, timeSpan);
            _mockQueue.Object.Enqueue(2, 2, timeSpan);
            _mockQueue.Object.Enqueue(3, 3, timeSpan);

            // Act
            int result = _mockQueue.Object.Dequeue(CancellationToken.None,"1");

            // Assert
            Assert.AreEqual(1, result);
            Assert.AreEqual(2, _mockQueue.Object.Count);
        }

        [Test]
        public void Dequeue_ShouldWaitForItemWhenQueueIsEmpty()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000); // Cancel after 1 second

            // Act & Assert
            Assert.Throws<OperationCanceledException>(() => _mockQueue.Object.Dequeue(cancellationTokenSource.Token,"1"));
        }

        [Test]
        public void Enqueue_ShouldSignalDequeue()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            DateTime timeSpan = DateTime.Now;
            var dequeueTask = Task.Run(() => _mockQueue.Object.Dequeue(cancellationTokenSource.Token ,"1"));

            // Act
            _mockQueue.Object.Enqueue(1, 1,timeSpan);

            // Assert
            Assert.AreEqual(1, dequeueTask.Result);
        }
    }
}

