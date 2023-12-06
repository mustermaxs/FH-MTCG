using System.Text.Json.Serialization;

namespace MTCG;

public abstract class IRequest
{
    /// IMPORTANT
    /// ? wann, wo und wie JSON deserialisieren
    /// wann, wo und wie serialisieren für response
    /// IDEE: model hat eigene ToJsonString methode
    protected string? payload;
    public string? Payload => payload ?? "";
}