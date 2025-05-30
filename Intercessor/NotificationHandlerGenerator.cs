using Intercessor.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Intercessor;

/// <summary>
/// A source generator that automatically creates notification handlers for Intercessor INotification implementations.
/// </summary>
[Generator]
public class NotificationHandlerGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental source generator by registering a syntax provider that scans for types
    /// implementing the INotification interface from Intercessor. When such types are found,
    /// this generator produces corresponding INotificationHandler implementations
    /// as compile-time-generated C# classes. The initialization context provided by the Roslyn compiler. 
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var requestCandidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { BaseList: not null },
                transform: static (ctx, _) => GetSemanticTarget(ctx))
            .Where(static t => t is not null);

        var compilationAndNotifications = context.CompilationProvider.Combine(requestCandidates.Collect());
        context.RegisterSourceOutput(compilationAndNotifications, static (spc, source) =>
        {
            var (compilation, notifications) = source;

            foreach (var notif in notifications.Distinct())
            {
                if (notif?.SyntaxTree == null) continue;

                var semanticModel = compilation.GetSemanticModel(notif.SyntaxTree);

                if (semanticModel.GetDeclaredSymbol(notif) is not INamedTypeSymbol symbol) continue;

                var notificationName = symbol.Name;
                var ns = symbol.ContainingNamespace.ToDisplayString();

                // Skip if handler already exists in source or referenced assemblies
                var handlerName = $"{notificationName}Handler";
                if (compilation.GetTypeByMetadataName($"{ns}.{handlerName}") is not null)
                    continue;

                var code = $$"""
                             // <auto-generated />
                             using System.Threading;
                             using System.Threading.Tasks;
                             using Intercessor.Abstractions;

                             namespace {{ns}};

                             public sealed class {{handlerName}} : INotificationHandler<{{notificationName}}>
                             {
                                 public Task HandleAsync({{notificationName}} notification, CancellationToken cancellationToken = default)
                                 {
                                     // TODO: Implement logic
                                     throw new NotImplementedException();
                                 }
                             }
                             """;

                spc.AddSource($"{handlerName}.cs", code);
            }
        });
    }
    
    private static ClassDeclarationSyntax? GetSemanticTarget(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
        if (symbol is not INamedTypeSymbol classSymbol)
            return null;

        var implementsINotification = classSymbol.AllInterfaces
            .Any(i =>
                i.Name == nameof(INotification) &&
                i.ContainingNamespace.ToDisplayString().StartsWith(nameof(Intercessor)));

        return implementsINotification ? classDecl : null;
    }
}