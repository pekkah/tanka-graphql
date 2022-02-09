using System;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL
{
    public class VariableException : Exception
    {
        public VariableException(string message, string variableName, INode variableType) : base(message)
        {
            VariableName = variableName;
            VariableType = variableType;
        }

        public string VariableName { get; set; }

        public INode VariableType { get; set; }
    }
}