// AvaloniaApp.MessengerGenerator/Attributes/MessageHandlerAttribute.cs

using System;

namespace MessengerGenerator;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MessageHandlerAttribute : Attribute
{
}