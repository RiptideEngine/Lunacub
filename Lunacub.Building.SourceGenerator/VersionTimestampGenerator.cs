using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Lunacub.Building.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class VersionTimestampGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var provider = context.SyntaxProvider.CreateSyntaxProvider<ClassToGenerate?>(
            static (node, token) => node is ClassDeclarationSyntax classDeclaration,
            transform: static (context, token) => {
                var classDeclaration = (ClassDeclarationSyntax)context.Node;

                if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classTypeSymbol) {
                    return null;
                }
                
                // Determine whether the type inherits from our candidate types (Importer, Processor, Serializer).
                INamedTypeSymbol? baseTypeSymbol = classTypeSymbol.BaseType;

                while (baseTypeSymbol != null && !IsQualifiedCandidate(baseTypeSymbol)) {
                    baseTypeSymbol = baseTypeSymbol.BaseType;
                }

                if (baseTypeSymbol == null) return null;
                
                // No need to emit if user already apply the attribute.
                foreach (var attributeList in classDeclaration.AttributeLists) {
                    foreach (var attribute in attributeList.Attributes) {
                        if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attrSymbol) continue;

                        if (attrSymbol.ContainingType.ToDisplayString() == "Caxivitual.Lunacub.Building.Attributes.VersionTimestampAttribute") {
                            return null;
                        }
                    }
                }

                List<INamedTypeSymbol> containingTypes = [];

                CollectContainingTypes(classTypeSymbol.ContainingType, containingTypes);
                
                return new ClassToGenerate(containingTypes, classDeclaration, classTypeSymbol, DateTime.Now);
            }
        ).Where(static result => result is not null);
        
        context.RegisterSourceOutput(provider, static (context, target) => {
            if (!target.HasValue) return;

            string source = GenerateSource(context, target.Value);
            context.AddSource($"{target.Value.ClassDeclaration.Identifier.ToString()}.VersionTimestamp.generated.cs", source);
        });
    }

    private static bool IsQualifiedCandidate(INamedTypeSymbol symbol) {
        return symbol.ContainingNamespace?.ToDisplayString() == "Caxivitual.Lunacub.Building" && symbol.Name is "Importer" or "Processor" or "Serializer";
    }

    private static void CollectContainingTypes(INamedTypeSymbol? containingType, List<INamedTypeSymbol> containingTypes) {
        if (containingType == null) return;
        
        CollectContainingTypes(containingType.ContainingType, containingTypes);
        containingTypes.Add(containingType);
    }

    private static string GenerateSource(SourceProductionContext context, ClassToGenerate target) {
        StringBuilder builder = new StringBuilder();

        int tab = 0;

        if (target.ClassSymbol.ContainingNamespace != null) {
            builder.Append("namespace ").Append(target.ClassSymbol.ContainingNamespace.ToDisplayString()).AppendLine(" {");
            tab += 4;
        }

        foreach (var containingType in target.ContainingTypes) {
            builder.Append(' ', tab).Append("partial ");

            switch (containingType.TypeKind) {
                case TypeKind.Class: builder.Append("class "); break;
                case TypeKind.Struct: builder.Append("struct "); break;
                case TypeKind.Interface: builder.Append("interface "); break;
                default:
                    builder.Clear().Append("// Unknown type kind '").Append(containingType.TypeKind).Append("' for type ").Append(containingType.Name);
                    return builder.ToString();
            }

            builder.Append(containingType.Name).AppendLine(" {");
            tab += 4;
        }

        builder.Append(' ', tab).AppendLine("// Attribute lists:");

        foreach (var attributeList in target.ClassDeclaration.AttributeLists) {
            builder.Append(' ', tab).Append("//");

            foreach (var attribute in attributeList.Attributes) {
                builder.Append(attribute.Name.ToFullString()).Append(", ");
            }

            builder.AppendLine();
        }
        
        builder.Append(' ', tab).Append('[').Append("Caxivitual.Lunacub.Building.Attributes.VersionTimestampAttribute(\"").Append(target.DateTime.ToString("yyyyMMdd_HHmmss")).AppendLine("\")]");
        builder.Append(' ', tab).Append("partial class ").Append(target.ClassDeclaration.Identifier.ToString()).Append(';').AppendLine();

        for (int i = 0; i < target.ContainingTypes.Count; i++) {
            tab -= 4;
            builder.Append(' ', tab).AppendLine("}");
        }

        if (target.ClassSymbol.ContainingNamespace != null) {
            builder.Append("}");
        }

        return builder.ToString();
    }

    private readonly struct ClassToGenerate {
        public readonly IReadOnlyCollection<INamedTypeSymbol> ContainingTypes;
        public readonly ClassDeclarationSyntax ClassDeclaration;
        public readonly INamedTypeSymbol ClassSymbol;
        public readonly DateTime DateTime;

        public ClassToGenerate(IReadOnlyCollection<INamedTypeSymbol> containingTypes, ClassDeclarationSyntax classDeclaration, INamedTypeSymbol classSymbol, DateTime dateTime) {
            ContainingTypes = containingTypes;
            ClassDeclaration = classDeclaration;
            ClassSymbol = classSymbol;
            DateTime = dateTime;
        }
    }
}