namespace ProtoHackersDotNet.GUI.MainView.Grader.Messages;

/// <summary>Status of the external checker.</summary>
/// <remarks>For JSON communication. Not to be confused with our internal tracking.</remarks>
public enum GraderStatus { Checking, Fail, Pass }