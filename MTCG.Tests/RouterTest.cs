// using NUnit.Framework;
// using MTCG;
// using Moq;
// using System;
// using NUnit.Framework.Internal;
// using System.IO;

// namespace UnitTest.MTCG
// {

//     [TestFixture]
//     public class Test_Router
//     {
//         IUrlParser urlParser;
//         IEndpointMapper routeRegistry;
//         IAttributeHandler attributeHandler;
//         IRouteObtainer routeObtainer;
//         Router router;
//         string authToken;
//         User user;

//         [SetUp]
//         public void Setup()
//         {
//             Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");
//             urlParser = new UrlParser();
//             routeRegistry = RouteRegistry.GetInstance(urlParser);
//             attributeHandler = new AttributeHandler();
//             routeObtainer = new ReflectionRouteObtainer(attributeHandler);
//             var user = new User();
//             user.UserAccessLevel = Role.USER;
//             var mockLangService = new Mock<ResponseTextTranslator>();
//             mockLangService.Setup(l => l.SetLanguage(It.IsAny<string>())).Returns(true);
//             mockLangService.Setup(l => l[It.IsAny<string>()]).Returns("MOCK RESPONSE");
//             var mockProgram = new Mock<Program>();
//             mockProgram.Setup(p => Program.services.Get<ResponseTextTranslator>()).Returns(mockLangService.Object);
//             var testProgram = mockProgram.Object;
//             router = new Router(routeRegistry, routeObtainer);
//             authToken = SessionManager.CreateAuthToken();
//             SessionManager.CreateSessionForUser(authToken, user);
//         }


//         [Order(0)]
//         [Test]
//         public void ClientHasPermissionToRequest_ReturnsTrueForUserOnPermissionUser()
//         {
//             var request = new Mock<IRequest>();
//             var endpoint = new Mock<IEndpoint>();
//             request.Setup(r => r.TryGetHeader("Authorization", out authToken)).Returns(true);
//             request.Setup(r => r.Endpoint).Returns(endpoint.Object);
//             endpoint.Setup(e => e.AccessLevel).Returns(Role.USER);

//             bool result = router.ClientHasPermissionToRequest(request.Object);

//             Assert.IsTrue(result);
//         }


//         [Order(1)]
//         [Test]
//         public void ClientHasPermissionToRequest_ReturnsTrueForAnonymousAccessLevel()
//         {
//             var request = new Mock<IRequest>();
//             var endpoint = new Mock<IEndpoint>();
//             request.Setup(r => r.Endpoint).Returns(endpoint.Object);
//             endpoint.Setup(e => e.AccessLevel).Returns(Role.ANONYMOUS);

//             bool result = router.ClientHasPermissionToRequest(request.Object);

//             Assert.IsTrue(result);
//         }


//         [Order(2)]
//         [Test]
//         public void ClientHasPermissionToRequest_ReturnsFalseForMissingAuthToken()
//         {
//             var request = new Mock<IRequest>();
//             var endpoint = new Mock<IEndpoint>();
//             endpoint.Setup(e => e.AccessLevel).Returns(Role.ANONYMOUS);
//             SessionManager.EndSession(authToken);
//             request.Setup(r => r.TryGetHeader("Authorization", out authToken)).Returns(true);
//             request.Setup(r => r.Endpoint).Returns(endpoint.Object);
//             endpoint.Setup(e => e.AccessLevel).Returns(Role.USER);
//             bool result = router.ClientHasPermissionToRequest(request.Object);

//             Assert.IsFalse(result);
//         }
//     }
// }