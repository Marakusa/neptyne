using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmMathAssignmentCollection
{
    private readonly Dictionary<MathAssignmentType, ParserToken> _calculations;

    public AsmMathAssignmentCollection()
    {
        _calculations = new();
    }

    public void Add(MathAssignmentType assignmentType, ParserToken value)
    {
        _calculations.Add(assignmentType, value);
    }

    public override string ToString()
    {
        List<AsmStatement> statements = new();

        foreach (var calculation in _calculations)
        {
            
        }
    }
}

public enum MathAssignmentType
{
    Add, Substract, Multiply, Divide
}
