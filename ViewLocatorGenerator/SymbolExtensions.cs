using Microsoft.CodeAnalysis;

namespace ViewLocatorGenerator;

public static class SymbolExtensions
{
    /// <summary>
    ///     Determines if a type symbol inherits from a specified base type
    /// </summary>
    /// <param name="symbol">The type symbol to check</param>
    /// <param name="fullName">Full name of the potential base type (using symbol display format)</param>
    /// <returns>
    ///     True if the type hierarchy contains the specified base type,
    ///     false otherwise or if symbol is null
    /// </returns>
    public static bool InheritsFrom(this INamedTypeSymbol symbol, string fullName)
    {
        var current = symbol;
        while (current != null)
        {
            if (current.ToDisplayString() == fullName)
                return true;
            current = current.BaseType;
        }

        return false;
    }
}
