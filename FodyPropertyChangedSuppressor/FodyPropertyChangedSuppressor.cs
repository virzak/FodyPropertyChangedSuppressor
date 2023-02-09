using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace FodyPropertyChangedSuppress
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FodyPropertyChangedSuppressor : DiagnosticSuppressor
    {
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = new[]
        {
            new SuppressionDescriptor("SUPP2953", "IDE0051", "OnChanged suppression")
        }.ToImmutableArray();

        readonly Regex OnChangedPattern = new("^On(.+)Changed$", RegexOptions.Compiled);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                var methodSourceTree = diagnostic.Location.SourceTree;

                if (methodSourceTree == null)
                    continue;

                var methodNode = methodSourceTree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
                if (methodNode is not MethodDeclarationSyntax methodDec)
                    continue;

                // Ensure the method has no parameters
                if (methodDec.ParameterList.Parameters.Count != 0)
                    continue;

                // Determine if the method matches On_PropertyName_Changed pattern
                var matches = OnChangedPattern.Matches(methodDec.Identifier.Text);
                if (matches.Count == 0 || matches.Count > 1)
                    continue;

                var expectedPropertyName = matches[0].Groups[1].Value;

                var classNode = methodDec.Parent;

                if (classNode == null)
                    continue;

                var classModel = context.GetSemanticModel(classNode.SyntaxTree);
                var classDeclaredSymbol = classModel.GetDeclaredSymbol(classNode, context.CancellationToken);

                if (classDeclaredSymbol is not INamedTypeSymbol classDeclaringSymbol)
                    continue;

                var isINotifyPropertyChanged = IsDerivedFromInterface(classDeclaringSymbol, "INotifyPropertyChanged");

                // Check if parent class has AddINotifyPropertyChangedInterfaceAttribute
                bool hasAddINotifyPropertyChangedInterface = classDeclaringSymbol.GetAttributes()
                    .Any(a => a.AttributeClass is { Name: "AddINotifyPropertyChangedInterfaceAttribute" });

                if (!hasAddINotifyPropertyChangedInterface && !isINotifyPropertyChanged)
                    continue;

                var relatedPropertyFound = false;

                // Iterate through all locations (possible partial classes)
                foreach (var loc in classDeclaringSymbol.Locations)
                {
                    var locSourceTree = loc.SourceTree;

                    if (locSourceTree == null)
                        continue;

                    var locnode = locSourceTree.GetRoot(context.CancellationToken).FindNode(loc.SourceSpan);
                    
                    SyntaxList<MemberDeclarationSyntax> members;
                    if (locnode is ClassDeclarationSyntax declaringClass)
                    {
                        members = declaringClass.Members;
                    }
                    else if (locnode is StructDeclarationSyntax declaringStruct)
                    {
                        members = declaringStruct.Members;
                    }
                    else if (locnode is RecordDeclarationSyntax recordDeclaration)
                    {
                        members = recordDeclaration.Members;
                    }
                    else
                        continue;

                    // Iterate through member of (partial) class
                    foreach (var member in members)
                    {
                        // Bail out if not a property
                        if (member is not PropertyDeclarationSyntax property)
                            continue;

                        // Check to see if property name is what's between On and Changed
                        if (property.Identifier.Text.Equals(expectedPropertyName, StringComparison.InvariantCulture))
                        {
                            relatedPropertyFound = true;
                        }
                    }

                    if (relatedPropertyFound)
                        context.ReportSuppression(Suppression.Create(SupportedSuppressions[0], diagnostic));
                }
            }
        }

        /// <summary>
        /// Determines if a class or its base type implement specified interface.
        /// </summary>
        /// <param name="classDeclaringSymbol">Class</param>
        /// <param name="interfaceName">Interface</param>
        /// <returns><see langword="true" /> if <paramref name="classDeclaringSymbol" /> or its base type implement <paramref name="interfaceName"/>; otherwise, <see langword="false" />.</returns>
        bool IsDerivedFromInterface(INamedTypeSymbol classDeclaringSymbol, string interfaceName)
        {
            INamedTypeSymbol? classSymbol = classDeclaringSymbol;
            do
            {
                // Check if parent class implements INotifyPropertyChanged
                foreach (var inheditedInterface in classSymbol.Interfaces)
                {
                    if (inheditedInterface.Name.Equals(interfaceName, StringComparison.InvariantCulture))
                    {
                        return true;
                    }
                }

                classSymbol = classSymbol.BaseType;
            } while (classSymbol != null);
            return false;
        }
    }
}
