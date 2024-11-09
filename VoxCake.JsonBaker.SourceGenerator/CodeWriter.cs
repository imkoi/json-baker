using System;
using System.Text;

namespace VoxCake.JsonBaker.SourceGenerator;

public class CodeWriter
{
    private readonly int _scopeIdentSize;
    private readonly StringBuilder _builder = new StringBuilder(1024);
    
    private int _indent;

    public CodeWriter(int scopeIdentSize, params string[] usingLiterals)
    {
        _scopeIdentSize = scopeIdentSize;
        foreach (var usingLiteral in usingLiterals)
        {
            _builder.Append("using ").Append(usingLiteral).AppendLine(";");
        }
        
        _builder.AppendLine();
    }

    public void Write(string text)
    {
        var ident = GetIdent();
        
        _builder.Append(ident).Append(text);
    }

    public void WriteLine(string text)
    {
        var ident = GetIdent();
        
        _builder.Append(ident).AppendLine(text);
    }

    public IDisposable Scope(string code, string? afterScopeCode = default)
    {
        return new CodeScope(this, code, afterScopeCode);
    }

    public void AddIdent()
    {
        _indent += _scopeIdentSize;
    }

    public void RemoveIdent()
    {
        _indent -= _scopeIdentSize;
    }

    public string Build()
    {
        var result = _builder.ToString();

        return result;
    }

    private string GetIdent()
    {
        var idents = new char[_indent];
        for (var i = 0; i < _indent; i++)
        {
            idents[i] = ' ';
        }
        
        return new string(idents);
    }
}

public class CodeScope : IDisposable
{
    private readonly CodeWriter _writer;
    private readonly string? _afterScopeCode;

    public CodeScope(CodeWriter writer, string code, string? afterScopeCode)
    {
        _writer = writer;
        _afterScopeCode = afterScopeCode;
        
        _writer.WriteLine(code);
        _writer.WriteLine("{");
        
        _writer.AddIdent();
    }
    
    public void Dispose()
    {
        _writer.RemoveIdent();
        
        if (string.IsNullOrEmpty(_afterScopeCode))
        {
            _writer.WriteLine("}\n");
        }
        else
        {
            _writer.Write("}");
            _writer.WriteLine(_afterScopeCode!);
        }
    }
}