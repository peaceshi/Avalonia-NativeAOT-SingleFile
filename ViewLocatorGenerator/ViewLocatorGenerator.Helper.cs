using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ViewLocatorGenerator;

public partial class ViewLocatorGenerator
{
    /// <summary>
    ///     Determines if a class is a valid ViewModel
    /// </summary>
    /// <param name="cls">Class declaration syntax</param>
    /// <param name="model">Semantic model</param>
    /// <returns>
    ///     True if class inherits from {rootNamespace}.ViewModels.ViewModelBase,
    ///     and sets the root namespace from containing namespace
    /// </returns>
    private static bool IsViewModel(ClassDeclarationSyntax cls, SemanticModel model)
    {
        var symbol = model.GetDeclaredSymbol(cls) as INamedTypeSymbol;
        var rootNamespace = symbol?.ContainingNamespace.ToDisplayString().Split('.').First();
        return symbol?.BaseType?.ToDisplayString() == $"{rootNamespace}.ViewModels.ViewModelBase";
    }

    /// <summary>
    ///     Determines if a class is a valid View
    /// </summary>
    /// <param name="cls">Class declaration syntax</param>
    /// <param name="model">Semantic model</param>
    /// <returns>
    ///     True if class inherits from Avalonia.Controls.Control
    /// </returns>
    private static bool IsView(ClassDeclarationSyntax cls, SemanticModel model)
    {
        var symbol = model.GetDeclaredSymbol(cls) as INamedTypeSymbol;
        return symbol?.InheritsFrom("Avalonia.Controls.Control") == true;
    }

    /// <summary>
    ///     Derives View name from ViewModel name using naming conventions
    /// </summary>
    /// <param name="viewModel">ViewModel type symbol</param>
    /// <returns>
    ///     View name based on suffix rules:
    ///     - Remove "ViewModel" suffix
    ///     - Replace "VM" suffix with "View"
    ///     - Append "View" if no suffix matches
    /// </returns>
    private static string DeriveViewName(INamedTypeSymbol viewModel)
    {
        const string viewModelSuffix = "ViewModel";
        const string vmSuffix = "VM";

        var name = viewModel.Name;

        if (name.EndsWith(viewModelSuffix, StringComparison.Ordinal))
            return name.Substring(0, name.Length - viewModelSuffix.Length);

        if (name.EndsWith(vmSuffix, StringComparison.Ordinal))
            return name.Substring(0, name.Length - vmSuffix.Length) + "View";

        return name + "View";
    }
}
