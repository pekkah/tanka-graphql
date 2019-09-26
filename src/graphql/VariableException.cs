using System;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL
{
    public class VariableException : Exception
    {
        public VariableException(string message, string variableName, IType variableType): base(message)
        {
            VariableName = variableName;
            VariableType = variableType;
        }

        public IType VariableType { get; set; }

        public string VariableName { get; set; }
    }
}