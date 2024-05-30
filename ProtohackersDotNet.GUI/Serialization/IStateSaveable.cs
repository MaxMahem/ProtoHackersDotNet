namespace ProtoHackersDotNet.GUI.Serialization;

public interface IStateSaveable<T>
{
    T GetState();
}