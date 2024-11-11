#if !JSON_BAKER_DISABLE_SOURCE_GENERATION

using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace VoxCake.JsonBaker.SourceGenerator;

[Generator]
public class SampleSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new BakedTypesReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!(context.SyntaxContextReceiver is BakedTypesReceiver receiver))
            return;

        if (receiver.SerializableTypes.Count < 1)
        {
            return;
        }

        var codeWriter = new CodeWriter(4, "System", "System.Collections.Generic", "Newtonsoft.Json", "VoxCake.JsonBaker");
        var converterNames = new List<(ITypeSymbol type, string converterName)>(receiver.SerializableTypes.Count);
        
        foreach (var typeSymbol in receiver.SerializableTypes)
        {
            var converterName = JsonBakerConverterGenerator.GenerateAndGetConverterName(typeSymbol, codeWriter);
            
            converterNames.Add((typeSymbol, converterName));
        }
        
        JsonBakerAssemblyConverterProviderGenerator.Generate(codeWriter, converterNames);
        
        var generatedCode = codeWriter.Build();

        context.AddSource("JsonBakerAssemblyConverter.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
    }
}

#endif