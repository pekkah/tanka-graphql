using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public static class InterfaceTypeExtensions
    {
        public static InterfaceType WithEachField(this InterfaceType interfaceType,
            Func<KeyValuePair<string, IField>, KeyValuePair<string, IField>> withField)
        {
            var deletedFields = new List<KeyValuePair<string, IField>>();
            var addedFields = new List<KeyValuePair<string, IField>>();
            foreach (var field in interfaceType.Fields)
            {
                var maybeNewField = withField(field);

                if (Equals(maybeNewField, field))
                    continue;

                if (maybeNewField.Equals(default))
                {
                    deletedFields.Add(field);
                    continue;
                }

                addedFields.Add(maybeNewField);
                deletedFields.Add(field);
            }

            return interfaceType
                .ExcludeFields(deletedFields.ToArray())
                .IncludeFields(addedFields.ToArray());
        }

        public static InterfaceType ExcludeFields(
            this InterfaceType interfaceType,
            params KeyValuePair<string, IField>[] excludedFields)
        {
            if (!excludedFields.Any()) return interfaceType;

            return interfaceType.WithFields(
                interfaceType
                    .Fields
                    .Where(field => !excludedFields.Contains(field))
                    .ToArray()
            );
        }

        public static InterfaceType WithFields(
            this InterfaceType interfaceType,
            params KeyValuePair<string, IField>[] fields)
        {
            return new InterfaceType(
                interfaceType.Name,
                new Fields(fields),
                interfaceType.Meta
            );
        }

        public static InterfaceType IncludeFields(
            this InterfaceType interfaceType,
            params KeyValuePair<string, IField>[] includedFields)
        {
            if (!includedFields.Any())
                return interfaceType;

            return new InterfaceType(
                interfaceType.Name,
                new Fields(interfaceType.Fields.Concat(includedFields)),
                interfaceType.Meta
            );
        }
    }
}