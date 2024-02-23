﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Scriban;
using Scriban.Runtime;

namespace Tanka.GraphQL.Server.SourceGenerators.Templates;

public class ObjectTemplate
{
    private const string PropertyResolverTemplate =
        """
        {{~ 
        $instance = $"(({name})context.ObjectValue)"
        $instanceOrStatic = property.is_static ? name : $instance
        ~}}
        public static ValueTask {{property.resolver_name}}(ResolverContext context)
        {
            context.ResolvedValue = {{$instanceOrStatic}}.{{property.name}};
            return default;
        }
        """;

    private const string MethodResolverTemplate =
        """
        {{~ 
        $prefix = method.is_async && !method.is_subscription ? "async " : ""
        $instance = $"(({name})context.ObjectValue)"
        $instanceOrStatic = method.is_static ? name : $instance
        ~}}
        public static {{ $prefix }}ValueTask {{method.resolver_name}}(ResolverContext context)
        {
            Before{{method.resolver_name}}(context);
            
            {{~ if method.is_subscription ~}}
            context.ResolvedValue = context.ObjectValue;
            After{{method.resolver_name}}(context);
            return default;
            {{~ else ~}}
            {{~ if method.is_async ~}}
            context.ResolvedValue = await {{$instanceOrStatic}}.{{method.name}}{{~ include "parameters" method ~}};
            After{{method.resolver_name}}(context);
            {{~ else ~}}
            context.ResolvedValue = {{$instanceOrStatic}}.{{method.name}}{{~include "parameters" method ~}};
            
            After{{method.resolver_name}}(context);
            return default;
            {{~ end ~}}
            {{~ end ~}}
        }
        {{~ if method.is_subscription ~}}
        
        public static ValueTask {{method.name}}(SubscriberContext context, CancellationToken cancellationToken)
        {
            Before{{method.name}}(context);
            
            context.SetResult({{$instanceOrStatic}}.{{method.name}}{{~ include "parameters" method ~}});
            
            After{{method.name}}(context);
            return default;
        }
        
        static partial void Before{{method.name}}(SubscriberContext context);
        static partial void After{{method.name}}(SubscriberContext context);
        {{~ end ~}}
        static partial void Before{{method.resolver_name}}(ResolverContext context);
        static partial void After{{method.resolver_name}}(ResolverContext context);
        
        """;

    private const string ParametersTemplate =
        """
        {{~ $parameters = method.parameters ~}}
        {{- if $parameters.size == 0 -}}
        ()
        {{- else -}}
        (
        {{~ for parameter in $parameters ~}}
                {{include "parameter_usage" method parameter}}{{~ if for.last ~}}{{~ else ~}},
                {{~ end ~}}
        {{~ end }}
                )
        {{- end -}}
        """;

    private const string ParameterUsage =
        """
        {{- if parameter.type | string.ends_with "ResolverContext" }}context
        {{- else if parameter.type | string.ends_with "SubscriberContext" }}context
        {{- else if parameter.type | string.ends_with "CancellationToken" -}}
            {{- if method.is_subscription -}}cancellationToken{{- else -}}context.RequestAborted{{- end -}}
        {{- else if parameter.type | string.ends_with "IServiceProvider" }}context.RequestServices
        {{- else if parameter.from_arguments -}}
            {{- if parameter.is_primitive -}}
            context.GetArgument<{{parameter.type}}>("{{parameter.name}}")
            {{- else -}}
            context.BindInputObject<{{parameter.type}}>("{{parameter.name}}")
            {{- end -}}
        {{- else if parameter.from_services -}}
            {{- if parameter.is_nullable -}}
            context.RequestServices.GetService<{{parameter.type}}>()
            {{- else -}}
            context.GetRequiredService<{{parameter.type}}>()
            {{- end -}}
        {{- else if parameter.is_primitive }}context.GetArgument<{{parameter.type}}>("{{parameter.name}}"){{ end }}
        """;

    private const string FileTemplate =
        """"
        /// <auto-generated/>
        #nullable enable

        {{~ for using in usings ~}}
        {{using}}
        {{~ end ~}}

        {{~ if !string.whitespace namespace ~}}
        namespace {{ namespace }};
        {{~ end ~}}

        public static partial class {{name}}Controller
        {
            {{~ for property in properties ~}}
            {{ include "property_resolver" property name }}
            {{~ if for.last ~}}{{~ else ~}}
        
            {{~ end ~}}
            {{~ end ~}}
            {{~ for method in methods ~}}
            {{~ if properties.size > 0 ~}}
        
            {{~ end ~}}
            {{ include "method_resolver" method name  }}
            {{- if for.last ~}}{{~ else ~}}
        
            {{~ end ~}}
            {{~ end ~}}
        }

        public static class {{name}}ControllerExtensions
        {
            public static SourceGeneratedTypesBuilder Add{{name}}Controller(
                this SourceGeneratedTypesBuilder builder)
            {
                builder.Builder.Configure(options => options.Builder.Add(
                    "{{name}}",
                    new FieldsWithResolvers()
                    {
                    {{~ for property in properties ~}}
                        { "{{ property.as_field }}", {{name}}Controller.{{ property.resolver_name }} }{{ if for.last }}{{ else }},{{ end }}
                    {{~ end ~}}
                    {{~ for method in methods ~}}
                        {{~ if properties.size > 0 ~}},
                        {{~ end ~}}
                        { "{{ method.as_field }}", {{name}}Controller.{{ method.resolver_name }} }{{ if for.last }}{{ else }},{{ end }}
                    {{~ end ~}}
                    }
                    {{- for subMethod in subscribers -}}
                    {{- if for.first -}}
                    , 
                    new FieldsWithSubscribers()
                    {
                    {{~ end ~}} 
                        { "{{ subMethod.as_field }}", {{name}}Controller.{{ subMethod.subscriber_name }} }{{ if for.last }}{{ else }},{{ end }}
                    {{~ if for.last ~}}   
                    }
                    {{~ end ~}} 
                    {{~ end ~}}
                    ));
                    
                {{~ if implements.size > 0 ~}}
                builder.Builder.Configure(options => options.Builder.Add(
                    """
                    extend type {{name}} implements {{ for base in implements ~}}{{base.graph_qlname}}{{~ if for.last ~}}{{~ else }} & {{ end ~}}{{~ end }}
                    """));
                    {{~ end ~}}
                    
                return builder;
            }
        }

        {{ if named_type_extension -}}
        {{ named_type_extension }}
        {{- end }}
        #nullable restore
        """";

    public static readonly IReadOnlyList<string> DefaultUsings =
    [
        "using System;",
        "using System.Threading.Tasks;",
        "using Microsoft.Extensions.Options;",
        "using Tanka.GraphQL.TypeSystem;",
        "using Tanka.GraphQL.Server;",
        "using Tanka.GraphQL.Executable;",
        "using Tanka.GraphQL.ValueResolution;",
        "using Tanka.GraphQL.Fields;"
    ];

    private IEnumerable<string> _usings = [];

    public required IEnumerable<string> Usings
    {
        get => _usings;
        [MemberNotNull(nameof(_usings))]
        set =>
            _usings = DefaultUsings
                .Union(value)
                .Distinct()
                .OrderBy(u => u)
                .ToList();
    }

    public required string Namespace { get; set; } = string.Empty;

    public required string Name { get; set; } = string.Empty;

    public required string? TypeName { get; set; }

    public IReadOnlyList<ObjectPropertyDefinition> AllProperties =>
        Properties.Concat(Implements.SelectMany(i => i.Properties)).ToList();

    public required IEnumerable<ObjectPropertyDefinition> Properties { get; set; } = [];

    public IReadOnlyList<ObjectMethodDefinition> AllMethods =>
        Methods.Concat(Implements.SelectMany(i => i.Methods))
            .OrderBy(m => m.Name)
            .ToList();

    public required IEnumerable<ObjectMethodDefinition> Methods { get; set; } = [];
    
    
    public IEnumerable<ObjectMethodDefinition> Subscribers => Methods.Where(m => m.IsSubscription);

    public required string NamedTypeExtension { get; set; }
    
    public IReadOnlyList<BaseDefinition> Implements { get; set; } = [];

    public string Render()
    {
        Template? template = Template.Parse(FileTemplate);
        var scriptObject = new ScriptObject();
        scriptObject.Import(this);
        var templateContext = new TemplateContext
        {
            TemplateLoader = new InMemoryTemplateLoader(
                new Dictionary<string, string>
                {
                    ["property_resolver"] = PropertyResolverTemplate, 
                    ["method_resolver"] = MethodResolverTemplate,
                    ["parameters"] = ParametersTemplate,
                    ["parameter_usage"] = ParameterUsage
                })
        };
        templateContext.PushGlobal(scriptObject);

        string? content = template.Render(templateContext);
        return content;
    }
}