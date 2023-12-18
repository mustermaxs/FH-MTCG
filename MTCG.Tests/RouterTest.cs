using NUnit.Framework;
using MTCG;
using Moq;

namespace UnitTest.MTCG
{

    [TestFixture]
    [Ignore("")]
    public class Test_Router
    {
        IUrlParser urlParser;
        IEndpointMapper routeRegistry;
        IAttributeHandler attributeHandler;
        IRouteObtainer routeObtainer;
        Router router;
        string authToken;
        Mock user;

        [SetUp]
        public void Setup()
        {
            urlParser = new UrlParser();
            routeRegistry = RouteRegistry.GetInstance(urlParser);
            attributeHandler = new AttributeHandler();
            routeObtainer = new ReflectionRouteObtainer(attributeHandler);
            var user = new Mock<User>();

            router = new Router(routeRegistry, routeObtainer);
            authToken = SessionManager.CreateAuthToken();
            SessionManager.CreateSessionForUser(authToken, user.Object);
        }


        [Test]
        public void ClientHasPermissionToRequest_ReturnsTrueForUserOnPermissionUser()
        {
            // Arrange
            
            var request = new Mock<IRequest>();
            request.Setup(m => m.TryGetHeader(It.Is<string>(key => key == "Authorization"), out It.Ref<string>.IsAny))
                   .Returns(true);
            request.Setup(m => m.Endpoint.AccessLevel).Returns(Role.USER);

            var sessionMngr = new Mock<SessionManager>();
            sessionMngr.Setup(m => SessionManager.TryGetSession(authToken, out It.Ref<Session>.IsAny)).Returns(true);
            // Act
            bool result = router.ClientHasPermissionToRequest(request.Object);

            // Assert
            Assert.IsTrue(result);
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
}