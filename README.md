# MTCG
Developed by: Maximilian Sinnl

## Custom features
### Buy specific package
`POST /packages/vip`
A user can request a list of all available packages for a fee of 15 coins. Upon receiving the list, the user is given 1 minute to decide and buy a specific package from the list (for the normal amount of coins).


# Architecture
## Config
### ConfigService
+ Singleton
+ Registeres config objects implementing the `IConfig` interface.
+ Loads data for config objects from `config_*.json` files.
+ Provides config objects and corresponding data via static *getter*s.

### IConfig
+ contract for `*Config` objects
+ contains information on where to find config data (json-files) and under which section (json object key)
+ provides name for config object to prevent overriding of the same config object

## Response
+ Response objects implement `abstract class BaseJsonResponse`
+ responsible for **serializing** the payload
  + if payload is `IModel` => make use of their `object ToSerializableObj` method
  that only exposes the fields that the client is supposed to see, whislt ignoring sensitive or irrelevant information like `User::Password` etc.
+ contain payload as serialized JSON string

## Request
+ client request
+ contains HTTP headers, payload, etc
+ `T? PayloadAsObject<T>()` tries to deserialize payload to provided `T where T is IModel`

