namespace Avalonia_NativeAOT_SingleFile.Messages;

public sealed class RequestCloseMessage(string result)
{
    public string Result { get; } = result;
}
