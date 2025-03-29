namespace Avalonia_NativeAOT_SingleFile.Messages;

public sealed class DialogResultMessage(string result)
{
    public string Result { get; } = result;
}