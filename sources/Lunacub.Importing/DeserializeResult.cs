namespace Caxivitual.Lunacub.Importing;

internal readonly record struct DeserializeResult(Deserializer Deserializer, object Output, DeserializationContext Context);