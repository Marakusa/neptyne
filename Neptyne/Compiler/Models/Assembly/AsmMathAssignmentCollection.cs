using System.Collections.Generic;
using Neptyne.Compiler.Exceptions;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmMathAssignmentCollection
{
    private string _originalValue;
    private readonly List<MathAssignment> _calculations;

    public AsmMathAssignmentCollection(string originalValue)
    {
        _originalValue = originalValue;
        _calculations = new();
    }

    public void Add(MathAssignmentType assignmentType, string value, int line)
    {
        _calculations.Add(new(assignmentType, value, line));
    }

    public override string ToString()
    {
        string result = "";
        foreach (var statement in GetStatements())
        {
            result += $"    {statement}\n";
        }
        return result;
    }

    public List<AsmStatement> GetStatements()
    {
        List<AsmStatement> statements = new();
        
        foreach (var calculation in _calculations)
        {
            if (calculation.Type == MathAssignmentType.None)
            {
                statements.Add(new("mov", $"edx, {calculation.Value}"));
            }
            else if (calculation.Type == MathAssignmentType.Add)
            {
                statements.Add(new("mov", $"eax, {calculation.Value}"));
                statements.Add(new("add", "eax, edx"));
            }
            else
                throw new CompilerException("Calculation assignment type not implemented yet", calculation.Line);
        }
        
        statements.Add(new("mov", $"{_originalValue}, eax"));
        return statements;
    }
}

public enum MathAssignmentType
{
    None, Add, Substract, Multiply, Divide
}

public class MathAssignment
{
    public MathAssignmentType Type { get; }

    public string Value { get; }
    
    public int Line { get; }

    public MathAssignment(MathAssignmentType assignmentType, string value, int line)
    {
        Type = assignmentType;
        Value = value;
        Line = line;
    }
}
