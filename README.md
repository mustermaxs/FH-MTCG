
# README
# MTCG

Developed by: Maximilian Sinnl
## Custom features
## Multilanguage
> In hindsight it would have been nice to have a TranslationService that handles language preferences concerning every output and works a bit more general than the dedicated classes now do - but I only thought of this when I already finished the `ResponseTextTranslator`.
+ there's a german and an english translation for every status message and for the battle gameplay output that the clients receive
+ `BattleConfig` and `ResponseTranslationService` read `.json` files via `JsonConfigLoader` and extract the relevant information
+ the prefered language gets stored inside the config objects and redundantly also in the `Session` and the `User`


# Architecture

## Services & Configs

## Controller
+ are identified by the custom attribute `ControllerAttribute`
+ handle client requests by implementing dedicated methods
+ dedicated Endpoint methods are identified by the custom attribute `RouteAttribute`
+ receive a `Request` object from the `Router` class
+ handle database operations via implementations of the abstract `BaseRepository` class
+ methods with the `RouteAttribute` return a `IResponse` object (or `Task<IResponse>`) to the router

## RouteAttribute
+ used to register methods as endpoint handlers
E.g. `[Route("/users/{username:alphanum}", HTTPMethod.PUT, Role.USER | Role.ADMIN)]`
+ the url defines a template for the `UrlParser` class, that generates a *Regex* pattern from it to handle named parameters and query strings
+ HTTPMethod.*MethodName* HTTP-method
+ `Role.*` define the access level a client must have to be able to access this endpoint
	+ get stored as `Enum` because bitwise operations make it pretty easy to check and compare the permission level even with access groups
		+ e.g. `Role.ALL = Role.USER | Role.ADMIN | Role.ANONYMOUS`

## RouteRegistry
+ used to register endpoint handlers
+ can be used together with `ReflectionRouteObtainer` to register `Endpoint` objects
+ `MapRequestToEndpoint(ref IRequest request)` uses `UrlParser` to check if one of the registered URLs matches the incoming client request concerning the HTTP method, access rights, expected named parameters, ...
+ in case a endpoint was found, it completes the provided `IRequest` object with the `Endpoint` object
+ throws a `RouteDoesntExistException` in case no registered endpoint fits the request




## ReflectionRouteObtainer
+ scans the source code for the `ControllerAttribute`s and `RouteAttribute`s to register the endpoints along with their corresponding:
	+ controller types and controller methods
	+ access rights
	+ URL templates (patterns)
	+ URL Regex patterns
	+ ...


## ReflectionUtils
+ used in `Router.HandleRequest` to map variable arguments in the URI to the expected parameter list in the controller methods that handle an endpoint
+ one implementation of `MapArgumentsAndInvoke` handles synchronized method calls and one asynchronous method calls
+ since the named params are received as type `string`, they get converted/cast/parsed to the data types of the parameters defined in the controller methods by `ReflectionUtils.MapProvidedArgumentsToSignature`
	+ e.g.  `alphanum` translates to "alphanumeric" value
	+ the alphanumeric value will get parsed as type Guid
	+ ![[Pasted image 20240115161959.png]]

### ServiceProvider

+ Registeres config objects implementing the `IService` interface.
+ Provides config objects and corresponding data via static *getter*s.
+ Entities can be statically registered or just by their type, so that when `GetDisposable` is called, a fresh instance is returned.

  

### IConfig

+ contract for `*Config` objects
+ contains information on where to find config data (json-files) and under which section (json object key)
+ provides name for config object to prevent overriding of the same config object
+ Loads data for config objects from file path that is passed via `Load` method.
## Response
+ Response objects implement `abstract class BaseJsonResponse : IResponse`
+ responsible for **serializing** the payload
+ if payload is `IModel` => make use of their `object ToSerializableObj` method that only exposes the fields that the client is supposed to see, whislt ignoring sensitive or irrelevant information like `User::Password` etc. and takes care of providing nested `IModel` objects (in case there are any).
+ contain payload as serialized JSON string
+ contains payload as instance of `IModel`
+ contains response code and optional description
## Request
+ client request
+ contains HTTP headers, payload, etc
+ `T? PayloadAsObject<T>()` tries to deserialize payload to provided `T where T is IModel`

