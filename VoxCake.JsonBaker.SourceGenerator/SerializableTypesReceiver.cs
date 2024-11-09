using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace VoxCake.JsonBaker.SourceGenerator;

public class SerializableTypesReceiver : ISyntaxContextReceiver
{
    public HashSet<INamedTypeSymbol> SerializableTypes { get; } = new HashSet<INamedTypeSymbol>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is InvocationExpressionSyntax invocationExpr)
        {
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpr).Symbol as IMethodSymbol;
            if (methodSymbol == null)
                return;
            
            if (methodSymbol.Name == "SerializeObject" &&
                methodSymbol.ContainingType.ToDisplayString() == "Newtonsoft.Json.JsonConvert")
            {
                var arguments = invocationExpr.ArgumentList.Arguments;
                if (arguments.Count >= 1)
                {
                    var objectExpression = arguments[0].Expression;
                    var typeInfo = context.SemanticModel.GetTypeInfo(objectExpression);

                    if (typeInfo.Type is INamedTypeSymbol namedTypeSymbol)
                    {
                        SerializableTypes.Add(namedTypeSymbol);

                        AddGenericTypeArguments(namedTypeSymbol);
                    }
                }
            }
            
            if (methodSymbol.Name == "DeserializeObject" &&
                methodSymbol.ContainingType.ToDisplayString() == "Newtonsoft.Json.JsonConvert")
            {
                if (methodSymbol.IsGenericMethod)
                {
                    var typeArgument = methodSymbol.TypeArguments.FirstOrDefault();
                    if (typeArgument is INamedTypeSymbol namedTypeSymbol)
                    {
                        SerializableTypes.Add(namedTypeSymbol);
                    }
                }
                else
                {
                    var arguments = invocationExpr.ArgumentList.Arguments;
                    if (arguments.Count >= 2)
                    {
                        var typeArgumentExpression = arguments[1].Expression;
                        var typeInfo = context.SemanticModel.GetTypeInfo(typeArgumentExpression);

                        if (typeInfo.Type != null && typeInfo.Type.Name == "Type")
                        {
                            if (typeArgumentExpression is TypeOfExpressionSyntax typeofExpr)
                            {
                                var typeSyntax = typeofExpr.Type;
                                var typeSymbol = context.SemanticModel.GetSymbolInfo(typeSyntax).Symbol as INamedTypeSymbol;
                                if (typeSymbol != null)
                                {
                                    SerializableTypes.Add(typeSymbol);
                                }
                            }
                            else if (typeArgumentExpression is IdentifierNameSyntax identifierName)
                            {
                                var symbolInfo = context.SemanticModel.GetSymbolInfo(identifierName).Symbol;

                                if (symbolInfo is ILocalSymbol localSymbol && localSymbol.HasConstantValue)
                                {
                                    var constantValue = localSymbol.ConstantValue as INamedTypeSymbol;
                                    if (constantValue != null)
                                    {
                                        SerializableTypes.Add(constantValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void AddGenericTypeArguments(INamedTypeSymbol typeSymbol)
    {
        foreach (var typeArg in typeSymbol.TypeArguments)
        {
            if (typeArg is INamedTypeSymbol namedTypeArg)
            {
                SerializableTypes.Add(namedTypeArg);

                AddGenericTypeArguments(namedTypeArg);
            }
        }
    }
}