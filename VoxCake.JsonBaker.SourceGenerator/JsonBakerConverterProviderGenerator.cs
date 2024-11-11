using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public static class JsonBakerAssemblyConverterProviderGenerator
{
    public static void Generate(CodeWriter codeWriter, List<(ITypeSymbol type, string converterName)> processedTypes)
    {
        using(codeWriter.Scope("public class JsonBakerAssemblyConverterProvider : JsonBakerAssemblyConverterProviderBase"))
        {
            codeWriter.WriteLine("private Dictionary<Type, JsonConverter> _converters;");
            codeWriter.WriteLine("private bool _initialized;");
            codeWriter.EmptyLine();
            
            using(codeWriter.Scope("public override JsonConverter GetConverter(Type type)"))
            {
                using (codeWriter.Scope("if (!_initialized)"))
                {
                    codeWriter.WriteLine("Initialize();");
                    codeWriter.WriteLine("_initialized = true;");
                }

                codeWriter.WriteLine("_converters.TryGetValue(type, out var converter);");
                
                codeWriter.WriteLine("return converter;");
            }
            
            using(codeWriter.Scope("private void Initialize()"))
            {
                using (codeWriter.Scope("_converters = new Dictionary<Type, JsonConverter>(16)", ";"))
                {
                    var index = 0;
                    
                    foreach (var processedType in processedTypes)
                    {
                        var inner = $"typeof({processedType.type.ToDisplayString()}), new {processedType.converterName}()";
                        var endSymbol = index + 1 < processedTypes.Count ? "," : "";
                        
                        codeWriter.WriteLine("{ " + inner + " }" + endSymbol);

                        index++;
                    }
                }
            }
        }
    }
}