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
public partial class ViewLocatorGenerator : IIncrementalGenerator
{
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
        var syntaxProvider = context.SyntaxProvider;

        var rootNamespaceProvider = syntaxProvider.CreateSyntaxProvider(
                (node, _) => node is ClassDeclarationSyntax,
                (syntaxContext, _) => (Class: syntaxContext.Node, Model: syntaxContext.SemanticModel))
            .Select((valueTuple, _) => valueTuple.Model.GetDeclaredSymbol(valueTuple.Class))
            .Select((valueTuple, _) => valueTuple?.ContainingNamespace.ToDisplayString().Split('.').First());

        // Creates a syntax provider that filters and transforms syntax nodes
        // into a tuple containing the class declaration and its semantic model.
        var classDeclarationProvider = syntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => (Class: (ClassDeclarationSyntax)ctx.Node, Model: ctx.SemanticModel))
            .Where(x => x.Class != null);

        // Filtering view models from class declarations,
        // selecting their declared symbols, ensuring they are not null.
        var viewModelsProvider = classDeclarationProvider
            .Where(x => IsViewModel(x.Class, x.Model))
            .Select((x, _) => x.Model.GetDeclaredSymbol(x.Class) as INamedTypeSymbol)
            .Where(s => s != null);

        // Filters class declarations to identify views based on the IsView method
        // Maps each class declaration to its declared symbol, casting to INamedTypeSymbol
        // Removes any null symbols from the collection.
        var viewsProvider = classDeclarationProvider
            .Where(x => IsView(x.Class, x.Model))
            .Select((x, _) => x.Model.GetDeclaredSymbol(x.Class) as INamedTypeSymbol)
            .Where(s => s != null);

        // Combines multiple view models into a single combined model
        // Collect **ALL** provider here then combine to multiSourceProvider.
        var viewSourceProvider = viewsProvider.Collect().Combine(viewModelsProvider.Collect());
        var multiSourceProvider = viewSourceProvider.Combine(rootNamespaceProvider.Collect());

        // Registers a source output for the 'combined' source with a delegate
        // that generates code using the left and right sources.
        context.RegisterSourceOutput(multiSourceProvider, (spc, valueTuple) =>
        {
            var ((viewSymbols, viewModelSymbols), rootNamespace) = valueTuple;
            Generator(spc, rootNamespace!, viewSymbols!, viewModelSymbols!);
        });
    }

    #endregion

    /// <summary>
    ///     Generates source code for view locator registration mappings between view models and views.
    ///     Creates ViewLocator.g.cs with mapping registrations.
    /// </summary>
    /// <param name="context">Source production context for adding source files and reporting diagnostics</param>
    /// <param name="viewModels">Collection of view model type symbols to generate registrations for</param>
    /// <param name="rootNamespace">Root Namespace</param>
    /// <param name="views">Collection of all available view type symbols in the application</param>
    /// <exception cref="Diagnostic">Throws diagnostic exception (VLGEN0001) if code generation fails</exception>
    /// <remarks>
    ///     - Matches views to view models using naming convention (DeriveViewName)
    ///     - Generates strongly-typed view instantiation code
    ///     - Includes error handling with diagnostic reporting
    /// </remarks>
    private static void Generator(
        SourceProductionContext context,
        ImmutableArray<string> rootNamespace,
        ImmutableArray<INamedTypeSymbol> views,
        ImmutableArray<INamedTypeSymbol> viewModels)
    {
        try
        {
            var mappings = new StringBuilder();
            var viewModelHashSet = ImmutableHashSet.CreateRange(new NamedTypeSymbolComparer(), viewModels);
            foreach (var viewModel in viewModelHashSet)
            {
                var viewName = DeriveViewName(viewModel);
                var matchedView = views.FirstOrDefault(v => v.Name == viewName);

                if (matchedView != null)
                    mappings.AppendLine(
                        $"            {viewModel.ToDisplayString()} vm => new {matchedView.ToDisplayString()} {{ DataContext = vm }},");
            }

            var viewLocatorSource = $$"""
                                      // <auto-generated/>
                                      using System;
                                      using {{rootNamespace.FirstOrDefault()}}.ViewModels;
                                      using Avalonia.Controls;
                                      using Avalonia.Controls.Templates;

                                      namespace {{rootNamespace.FirstOrDefault()}};

                                      internal sealed class ViewLocator : IDataTemplate
                                      {
                                          public Control Build(object data)
                                          {
                                              return data switch
                                              {
                                      {{mappings}}
                                                  _ => new TextBlock { Text = $"View not found for {data.GetType().Name}" }
                                              };
                                          }

                                          public bool Match(object data) => data is ViewModelBase;
                                      }

                                      """;
            context.AddSource("ViewLocator.g.cs", SourceText.From(viewLocatorSource, Encoding.UTF8));
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
