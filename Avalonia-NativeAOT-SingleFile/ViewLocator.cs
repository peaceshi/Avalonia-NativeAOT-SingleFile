using System;
using System.Collections.Generic;
using Avalonia_NativeAOT_SingleFile.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Avalonia_NativeAOT_SingleFile;

/// <summary>
///     Provides view resolution infrastructure for Avalonia view-model-first patterns
/// </summary>
/// <remarks>
///     Implements <see cref="IDataTemplate" /> to handle view/viewmodel associations.
///     Maintains a static registry of view factories mapped to specific view model types.
///     When used with <see cref="ViewModelBase" /> derived view models:
///     <list type="bullet">
///         <item>Automatically matches view models through <see cref="Match" /></item>
///         <item>Resolves views using the registered factory methods in <see cref="Registration" /></item>
///         <item>Returns fallback UI for unmapped types</item>
///     </list>
///     Implement registration logic in <see cref="RegisterMappings" /> partial method
///     to define view/viewmodel relationships.
/// </remarks>
/// <seealso cref="ViewModelBase" />
/// <seealso cref="IDataTemplate" />
public partial class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> Registration = new();

    #region IDataTemplate Members

    public Control Build(object? param)
    {
        var type = param?.GetType();
        if (type == null || !Registration.TryGetValue(type, out var factory))
            return new TextBlock { Text = $"View not found for {type?.Name}" };

        return factory();
    }

    public bool Match(object? data) => data is ViewModelBase;

    #endregion

    public static partial void RegisterMappings();
}