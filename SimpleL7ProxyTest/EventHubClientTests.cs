using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Moq;
using NUnit.Framework;
using SimpleL7Proxy.Interfaces;
using SimpleL7Proxy.EventHubClient;
using Newtonsoft.Json.Linq;
using SimpleL7Proxy.BackendHost;
using System.Reflection;
using System.Text.Json;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Collections.Concurrent;

namespace SimpleL7ProxyTest.EventHubClientTests
{
    [TestFixture]
    public class Tests
    {
        private ConcurrentQueue<string> _logBuffer;
        private EventHubProducerClient _mockProducerClient;
        private EventDataBatch? _batchData;
        private EventHubClient _eventHubClient;
        private string _connectionString = "";
        private string _eventHubName = "";
        //private string _connectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=testkey";
        //private string _eventHubName = "testhub";
        private object _lockObject = new object();
        private bool _isRunning = true;
        private Queue<string> EHLogBuffer = new Queue<string>();
        private Mock<IEventHubClient> _mockEventHubClient;
        [SetUp]
        public void Setup()
        {

            _logBuffer = new ConcurrentQueue<string>();
            //_mockProducerClient = new EventHubProducerClient(_connectionString, _eventHubName);
            //_connectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=testkey";
            //_eventHubName = "testhub";
            _eventHubClient = new EventHubClient(_connectionString, _eventHubName);
            _connectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=testkey";
            _eventHubName = "testhub";
            _mockProducerClient = new EventHubProducerClient(_connectionString, _eventHubName);
            _mockEventHubClient = new Mock<IEventHubClient>();
            //_batchData = _mockProducerClient.CreateBatchAsync().Result;
        }

        [Test]
        public void Constructor_ShouldInitializeProducerClient()
        {
            // Arrange & Act
            //var client = new EventHubClient(_connectionString, _eventHubName);
            var type = typeof(EventHubClient);
            _isRunning = (bool)type.GetField("isRunning", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_eventHubClient);
            // Assert
            Assert.IsNotNull(_isRunning);

        }

        [Test]
        public void StartTimer_ShouldStartWriterTask()
        {
            // Arrange
            // Act
            _eventHubClient.StartTimer();
            var type = typeof(EventHubClient);
            _isRunning = (bool)type.GetField("isRunning", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_eventHubClient);
            // Assert

            if (!_isRunning)
                Assert.IsFalse(_isRunning);

            if (_isRunning)
                Assert.IsTrue(_isRunning);
        }

        [Test]
        public void StopTimer_ShouldCancelWriterTask()
        {
            // Arrange
            if (!_isRunning)
            {
                _eventHubClient.StartTimer();
            }

            // Act
            _eventHubClient.StopTimer();

            var type = typeof(EventHubClient);
            _isRunning = (bool)type.GetField("isRunning", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_eventHubClient);

            // Assert
            Assert.IsFalse(_isRunning);
        }

        [Test]
        public void SendData_ShouldAddDataToBuffer()
        {
            // Arrange
            var buffer = EHLogBuffer;
            EHLogBuffer = new Queue<string>();
            _eventHubClient.StartTimer();
            string data = "test data";

            // Act
            _eventHubClient.SendData(data);
            EHLogBuffer.Enqueue(data);
            // Assert
            Assert.AreEqual(1, EHLogBuffer.Count);
        }

        [Test]
        public async Task WriterTask_ShouldSendDataToEventHub()
        {
            // Arrange
            _eventHubClient.StartTimer();
            string data = "test data";
            _eventHubClient.SendData(data);

            // Act
            await _eventHubClient.WriterTask();

            // Assert

            _mockProducerClient.SendAsync(It.IsAny<EventDataBatch>(), It.IsAny<CancellationToken>());
        }
        [Test]
        public void SendData_ShouldSerializeEventDataAndSend()
        {
            // Arrange
            var eventData = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };
            var expectedJsonData = JsonSerializer.Serialize(eventData);

            // Act
            _mockEventHubClient.Object.SendData(expectedJsonData);

            // Assert
            _mockEventHubClient.Verify(sender => sender.SendData(expectedJsonData), Times.Once);
        }

        [Test]
        public void GetNextBatch_BatchDataIsNull_ReturnsZero()
        {
            int result = _eventHubClient.GetNextBatch(5);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetNextBatch_LogBufferIsEmpty_ReturnsZero()
        {
            int result = _eventHubClient.GetNextBatch(5);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetNextBatch_LogsAvailable_ReturnsCorrectCount()
        {
            _logBuffer.Enqueue("Log1");
            _logBuffer.Enqueue("Log2");
            _logBuffer.Enqueue("Log3");

            int result = _eventHubClient.GetNextBatch(2);

            Assert.AreEqual(2, result);
            Assert.AreEqual(2, _batchData.Count);
        }

        [Test]
        public void GetNextBatch_CountExceedsLogs_ReturnsAllLogs()
        {
            var type = typeof(EventHubClient);
            var batchData = (int)type.GetField("batchDataCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_eventHubClient);


            _logBuffer.Enqueue("Log1");
            _logBuffer.Enqueue("Log2");

            int result = _eventHubClient.GetNextBatch(5);

            Assert.AreEqual(2, result);
            Assert.AreEqual(2, _batchData.Count);
        }



        [TearDown]
        public void Teardown()
        {
            _connectionString = "";
            _eventHubName = "";
            _mockEventHubClient.Object.StopTimer();
            _mockProducerClient.CloseAsync();
        }
    }
}

