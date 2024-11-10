using System.Collections.Generic;

namespace VoxCake.JsonBaker.SourceGenerator;

public static class JsonBakerAssemblyConverterProviderGenerator
{
    public static void Generate(CodeWriter codeWriter, List<(string typeName, string converterName)> converterNames)
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
                    
                    foreach (var converter in converterNames)
                    {
                        var inner = $"typeof({converter.typeName}), new {converter.converterName}()";
                        var endSymbol = index + 1 < converterNames.Count ? "," : "";
                        
                        codeWriter.WriteLine("{ " + inner + " }" + endSymbol);

                        index++;
                    }
                }
            }
        }
    }
}