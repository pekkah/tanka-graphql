//HintName: InterfaceType.g.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Executable;

namespace Tanka.GraphQL.Server;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public class InterfaceTypeAttribute: Attribute
{
}