using System.Text.Json.Serialization;

namespace MTCG;

public abstract class IRequest
{
    /// IMPORTANT
    /// include payload (JSON)
    /// 12.02.2023 02:19
    /// IMPORTANT
    /// ? wann, wo und wie JSON deserialisieren
    /// wann, wo und wie serialisieren für response
    /// IDEE: model hat eigene ToJsonString methode
    protected string? payload;
    public string? Payload => payload ?? "";
}