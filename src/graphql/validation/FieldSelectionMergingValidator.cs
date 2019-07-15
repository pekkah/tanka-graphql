using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.execution;
using tanka.graphql.language;
using tanka.graphql.type;
using tanka.graphql.type.converters;

namespace tanka.graphql.validation
{
    public class FieldSelectionMergingValidator
    {
        private readonly IRuleVisitorContext _context;

        public FieldSelectionMergingValidator(IRuleVisitorContext context)
        {
            _context = context;
        }

        public void Validate(GraphQLSelectionSet selectionSet)
        {
            var comparedFragmentPairs = new PairSet();
            var cachedFieldsAndFragmentNames = new Dictionary<GraphQLSelectionSet, CachedField>();
            var conflicts = FindConflictsWithinGraphQLSelectionSet(
                cachedFieldsAndFragmentNames,
                comparedFragmentPairs,
                _context.Tracker.GetParentType(),
                selectionSet);

            foreach (var conflict in conflicts)
                _context.Error(
                    ValidationErrorCodes.R532FieldSelectionMerging,
                    FieldsConflictMessage(conflict.Reason.Name, conflict.Reason),
                    conflict.FieldsLeft.Concat(conflict.FieldsRight)
                );
        }

        private static string FieldsConflictMessage(string responseName, ConflictReason reason)
        {
            return $"Fields {responseName} conflicts because {ReasonMessage(reason.Message)}. " +
                   "Use different aliases on the fields to fetch both if this was intentional.";
        }

        private static string ReasonMessage(Message reasonMessage)
        {
            if (reasonMessage.Msgs?.Count > 0)
                return string.Join(
                    " and ",
                    reasonMessage.Msgs.Select(x =>
                    {
                        return $"subfields \"{x.Name}\" conflict because {ReasonMessage(x.Message)}";
                    }).ToArray()
                );
            return reasonMessage.Msg;
        }

        private List<Conflict> FindConflictsWithinGraphQLSelectionSet(
            Dictionary<GraphQLSelectionSet, CachedField> cachedFieldsAndFragmentNames,
            PairSet comparedFragmentPairs,
            INamedType parentType,
            GraphQLSelectionSet selectionSet)
        {
            var conflicts = new List<Conflict>();

            var cachedField = GetFieldsAndFragmentNames(
                cachedFieldsAndFragmentNames,
                parentType,
                selectionSet);

            var fieldMap = cachedField.NodeAndDef;
            var fragmentNames = cachedField.Names;

            CollectConflictsWithin(
                conflicts,
                cachedFieldsAndFragmentNames,
                comparedFragmentPairs,
                fieldMap);

            if (fragmentNames.Count != 0)
            {
                // (B) Then collect conflicts between these fields and those represented by
                // each spread fragment name found.
                var comparedFragments = new ObjMap<bool>();
                for (var i = 0; i < fragmentNames.Count; i++)
                {
                    CollectConflictsBetweenFieldsAndFragment(
                        conflicts,
                        cachedFieldsAndFragmentNames,
                        comparedFragments,
                        comparedFragmentPairs,
                        false,
                        fieldMap,
                        fragmentNames[i]);

                    // (C) Then compare this fragment with all other fragments found in this
                    // selection set to collect conflicts between fragments spread together.
                    // This compares each item in the list of fragment names to every other
                    // item in that same list (except for itself).
                    for (var j = i + 1; j < fragmentNames.Count; j++)
                        CollectConflictsBetweenFragments(
                            conflicts,
                            cachedFieldsAndFragmentNames,
                            comparedFragmentPairs,
                            false,
                            fragmentNames[i],
                            fragmentNames[j]);
                }
            }

            return conflicts;
        }

        private void CollectConflictsWithin(
            List<Conflict> conflicts,
            Dictionary<GraphQLSelectionSet, CachedField> cachedFieldsAndFragmentNames,
            PairSet comparedFragmentPairs,
            Dictionary<string, List<FieldDefPair>> fieldMap)
        {
            // A field map is a keyed collection, where each key represents a response
            // name and the value at that key is a list of all fields which provide that
            // response name. For every response name, if there are multiple fields, they
            // must be compared to find a potential conflict.
            foreach (var entry in fieldMap)
            {
                var responseName = entry.Key;
                var fields = entry.Value;

                // This compares every field in the list to every other field in this list
                // (except to itself). If the list only has one item, nothing needs to
                // be compared.
                if (fields.Count > 1)
                    for (var i = 0; i < fields.Count; i++)
                    for (var j = i + 1; j < fields.Count; j++)
                    {
                        var conflict = FindConflict(
                            cachedFieldsAndFragmentNames,
                            comparedFragmentPairs,
                            false, // within one collection is never mutually exclusive
                            responseName,
                            fields[i],
                            fields[j]);

                        if (conflict != null) conflicts.Add(conflict);
                    }
            }
        }

        private Conflict FindConflict(
            Dictionary<GraphQLSelectionSet, CachedField> cachedFieldsAndFragmentNames,
            PairSet comparedFragmentPairs,
            bool parentFieldsAreMutuallyExclusive,
            string responseName,
            FieldDefPair fieldDefPair1,
            FieldDefPair fieldDefPair2)
        {
            var parentType1 = fieldDefPair1.ParentType;
            var node1 = fieldDefPair1.Field;
            var def1 = fieldDefPair1.FieldDef;

            var parentType2 = fieldDefPair2.ParentType;
            var node2 = fieldDefPair2.Field;
            var def2 = fieldDefPair2.FieldDef;

            // If it is known that two fields could not possibly apply at the same
            // time, due to the parent types, then it is safe to permit them to diverge
            // in aliased field or arguments used as they will not present any ambiguity
            // by differing.
            // It is known that two parent types could never overlap if they are
            // different Object types. Interface or Union types might overlap - if not
            // in the current state of the schema, then perhaps in some future version,
            // thus may not safely diverge.

            var areMutuallyExclusive =
                parentFieldsAreMutuallyExclusive ||
                parentType1 != parentType2 && isObjectType(parentType1) && isObjectType(parentType2);

            // return type for each field.
            var type1 = def1?.Type;
            var type2 = def2?.Type;

            if (!areMutuallyExclusive)
            {
                // Two aliases must refer to the same field.
                var name1 = node1.Name.Value;
                var name2 = node2.Name.Value;

                if (name1 != name2)
                    return new Conflict
                    {
                        Reason = new ConflictReason
                        {
                            Name = responseName,
                            Message = new Message
                            {
                                Msg = $"{name1} and {name2} are different fields"
                            }
                        },
                        FieldsLeft = new List<GraphQLFieldSelection> {node1},
                        FieldsRight = new List<GraphQLFieldSelection> {node2}
                    };

                // Two field calls must have the same arguments.
                if (!SameArguments(fieldDefPair1, fieldDefPair2))
                    return new Conflict
                    {
                        Reason = new ConflictReason
                        {
                            Name = responseName,
                            Message = new Message
                            {
                                Msg = "they have differing arguments"
                            }
                        },
                        FieldsLeft = new List<GraphQLFieldSelection> {node1},
                        FieldsRight = new List<GraphQLFieldSelection> {node2}
                    };
            }

            if (type1 != null && type2 != null && DoTypesConflict(type1, type2))
                return new Conflict
                {
                    Reason = new ConflictReason
                    {
                        Name = responseName,
                        Message = new Message
                        {
                            Msg = $"they return conflicting types {type1} and {type2}"
                        }
                    },
                    FieldsLeft = new List<GraphQLFieldSelection> {node1},
                    FieldsRight = new List<GraphQLFieldSelection> {node2}
                };

            // Collect and compare sub-fields. Use the same "visited fragment names" list
            // for both collections so fields in a fragment reference are never
            // compared to themselves.
            var graphQLSelectionSet1 = node1.SelectionSet;
            var graphQLSelectionSet2 = node2.SelectionSet;

            if (graphQLSelectionSet1 != null && graphQLSelectionSet2 != null)
            {
                var conflicts = FindConflictsBetweenSubGraphQLSelectionSets(
                    cachedFieldsAndFragmentNames,
                    comparedFragmentPairs,
                    areMutuallyExclusive,
                    type1.Unwrap(),
                    graphQLSelectionSet1,
                    type2.Unwrap(),
                    graphQLSelectionSet2);

                return SubfieldConflicts(conflicts, responseName, node1, node2);
            }

            return null;
        }

        private List<Conflict> FindConflictsBetweenSubGraphQLSelectionSets(
            Dictionary<GraphQLSelectionSet, CachedField> cachedFieldsAndFragmentNames,
            PairSet comparedFragmentPairs,
            bool areMutuallyExclusive,
            IType parentType1,
            GraphQLSelectionSet selectionSet1,
            IType parentType2,
            GraphQLSelectionSet selectionSet2)
        {
            var conflicts = new List<Conflict>();

            var cachedField1 = GetFieldsAndFragmentNames(
                cachedFieldsAndFragmentNames,
                parentType1,
                selectionSet1);

            var fieldMap1 = cachedField1.NodeAndDef;
            var fragmentNames1 = cachedField1.Names;

            var cachedField2 = GetFieldsAndFragmentNames(
                cachedFieldsAndFragmentNames,
                parentType2,
                selectionSet2);

            var fieldMap2 = cachedField2.NodeAndDef;
            var fragmentNames2 = cachedField2.Names;

            // (H) First, collect all conflicts between these two collections of field.
            CollectConflictsBetween(
                conflicts,
                cachedFieldsAndFragmentNames,
                comparedFragmentPairs,
                areMutuallyExclusive,
                fieldMap1,
                fieldMap2);

            // (I) Then collect conflicts between the first collection of fields and
            // those referenced by each fragment name associated with the second.
            if (fragmentNames2.Count != 0)
            {
                var comparedFragments = new ObjMap<bool>();

                for (var j = 0; j < fragmentNames2.Count; j++)
                    CollectConflictsBetweenFieldsAndFragment(
                        conflicts,
                        cachedFieldsAndFragmentNames,
                        comparedFragments,
                        comparedFragmentPairs,
                        areMutuallyExclusive,
                        fieldMap1,
                        fragmentNames2[j]);
            }

            // (I) Then collect conflicts between the second collection of fields and
            // those referenced by each fragment name associated with the first.
            if (fragmentNames1.Count != 0)
            {
                var comparedFragments = new ObjMap<bool>();

                for (var i = 0; i < fragmentNames1.Count; i++)
                    CollectConflictsBetweenFieldsAndFragment(
                        conflicts,
                        cachedFieldsAndFragmentNames,
                        comparedFragments,
                        comparedFragmentPairs,
                        areMutuallyExclusive,
                        fieldMap2,
                        fragmentNames1[i]);
            }

            // (J) Also collect conflicts between any fragment names by the first and
            // fragment names by the second. This compares each item in the first set of
            // names to each item in the second set of names.
            for (var i = 0; i < fragmentNames1.Count; i++)
            for (var j = 0; j < fragmentNames2.Count; j++)
                CollectConflictsBetweenFragments(
                    conflicts,
                    cachedFieldsAndFragmentNames,
                    comparedFragmentPairs,
                    areMutuallyExclusive,
                    fragmentNames1[i],
                    fragmentNames2[j]);

            return conflicts;
        }

        private void CollectConflictsBetweenFragments(
            List<Conflict> conflicts,
            Dictionary<GraphQLSelectionSet, CachedField> cachedFieldsAndFragmentNames,
            PairSet comparedFragmentPairs,
            bool areMutuallyExclusive,
            string fragmentName1,
            string fragmentName2)
        {
            // No need to compare a fragment to itself.
            if (fragmentName1 == fragmentName2) return;

            // Memoize so two fragments are not compared for conflicts more than once.
            if (comparedFragmentPairs.Has(fragmentName1, fragmentName2, areMutuallyExclusive)) return;

            comparedFragmentPairs.Add(fragmentName1, fragmentName2, areMutuallyExclusive);

            var fragments = _context.Document
                .Definitions
                .OfType<GraphQLFragmentDefinition>();
            var fragment1 = fragments.SingleOrDefault(f => f.Name.Value == fragmentName1);
            var fragment2 = fragments.SingleOrDefault(f => f.Name.Value == fragmentName2);

            if (fragment1 == null || fragment2 == null) return;

            var cachedField1 =
                GetReferencedFieldsAndFragmentNames(
                    cachedFieldsAndFragmentNames,
                    fragment1);

            var fieldMap1 = cachedField1.NodeAndDef;
            var fragmentNames1 = cachedField1.Names;

            var cachedField2 =
                GetReferencedFieldsAndFragmentNames(
                    cachedFieldsAndFragmentNames,
                    fragment2);

            var fieldMap2 = cachedField2.NodeAndDef;
            var fragmentNames2 = cachedField2.Names;

            // (F) First, collect all conflicts between these two collections of fields
            // (not including any nested fragments).
            CollectConflictsBetween(
                conflicts,
                cachedFieldsAndFragmentNames,
                comparedFragmentPairs,
                areMutuallyExclusive,
                fieldMap1,
                fieldMap2);

            // (G) Then collect conflicts between the first fragment and any nested
            // fragments spread in the second fragment.
            for (var j = 0; j < fragmentNames2.Count; j++)
                CollectConflictsBetweenFragments(
                    conflicts,
                    cachedFieldsAndFragmentNames,
                    comparedFragmentPairs,
                    areMutuallyExclusive,
                    fragmentName1,
                    fragmentNames2[j]);

            // (G) Then collect conflicts between the second fragment and any nested
            // fragments spread in the first fragment.
            for (var i = 0; i < fragmentNames1.Count; i++)
                CollectConflictsBetweenFragments(
                    conflicts,
                    cachedFieldsAndFragmentNames,
                    comparedFragmentPairs,
                    areMutuallyExclusive,
                    fragmentNames1[i],
                    fragmentName2);
        }

        private void CollectConflictsBetweenFieldsAndFragment(
            List<Conflict> conflicts,
            Dictionary<GraphQLSelectionSet, CachedField> cachedFieldsAndFragmentNames,
            ObjMap<bool> comparedFragments,
            PairSet comparedFragmentPairs,
            bool areMutuallyExclusive,
            Dictionary<string, List<FieldDefPair>> fieldMap,
            string fragmentName)
        {
            // Memoize so a fragment is not compared for conflicts more than once.
            if (comparedFragments.ContainsKey(fragmentName)) return;

            comparedFragments[fragmentName] = true;

            var fragment = _context.Document
                .Definitions
                .OfType<GraphQLFragmentDefinition>()
                .SingleOrDefault(f => f.Name.Value == fragmentName);

            if (fragment == null) return;

            var cachedField =
                GetReferencedFieldsAndFragmentNames(
                    cachedFieldsAndFragmentNames,
                    fragment);

            var fieldMap2 = cachedField.NodeAndDef;
            var fragmentNames2 = cachedField.Names;

            // Do not compare a fragment's fieldMap to itself.
            if (fieldMap == fieldMap2) return;

            // (D) First collect any conflicts between the provided collection of fields
            // and the collection of fields represented by the given fragment.
            CollectConflictsBetween(
                conflicts,
                cachedFieldsAndFragmentNames,
                comparedFragmentPairs,
                areMutuallyExclusive,
                fieldMap,
                fieldMap2);

            // (E) Then collect any conflicts between the provided collection of fields
            // and any fragment names found in the given fragment.
            for (var i = 0; i < fragmentNames2.Count; i++)
                CollectConflictsBetweenFieldsAndFragment(
                    conflicts,
                    cachedFieldsAndFragmentNames,
                    comparedFragments,
                    comparedFragmentPairs,
                    areMutuallyExclusive,
                    fieldMap,
                    fragmentNames2[i]);
        }

        private void CollectConflictsBetween(
            List<Conflict> conflicts,
            Dictionary<GraphQLSelectionSet, CachedField> cachedFieldsAndFragmentNames,
            PairSet comparedFragmentPairs,
            bool parentFieldsAreMutuallyExclusive,
            Dictionary<string, List<FieldDefPair>> fieldMap1,
            Dictionary<string, List<FieldDefPair>> fieldMap2)
        {
            // A field map is a keyed collection, where each key represents a response
            // name and the value at that key is a list of all fields which provide that
            // response name. For any response name which appears in both provided field
            // maps, each field from the first field map must be compared to every field
            // in the second field map to find potential conflicts.

            foreach (var responseName in fieldMap1.Keys)
            {
                fieldMap2.TryGetValue(responseName, out var fields2);

                if (fields2 != null && fields2.Count != 0)
                {
                    var fields1 = fieldMap1[responseName];
                    for (var i = 0; i < fields1.Count; i++)
                    for (var j = 0; j < fields2.Count; j++)
                    {
                        var conflict = FindConflict(
                            cachedFieldsAndFragmentNames,
                            comparedFragmentPairs,
                            parentFieldsAreMutuallyExclusive,
                            responseName,
                            fields1[i],
                            fields2[j]);

                        if (conflict != null) conflicts.Add(conflict);
                    }
                }
            }
        }

        private bool DoTypesConflict(IType type1, IType type2)
        {
            if (type1 is List type1List)
                return !(type2 is List type2List) || DoTypesConflict(type1List.OfType, type2List.OfType);

            if (type2 is List) return true;

            if (type1 is NonNull type1NonNull)
                return !(type2 is NonNull type2NonNull) ||
                       DoTypesConflict(type1NonNull.OfType, type2NonNull.OfType);

            if (type2 is NonNull) return true;

            if (type1 is IValueConverter || type2 is IValueConverter) return !Equals(type1, type2);

            return false;
        }

        private bool SameArguments(
            FieldDefPair fieldDefPair1,
            FieldDefPair fieldDefPair2)
        {
            var arguments1 = fieldDefPair1.Field.Arguments
                .ToDictionary(l => l.Name.Value, l => l);

            var arguments2 = fieldDefPair2.Field.Arguments
                .ToDictionary(l => l.Name.Value, l => l);

            if (arguments1.Count() != arguments2.Count()) 
                return false;

            return arguments1.All(arg1 =>
            {
                if (!arguments2.ContainsKey(arg1.Key))
                    return false;

                var arg2 = arguments2[arg1.Key];

                var value1 = Arguments.CoerceArgumentValue(
                    _context.Schema,
                    _context.VariableValues,
                    arg1.Key,
                    fieldDefPair1.FieldDef.GetArgument(arg1.Key),
                    arg1.Value);

                var value2 = Arguments.CoerceArgumentValue(
                    _context.Schema,
                    _context.VariableValues,
                    arg1.Key,
                    fieldDefPair2.FieldDef.GetArgument(arg1.Key),
                    arg2);

                return SameValue(value1, value2);
            });
        }

        private bool SameValue(object arg1, object arg2)
        {
            if (arg1 == null && arg2 == null)
                return true;

            return Equals(arg1, arg2);
        }

        private CachedField GetFieldsAndFragmentNames(
            Dictionary<GraphQLSelectionSet, CachedField> cachedFieldsAndFragmentNames,
            IType parentType,
            GraphQLSelectionSet selectionSet)
        {
            cachedFieldsAndFragmentNames.TryGetValue(selectionSet,
                out var cached);

            if (cached == null)
            {
                var nodeAndDef = new Dictionary<string, List<FieldDefPair>>();
                var fragmentNames = new Dictionary<string, bool>();

                CollectFieldsAndFragmentNames(
                    parentType,
                    selectionSet,
                    nodeAndDef,
                    fragmentNames);

                cached = new CachedField {NodeAndDef = nodeAndDef, Names = fragmentNames.Keys.ToList()};
                cachedFieldsAndFragmentNames.Add(selectionSet, cached);
            }

            return cached;
        }

        // Given a reference to a fragment, return the represented collection of fields
        // as well as a list of nested fragment names referenced via fragment spreads.
        private CachedField GetReferencedFieldsAndFragmentNames(
            Dictionary<GraphQLSelectionSet, CachedField> cachedFieldsAndFragmentNames,
            GraphQLFragmentDefinition fragment)
        {
            // Short-circuit building a type from the node if possible.
            if (cachedFieldsAndFragmentNames.ContainsKey(fragment.SelectionSet))
                return cachedFieldsAndFragmentNames[fragment.SelectionSet];

            var fragmentType = Ast.TypeFromAst(_context.Schema, fragment.TypeCondition);
            return GetFieldsAndFragmentNames(
                cachedFieldsAndFragmentNames,
                fragmentType,
                fragment.SelectionSet);
        }


        private void CollectFieldsAndFragmentNames(
            IType parentType,
            GraphQLSelectionSet selectionSet,
            Dictionary<string, List<FieldDefPair>> nodeAndDefs,
            Dictionary<string, bool> fragments)
        {
            var selections = selectionSet.Selections.ToArray();
            for (var i = 0; i < selections.Length; i++)
            {
                var selection = selections[i];

                if (selection is GraphQLFieldSelection field)
                {
                    var fieldName = field.Name.Value;
                    IField fieldDef = null;
                    if (isObjectType(parentType) || isInterfaceType(parentType))
                        fieldDef = _context.Schema.GetField(
                            ((INamedType) parentType).Name,
                            fieldName);

                    var responseName = !string.IsNullOrWhiteSpace(field.Alias?.Value) ? field.Alias.Value : fieldName;

                    if (!nodeAndDefs.ContainsKey(responseName)) nodeAndDefs[responseName] = new List<FieldDefPair>();

                    nodeAndDefs[responseName].Add(new FieldDefPair
                    {
                        ParentType = parentType,
                        Field = field,
                        FieldDef = fieldDef
                    });
                }
                else if (selection is GraphQLFragmentSpread fragmentSpread)
                {
                    fragments[fragmentSpread.Name.Value] = true;
                }
                else if (selection is GraphQLInlineFragment inlineFragment)
                {
                    var typeCondition = inlineFragment.TypeCondition;
                    var inlineFragmentType =
                        typeCondition != null
                            ? Ast.TypeFromAst(_context.Schema, typeCondition)
                            : parentType;

                    CollectFieldsAndFragmentNames(
                        inlineFragmentType,
                        inlineFragment.SelectionSet,
                        nodeAndDefs,
                        fragments);
                }
            }
        }

        private bool isInterfaceType(IType parentType)
        {
            return parentType is InterfaceType;
        }

        private bool isObjectType(IType parentType)
        {
            return parentType is ObjectType;
        }

        // Given a series of Conflicts which occurred between two sub-fields,
        // generate a single Conflict.
        private Conflict SubfieldConflicts(
            List<Conflict> conflicts,
            string responseName,
            GraphQLFieldSelection node1,
            GraphQLFieldSelection node2)
        {
            if (conflicts.Count > 0)
                return new Conflict
                {
                    Reason = new ConflictReason
                    {
                        Name = responseName,
                        Message = new Message
                        {
                            Msgs = conflicts.Select(c => c.Reason).ToList()
                        }
                    },
                    FieldsLeft = conflicts.Aggregate(new List<GraphQLFieldSelection> {node1}, (allfields, conflict) =>
                    {
                        allfields.AddRange(conflict.FieldsLeft);
                        return allfields;
                    }),
                    FieldsRight = conflicts.Aggregate(new List<GraphQLFieldSelection> {node2}, (allfields, conflict) =>
                    {
                        allfields.AddRange(conflict.FieldsRight);
                        return allfields;
                    })
                };

            return null;
        }

        private class FieldDefPair
        {
            public IType ParentType { get; set; }

            public GraphQLFieldSelection Field { get; set; }

            public IField FieldDef { get; set; }
        }

        private class Conflict
        {
            public ConflictReason Reason { get; set; }

            public List<GraphQLFieldSelection> FieldsLeft { get; set; }

            public List<GraphQLFieldSelection> FieldsRight { get; set; }
        }

        private class ConflictReason
        {
            public string Name { get; set; }
            public Message Message { get; set; }
        }

        private class Message
        {
            public string Msg { get; set; }
            public List<ConflictReason> Msgs { get; set; }
        }

        private class CachedField
        {
            public Dictionary<string, List<FieldDefPair>> NodeAndDef { get; set; }
            public List<string> Names { get; set; }
        }

        private class PairSet
        {
            private readonly ObjMap<ObjMap<bool>> _data;

            public PairSet()
            {
                _data = new ObjMap<ObjMap<bool>>();
            }

            public bool Has(string a, string b, bool areMutuallyExclusive)
            {
                _data.TryGetValue(a, out var first);

                if (first == null || !first.ContainsKey(b)) return false;

                var result = first[b];

                if (areMutuallyExclusive == false) return result == false;

                return true;
            }

            public void Add(string a, string b, bool areMutuallyExclusive)
            {
                PairSetAdd(a, b, areMutuallyExclusive);
                PairSetAdd(b, a, areMutuallyExclusive);
            }

            private void PairSetAdd(string a, string b, bool areMutuallyExclusive)
            {
                _data.TryGetValue(a, out var map);

                if (map == null)
                {
                    map = new ObjMap<bool>();
                    _data[a] = map;
                }

                map[b] = areMutuallyExclusive;
            }
        }

        private class ObjMap<T> : Dictionary<string, T>
        {
        }
    }
}