using NUnit.Framework;
using MTCG;
using Moq;
using System;
using NUnit.Framework.Internal;
using System.IO;

namespace UnitTest.MTCG
{

    [TestFixture]
    public class Test_Router
    {
        IUrlParser urlParser;
        IEndpointMapper routeRegistry;
        IAttributeHandler attributeHandler;
        IRouteObtainer routeObtainer;
        Router router;
        string authToken;
        User user;

        [SetUp]
        public void Setup()
        {
            Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");
            urlParser = new UrlParser();
            routeRegistry = RouteRegistry.GetInstance(urlParser);
            attributeHandler = new AttributeHandler();
            routeObtainer = new ReflectionRouteObtainer(attributeHandler);
            var user = new User();
            user.UserAccessLevel = Role.USER;

            router = new Router(routeRegistry, routeObtainer);
            authToken = SessionManager.CreateAuthToken();
            SessionManager.CreateSessionForUser(authToken, user);
        }


        [Order(0)]
        [Test]
        public void ClientHasPermissionToRequest_ReturnsTrueForUserOnPermissionUser()
        {
            var request = new Mock<IRequest>();
            var endpoint = new Mock<IEndpoint>();
            request.Setup(r => r.TryGetHeader("Authorization", out authToken)).Returns(true);
            request.Setup(r => r.Endpoint).Returns(endpoint.Object);
            endpoint.Setup(e => e.AccessLevel).Returns(Role.USER);

            bool result = router.ClientHasPermissionToRequest(request.Object);

            Assert.IsTrue(result);
        }


        [Order(1)]
        [Test]
        public void ClientHasPermissionToRequest_ReturnsTrueForAnonymousAccessLevel()
        {
            var request = new Mock<IRequest>();
            var endpoint = new Mock<IEndpoint>();
            request.Setup(r => r.Endpoint).Returns(endpoint.Object);
            endpoint.Setup(e => e.AccessLevel).Returns(Role.ANONYMOUS);

            bool result = router.ClientHasPermissionToRequest(request.Object);

            Assert.IsTrue(result);
        }


        [Order(2)]
        [Test]
        public void ClientHasPermissionToRequest_ReturnsFalseForMissingAuthToken()
        {
            var request = new Mock<IRequest>();
            var endpoint = new Mock<IEndpoint>();
            endpoint.Setup(e => e.AccessLevel).Returns(Role.ANONYMOUS);
            SessionManager.EndSession(authToken);
            request.Setup(r => r.TryGetHeader("Authorization", out authToken)).Returns(true);
            request.Setup(r => r.Endpoint).Returns(endpoint.Object);
            endpoint.Setup(e => e.AccessLevel).Returns(Role.USER);
            bool result = router.ClientHasPermissionToRequest(request.Object);

            Assert.IsFalse(result);
        }
    }

    //     [Test]
    //     public void ClientHasPermissionToRequest_ReturnsTrueForAnonymousAccessLevel()
    //     {
    //         // Arrange

    //         Request request = new Mock<IRequest>();

    //         // Act
    //         bool result = router.ClientHasPermissionToRequest(request);

    //         // Assert
    //         Assert.IsTrue(result);
    //     }

    //     [Test]
    //     public void ClientHasPermissionToRequest_ReturnsFalseForMissingAuthToken()
    //     {
    //         // Arrange

    //         Request request = new Mock<IRequest>();

    //         // Act
    //         bool result = router.ClientHasPermissionToRequest(request);

    //         // Assert
    //         Assert.IsFalse(result);
    //     }

    //     [Test]
    //     public void ClientHasPermissionToRequest_ReturnsFalseForInvalidAuthToken()
    //     {
    //         // Arrange

    //         Request request = new Mock<IRequest>();
    //         request.AddHeader("Authorization", "invalid_token");

    //         // Act
    //         bool result = router.ClientHasPermissionToRequest(request);

    //         // Assert
    //         Assert.IsFalse(result);
    //     }

    //     [Test]
    //     public void ClientHasPermissionToRequest_ReturnsFalseForMissingSession()
    //     {
    //         // Arrange

    //         Request request = new Mock<IRequest>();
    //         request.AddHeader("Authorization", "valid_token");

    //         // Act
    //         bool result = router.ClientHasPermissionToRequest(request);

    //         // Assert
    //         Assert.IsFalse(result);
    //     }

    //     [Test]
    //     public void ClientHasPermissionToRequest_ReturnsFalseForInsufficientAccessLevel()
    //     {
    //         // Arrange

    //         Request request = new Mock<IRequest>();
    //         request.AddHeader("Authorization", "valid_token");
    //         SessionManager.AddSession("valid_token", new Session(new User());

    //         // Act
    //         bool result = router.ClientHasPermissionToRequest(request);

    //         // Assert
    //         Assert.IsFalse(result);
    //     }

    //     [Test]
    //     public void ClientHasPermissionToRequest_ReturnsTrueForSufficientAccessLevel()
    //     {
    //         // Arrange

    //         Request request = new Mock<IRequest>();
    //         request.AddHeader("Authorization", "valid_token");
    //         SessionManager.AddSession("valid_token", new Session(new User(), ));

    //         // Act
    //         bool result = router.ClientHasPermissionToRequest(request);

    //         // Assert
    //         Assert.IsTrue(result);
    //     }
    // }

    // // Mock classes for testing
    // public class MockRequest : IRequest
    // {
    //     public Endpoint Endpoint { get; set; }
    //     private Dictionary<string, string> headers;

    //     public MockRequest()
    //     {
    //         Endpoint = new Endpoint(accessLevel);
    //         headers = new Dictionary<string, string>();
    //     }

    //     public bool TryGetHeader(string key, out string value)
    //     {
    //         return headers.TryGetValue(key, out value);
    //     }

    //     public void AddHeader(string key, string value)
    //     {
    //         headers.Add(key, value);
    //     }
    // }
}