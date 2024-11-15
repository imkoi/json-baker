using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public static class JsonBakerAssemblyConverterProviderGenerator
{
    public static void Generate(CodeWriter codeWriter, List<(ITypeSymbol type, string converterName)> processedTypes)
    {
        using(codeWriter.Scope("public class JsonBakerAssemblyConverterProvider : JsonBakerAssemblyConverterProviderBase"))
        {
            codeWriter.WriteLine("private Dictionary<Type, JsonBakerConcreteJsonConverterBase> _converters;");
            codeWriter.WriteEmptyLine();
            
            using(codeWriter.Scope("public override JsonConverter GetConverter(Type type)"))
            {
                codeWriter.WriteLine("_converters.TryGetValue(type, out var converter);");
                
                codeWriter.WriteLine("return converter;");
            }
            
            using(codeWriter.Scope("public override void Initialize(VoxCake.JsonBaker.IJsonBakerConverterResolver converterResolver)"))
            {
                using (codeWriter.Scope("_converters = new Dictionary<Type, JsonBakerConcreteJsonConverterBase>(16)", ";"))
                {
                    var index = 0;
                    
                    foreach (var processedType in processedTypes)
                    {
                        var inner = $"typeof({processedType.type.ToDisplayString()}), new {processedType.converterName}(converterResolver)";
                        var endSymbol = index + 1 < processedTypes.Count ? "," : "";
                        
                        codeWriter.WriteLine("{ " + inner + " }" + endSymbol);

                        index++;
                    }
                }

                using (codeWriter.Scope("foreach(var converterPair in _converters)"))
                {
                    codeWriter.WriteLine("converterPair.Value.Initialize();");
                }
            }
        }
    }
}