using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ViewLocatorGenerator;

/// <summary>
///     Incremental source generator that automatically creates view-viewmodel mappings for Avalonia applications.
/// </summary>
/// <remarks>
///     Analyzes project code to:
///     1. Find ViewModels inheriting from ViewModelBase
///     2. Find Views inheriting from Avalonia Control
///     3. Generate ViewLocator registration code based on naming conventions
/// </remarks>
[Generator]
public class ViewLocatorGenerator : IIncrementalGenerator
{
    private static string? _rootNamespace;

    #region IIncrementalGenerator Members

    /// <summary>
    ///     Initializes the generator pipeline
    /// </summary>
    /// <param name="context">Generator initialization context</param>
    /// <remarks>
    ///     Pipeline configuration:
    ///     1. Collects all class declarations
    ///     2. Filters ViewModels and Views
    ///     3. Combines results and generates output
    /// </remarks>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Creates a syntax provider that filters and transforms syntax nodes
        // into a tuple containing the class declaration and its semantic model.
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => (Class: (ClassDeclarationSyntax)ctx.Node, Model: ctx.SemanticModel))
            .Where(x => x.Class != null);

        // Collects view models from class declarations by filtering those that are view models,
        // selecting their declared symbols, ensuring they are not null, and then collecting them.
        var viewModels = classDeclarations
            .Where(x => IsViewModel(x.Class, x.Model))
            .Select((x, _) => x.Model.GetDeclaredSymbol(x.Class) as INamedTypeSymbol)
            .Where(s => s != null)
            .Collect();

        // Filters class declarations to identify views based on the IsView method
        // Maps each class declaration to its declared symbol, casting to INamedTypeSymbol
        // Removes any null symbols from the collection
        // Collects the final list of INamedTypeSymbol representing the views
        var views = classDeclarations
            .Where(x => IsView(x.Class, x.Model))
            .Select((x, _) => x.Model.GetDeclaredSymbol(x.Class) as INamedTypeSymbol)
            .Where(s => s != null)
            .Collect();

        // Combines multiple view models into a single combined model
        var combined = viewModels.Combine(views);

        // Registers a source output for the 'combined' source with a delegate
        // that generates code using the left and right sources.
        context.RegisterSourceOutput(combined, (spc, source) =>
            GenerateViewLocatorRegisterMappingsCode(spc, source.Left!, source.Right!));
    }

    #endregion

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
        _rootNamespace ??= symbol?.ContainingNamespace.ToDisplayString().Split('.').First();
        return symbol?.BaseType?.ToDisplayString() == $"{_rootNamespace}.ViewModels.ViewModelBase";
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

    /// <summary>
    ///     Generates source code for view locator registration mappings between view models and views.
    ///     Creates two files: ViewLocator.g.cs with mapping registrations, and App.g.cs with initialization code.
    /// </summary>
    /// <param name="context">Source production context for adding source files and reporting diagnostics</param>
    /// <param name="viewModels">Collection of view model type symbols to generate registrations for</param>
    /// <param name="allViews">Collection of all available view type symbols in the application</param>
    /// <exception cref="Diagnostic">Throws diagnostic exception (VLGEN0001) if code generation fails</exception>
    /// <remarks>
    ///     - Matches views to view models using naming convention (DeriveViewName)
    ///     - Generates strongly-typed view instantiation code
    ///     - Automatically hooks registration into App.Initialize() method
    ///     - Includes error handling with diagnostic reporting
    /// </remarks>
    private static void GenerateViewLocatorRegisterMappingsCode(
        SourceProductionContext context,
        ImmutableArray<INamedTypeSymbol> viewModels,
        ImmutableArray<INamedTypeSymbol> allViews)
    {
        try
        {
            var mappings = new StringBuilder();
            var viewModelHashSet = ImmutableHashSet.CreateRange(new NamedTypeSymbolComparer(), viewModels);

            foreach (var viewModel in viewModelHashSet)
            {
                var viewName = DeriveViewName(viewModel);
                var matchedView = allViews.FirstOrDefault(v => v.Name == viewName);

                if (matchedView != null)
                    mappings.AppendLine(
                        $"            Registration.Add(typeof({viewModel.ToDisplayString()}), () => new {matchedView.ToDisplayString()}());");
            }

            var viewLocatorSource = $$"""
                                      // <auto-generated/>
                                      namespace {{_rootNamespace}};

                                      public partial class ViewLocator
                                      {
                                          public static partial void RegisterMappings()
                                          {
                                      {{mappings}}
                                          }
                                      }

                                      """;

            var appSource = $$"""
                              // <auto-generated/>
                              using Avalonia;
                              using Avalonia.Markup.Xaml;
                              namespace {{_rootNamespace}};

                              public partial class App
                              {
                                  public override void Initialize()
                                  {
                                       AvaloniaXamlLoader.Load(this);
                                       ViewLocator.RegisterMappings();
                                  }
                              }
                              """;

            context.AddSource("ViewLocator.g.cs", SourceText.From(viewLocatorSource, Encoding.UTF8));
            context.AddSource("App.g.cs", SourceText.From(appSource, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "VLGEN0001",
                    "Generator failed",
                    $"ViewLocator generator failed: {ex.Message}",
                    "Generation",
                    DiagnosticSeverity.Error,
                    true),
                Location.None));
        }
    }

    #region Nested type: NamedTypeSymbolComparer

    /// <summary>
    ///     Compares <see cref="INamedTypeSymbol" /> instances for equality based on their display string representation.
    /// </summary>
    private class NamedTypeSymbolComparer : IEqualityComparer<INamedTypeSymbol>
    {
        #region IEqualityComparer<INamedTypeSymbol> Members

        /// <summary>
        ///     Determines equality by comparing the display strings of two <see cref="INamedTypeSymbol" /> instances.
        /// </summary>
        /// <param name="x">The first symbol to compare</param>
        /// <param name="y">The second symbol to compare</param>
        /// <returns>
        ///     true if both symbols' display strings are identical (including null references), otherwise false
        /// </returns>
        public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y)
            => x?.ToDisplayString() == y?.ToDisplayString();

        /// <summary>
        ///     Generates a hash code based on the symbol's display string representation.
        /// </summary>
        /// <param name="obj">The symbol to generate a hash code for</param>
        /// <returns>
        ///     Hash code of the symbol's display string, or 0 if the symbol is null
        /// </returns>
        public int GetHashCode(INamedTypeSymbol? obj)
            => obj?.ToDisplayString().GetHashCode() ?? 0;

        #endregion
    }

    #endregion
}