using Moq;
using NUnit.Framework;
using SimpleL7Proxy.RequestData;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SimpleL7ProxyTest.RequestDataTests
{

    [TestFixture]
    public class RequestDataTests
    {
        private Mock<HttpListenerRequest> _listenerRequestMock;
        private Mock<HttpListenerResponse> _listenerResponseMock;
        private Mock<HttpListenerContext> _listenerContext;
        private RequestData _requestData;
        private  Mock<Stream> _outputStream;
        private HttpListenerContext CreateMockContext(string url, string method)
        {
            var request = new Mock<HttpListenerRequest>(url,method);
            _outputStream = new Mock<Stream>();
            return new Mock<HttpListenerContext>(request, _outputStream);
        }

       

      

        [Test]
        public void Dispose_DisposesResources()
        {
            var context = CreateMockContext("http://localhost/test", "GET");
            var requestData = new RequestData(context);

            requestData.Dispose();

            Assert.IsNull(requestData.Body);
            Assert.IsNull(requestData.Context);
            Assert.AreEqual(string.Empty, requestData.Path);
            Assert.AreEqual(string.Empty, requestData.Method);
        }

        [Test]
        public async Task DisposeAsync_DisposesResourcesAsync()
        {
            var context = CreateMockContext("http://localhost/test", "GET");
            var requestData = new RequestData(context);

            await requestData.DisposeAsync();

            Assert.IsNull(requestData.Body);
            Assert.IsNull(requestData.Context);
            Assert.AreEqual(string.Empty, requestData.Path);
            Assert.AreEqual(string.Empty, requestData.Method);
        }
    }


}
