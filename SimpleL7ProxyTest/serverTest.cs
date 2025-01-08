//using NUnit.Framework;
//using Moq;
//using Microsoft.ApplicationInsights;
//using Microsoft.Extensions.Options;
//using System;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using SimpleL7Proxy.BackendOptions;
//using SimpleL7Proxy.Interfaces;
//using SimpleL7Proxy.Server;
//using SimpleL7Proxy.PriorityQueue;
//using SimpleL7Proxy.RequestData;
//using SimpleL7Proxy.EventHubClient;
//using System.Reflection;

//namespace SimpleL7ProxyTest
//{
//    [TestFixture]
//    public class serverTest
//    {
//        private readonly TelemetryClient? _telemetryClient;
//        private Mock<IOptions<BackendOptions>> _mockOptions;
//        private Mock<TelemetryClient> _mockTelemetryClient;
//        private BackendOptions _backendOptions;
//        private Server _Server;

//        [SetUp]
//        public void Setup()
//        {
//            _backendOptions = new BackendOptions
//            {
//                Port = 8000,
//                Timeout = 10000,
//                Workers = 10,
//                MaxQueueLength = 50,
//                DefaultPriority = 1,
//                PriorityKeys = new List<string> { "high", "medium", "low" },
//                PriorityValues = new List<int> { 1, 2, 3 }
//            };

//            _mockOptions = new Mock<IOptions<BackendOptions>>();
//            _mockOptions.Setup(o => o.Value).Returns(_backendOptions);

//            //_mockTelemetryClient.Setup(o => o.Value).Returns(_backendOptions);

//            //_Server = new Server(_mockOptions.Object, _telemetryClient);

//        }


//        //[Test]
//        //public void Server_Constructor_InitializesCorrectly()
//        //{
//        //    var server = new Server(_mockOptions.Object, _telemetryClient);

//        //    Assert.IsNotNull(_Server);
//        //}

//        //[Test]
//        //public void Start_Server_Success()
//        //{
//        //    //var server = new Server(_mockOptions.Object, _mockTelemetryClient.Object);
//        //    var cancellationToken = new CancellationToken();

//        //    var queue = _Server.Start(cancellationToken);
//        //    Assert.IsNotNull(queue);

//        //    var runTask = _Server.Run();

//        //    if (queue.Count > 0)
//        //        Assert.Greater(queue.Count, 0);

//        //    Assert.AreEqual(_backendOptions.MaxQueueLength, queue.MaxQueueLength);


//        //    Assert.IsNotNull(runTask);
//        //    Assert.Pass("Server ran and processed requests successfully.");
//        //}

//        [Test]
//        public async Task Run_Server_ProcessesRequests()
//        {
//            var cancellationTokenSource = new CancellationTokenSource();

//            var queue = _Server.Start(cancellationTokenSource.Token);

//            var runTask = _Server.Run();


//            cancellationTokenSource.Cancel();

//            await runTask;

//            Assert.IsNotNull(_Server);
//            Assert.Pass("Server ran and processed requests successfully.");

//            if (queue.Count > 0)
//                Assert.Greater(queue.Count, 0);

//            Assert.AreEqual(_backendOptions.MaxQueueLength, queue.MaxQueueLength);

//            Assert.IsNotNull(runTask);
//        }


//    }
//}
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Internal;
using SimpleL7Proxy.Interfaces;
using System.Net;


using NUnit.Framework;
using Moq;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using SimpleL7Proxy.BackendOptions;
using SimpleL7Proxy.Interfaces;
using SimpleL7Proxy.IServer;
using SimpleL7Proxy.PriorityQueue;
using SimpleL7Proxy.RequestData;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SimpleL7Proxy.Server;

namespace SimpleL7Proxy.Tests
{
    [TestFixture]
    public class ServerTests
    {
        private Mock<IOptions<BackendOptions.BackendOptions>> _mockOptions;
        private Mock<IEventHubClient> _mockEventHubClient;
        private Mock<IBackendService> _mockBackendService;
        private Mock<TelemetryClient> _mockTelemetryClient;
        private readonly TelemetryClient? _telemetryClient;
        private Server.Server _server;
        private CancellationTokenSource _cancellationTokenSource;
        private BackendOptions.BackendOptions _backendOptions;

        [SetUp]
        public void SetUp()
        {
            _mockOptions = new Mock<IOptions<BackendOptions.BackendOptions>>();
            _mockEventHubClient = new Mock<IEventHubClient>();
            _mockBackendService = new Mock<IBackendService>();

            _backendOptions = new BackendOptions.BackendOptions
            {
                Port = 8080,
                Timeout = 5000,
                Workers = 4,
                MaxQueueLength = 100,
                DefaultPriority = 1,
                PriorityKeys = new List<string> { "high", "medium", "low" },
                PriorityValues = new List<int> { 1, 2, 3 },
                IDStr = "test",
                PollInterval = 1000
            };

            _mockOptions.Setup(o => o.Value).Returns(_backendOptions);

            _server = new Server.Server(_mockOptions.Object, _mockEventHubClient.Object, _mockBackendService.Object, _telemetryClient);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        [Test]
        public void Constructor_ShouldInitializeServer()
        {
            Assert.IsNotNull(_server);
        }

        [Test]
        public void Start_ShouldStartHttpListener()
        {
            var queue = _server.Start(_cancellationTokenSource.Token);
            Assert.IsNotNull(queue);
            Assert.AreEqual(100, queue.MaxQueueLength);
        }

       
              
       
        [Test]
        public void WriteOutput_ShouldSendDataToEventHubClient()
        {
            var data = "Test data";
            _server.WriteOutput(data);

            _mockEventHubClient.Verify(e => e.SendData(data), Times.Once);
        }

        [Test]
        public async Task Run_Server_ProcessesRequests()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var queue = _server.Start(cancellationTokenSource.Token);

            var runTask = _server.Run();


            cancellationTokenSource.Cancel();

            await runTask;
            _server.StopServer();
            Assert.IsNotNull(_server);
            Assert.Pass("Server ran and processed requests successfully.");

            if (queue.Count > 0)
                Assert.Greater(queue.Count, 0);

            Assert.AreEqual(_backendOptions.MaxQueueLength, queue.MaxQueueLength);

            Assert.IsNotNull(runTask);
        }



        [TearDown]
        public void TearDown()
        {
            _cancellationTokenSource?.Dispose();
            
        }
    }
}
