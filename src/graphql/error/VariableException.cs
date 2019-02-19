using System;
using tanka.graphql.type;

namespace tanka.graphql.error
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