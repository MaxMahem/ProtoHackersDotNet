namespace ProtoHackersDotNet.GUI.MainView.IpIfyClient;

/// <summary>Metadata for deserializing <see cref="IPResponse"/> requests.</summary>
[JsonSerializable(typeof(IPResponse))] internal partial class IpIfyMetaData : JsonSerializerContext;