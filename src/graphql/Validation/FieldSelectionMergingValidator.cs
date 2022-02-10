using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation
{
    public class FieldSelectionMergingValidator
    {
        private readonly IRuleVisitorContext _context;

        public FieldSelectionMergingValidator(IRuleVisitorContext context)
        {
            _context = context;
        }

        public void Validate(SelectionSet selectionSet)
        {
            var comparedFragmentPairs = new PairSet();
            var cachedFieldsAndFragmentNames = new Dictionary<SelectionSet, CachedField>();
            var conflicts = FindConflictsWithinSelectionSet(
                cachedFieldsAndFragmentNames,
                comparedFragmentPairs,
                _context.Tracker.ParentType ?? throw new InvalidOperationException("todo: ParentType is null"),
                selectionSet);

            foreach (var conflict in conflicts)
                _context.Error(
                    ValidationErrorCodes.R532FieldSelectionMerging,
                    FieldsConflictMessage(conflict.Reason.Name, conflict.Reason),
                    conflict.FieldsLeft.Concat(conflict.FieldsRight)
                );
        }

        private void CollectConflictsBetween(
            List<Conflict> conflicts,
            Dictionary<SelectionSet, CachedField> cachedFieldsAndFragmentNames,
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

        private void CollectConflictsBetweenFieldsAndFragment(
            List<Conflict> conflicts,
            Dictionary<SelectionSet, CachedField> cachedFieldsAndFragmentNames,
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
                .FragmentDefinitions
                ?.SingleOrDefault(f => f.FragmentName == fragmentName);

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

        private void CollectConflictsBetweenFragments(
            List<Conflict> conflicts,
            Dictionary<SelectionSet, CachedField> cachedFieldsAndFragmentNames,
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
                ?.FragmentDefinitions
                .ToList();

            var fragment1 = fragments.SingleOrDefault(f => f.FragmentName == fragmentName1);
            var fragment2 = fragments.SingleOrDefault(f => f.FragmentName == fragmentName2);

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

        private void CollectConflictsWithin(
            List<Conflict> conflicts,
            Dictionary<SelectionSet, CachedField> cachedFieldsAndFragmentNames,
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


        private void CollectFieldsAndFragmentNames(
            TypeDefinition parentType,
            SelectionSet selectionSet,
            Dictionary<string, List<FieldDefPair>> nodeAndDefs,
            Dictionary<string, bool> fragments)
        {
            var selections = selectionSet.ToList();
            for (var i = 0; i < selections.Count; i++)
            {
                var selection = selections[i];

                if (selection is FieldSelection field)
                {
                    var fieldName = field.Name;
                    FieldDefinition? fieldDef = null;
                    if (parentType is not null && (IsObjectDefinition(parentType) || IsInterfaceType(parentType)))
                        fieldDef = _context.Schema.GetField( parentType.Name, fieldName);

                    var responseName = field.AliasOrName;

                    if (!nodeAndDefs.ContainsKey(responseName)) nodeAndDefs[responseName] = new List<FieldDefPair>();

                    nodeAndDefs[responseName].Add(new FieldDefPair
                    {
                        ParentType = parentType,
                        Field = field,
                        FieldDef = fieldDef
                    });
                }
                else if (selection is FragmentSpread fragmentSpread)
                {
                    fragments[fragmentSpread.FragmentName] = true;
                }
                else if (selection is InlineFragment inlineFragment)
                {
                    var typeCondition = inlineFragment.TypeCondition;

                    if (typeCondition is not null)
                    {
                        var inlineFragmentType =
                            _context.Schema.GetNamedType(typeCondition.Name) ?? parentType;

                        CollectFieldsAndFragmentNames(
                            inlineFragmentType,
                            inlineFragment.SelectionSet,
                            nodeAndDefs,
                            fragments);
                    }
                }
            }
        }

        private bool DoTypesConflict(TypeBase type1, TypeBase type2)
        {
            if (type1 is ListType type1List)
                return type2 is not ListType type2List || DoTypesConflict(type1List.OfType, type2List.OfType);

            if (type2 is ListType) return true;

            if (type1 is NonNullType type1NonNullType)
                return !(type2 is NonNullType type2NonNullType) ||
                       DoTypesConflict(type1NonNullType.OfType, type2NonNullType.OfType);

            if (type2 is NonNullType) return true;

            var typeDefinition1 = Ast.UnwrapAndResolveType(_context.Schema, type1);
            var typeDefinition2 = Ast.UnwrapAndResolveType(_context.Schema, type2);

            if (typeDefinition1 is ScalarDefinition || typeDefinition2 is ScalarDefinition) return !Equals(typeDefinition1, typeDefinition2);

            if (typeDefinition1 is EnumDefinition || typeDefinition2 is EnumDefinition) return !Equals(typeDefinition1, typeDefinition2);

            return false;
        }

        private static string FieldsConflictMessage(string responseName, ConflictReason reason)
        {
            return $"Fields {responseName} conflicts because {ReasonMessage(reason.Message)}. " +
                   "Use different aliases on the fields to fetch both if this was intentional.";
        }

        private Conflict? FindConflict(
            Dictionary<SelectionSet, CachedField> cachedFieldsAndFragmentNames,
            PairSet comparedFragmentPairs,
            bool parentFieldsAreMutuallyExclusive,
            string responseName,
            FieldDefPair fieldDefPair1,
            FieldDefPair fieldDefPair2)
        {
            var parentType1 = _context.Schema.GetNamedType(fieldDefPair1.ParentType.Name);
            var node1 = fieldDefPair1.Field;
            var def1 = fieldDefPair1.FieldDef;

            var parentType2 = _context.Schema.GetNamedType(fieldDefPair2.ParentType.Name);
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
                parentType1 != parentType2 && IsObjectDefinition(parentType1) && IsObjectDefinition(parentType2);

            // return type for each field.
            var type1 = def1?.Type;
            var type2 = def2?.Type;

            if (!areMutuallyExclusive)
            {
                // Two aliases must refer to the same field.
                var name1 = node1.Name;
                var name2 = node2.Name;

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
                        FieldsLeft = new List<FieldSelection> {node1},
                        FieldsRight = new List<FieldSelection> {node2}
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
                        FieldsLeft = new List<FieldSelection> {node1},
                        FieldsRight = new List<FieldSelection> {node2}
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
                    FieldsLeft = new List<FieldSelection> {node1},
                    FieldsRight = new List<FieldSelection> {node2}
                };

            // Collect and compare sub-fields. Use the same "visited fragment names" list
            // for both collections so fields in a fragment reference are never
            // compared to themselves.
            var selectionSet1 = node1.SelectionSet;
            var selectionSet2 = node2.SelectionSet;

            if (selectionSet1 != null && selectionSet2 != null)
            {
                var conflicts = FindConflictsBetweenSubSelectionSets(
                    cachedFieldsAndFragmentNames,
                    comparedFragmentPairs,
                    areMutuallyExclusive,
                    Ast.UnwrapAndResolveType(_context.Schema, type1),
                    selectionSet1,
                    Ast.UnwrapAndResolveType(_context.Schema, type2),
                    selectionSet2);

                return SubfieldConflicts(conflicts, responseName, node1, node2);
            }

            return null;
        }

        private List<Conflict> FindConflictsBetweenSubSelectionSets(
            Dictionary<SelectionSet, CachedField> cachedFieldsAndFragmentNames,
            PairSet comparedFragmentPairs,
            bool areMutuallyExclusive,
            TypeDefinition parentType1,
            SelectionSet selectionSet1,
            TypeDefinition parentType2,
            SelectionSet selectionSet2)
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

        private List<Conflict> FindConflictsWithinSelectionSet(
            Dictionary<SelectionSet, CachedField> cachedFieldsAndFragmentNames,
            PairSet comparedFragmentPairs,
            TypeDefinition parentType,
            SelectionSet selectionSet)
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

        private CachedField GetFieldsAndFragmentNames(
            Dictionary<SelectionSet, CachedField> cachedFieldsAndFragmentNames,
            TypeDefinition parentType,
            SelectionSet selectionSet)
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
            Dictionary<SelectionSet, CachedField> cachedFieldsAndFragmentNames,
            FragmentDefinition fragment)
        {
            // Short-circuit building a type from the node if possible.
            if (cachedFieldsAndFragmentNames.ContainsKey(fragment.SelectionSet))
                return cachedFieldsAndFragmentNames[fragment.SelectionSet];

            var fragmentType = fragment.TypeCondition;
            return GetFieldsAndFragmentNames(
                cachedFieldsAndFragmentNames,
                _context.Schema.GetNamedType(fragmentType.Name) ?? throw  new InvalidOperationException($"Could not find type '{fragmentType.Name}' from schema."),
                fragment.SelectionSet);
        }

        private bool IsInterfaceType(TypeDefinition? parentType)
        {
            return parentType is InterfaceDefinition;
        }

        private bool IsObjectDefinition(TypeDefinition? parentType)
        {
            return parentType is ObjectDefinition;
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

        private bool SameArguments(
            FieldDefPair fieldDefPair1,
            FieldDefPair fieldDefPair2)
        {
            var arguments1 = fieldDefPair1.Field.Arguments?
                .ToDictionary(l => l.Name, l => l);

            var arguments2 = fieldDefPair2.Field.Arguments?
                .ToDictionary(l => l.Name, l => l);

            if (arguments1 == null && arguments2 == null)
                return true;

            if (arguments1 == null)
                return false;

            if (arguments2 == null)
                return false;

            if (arguments1.Count() != arguments2.Count())
                return false;

            return arguments1.All(arg1 =>
            {
                if (!arguments2.ContainsKey(arg1.Key))
                    return false;

                var arg2 = arguments2[arg1.Key];

                if (fieldDefPair1.FieldDef?.TryGetArgument(arg1.Key, out var argDef1) == true && fieldDefPair2.FieldDef?.TryGetArgument(arg1.Key, out var argDef2) == true)
                {
                    var value1 = ArgumentCoercion.CoerceArgumentValue(
                        _context.Schema,
                        _context.VariableValues,
                        arg1.Key,
                        argDef1,
                        arg1.Value);

                    var value2 = ArgumentCoercion.CoerceArgumentValue(
                        _context.Schema,
                        _context.VariableValues,
                        arg1.Key,
                        argDef2,
                        arg2);

                    return SameValue(value1, value2);
                }

                return false;
                });
        }

        private bool SameValue(object arg1, object arg2)
        {
            if (arg1 == null && arg2 == null)
                return true;

            return Equals(arg1, arg2);
        }

        // Given a series of Conflicts which occurred between two sub-fields,
        // generate a single Conflict.
        private Conflict SubfieldConflicts(
            List<Conflict> conflicts,
            string responseName,
            FieldSelection node1,
            FieldSelection node2)
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
                    FieldsLeft = conflicts.Aggregate(new List<FieldSelection> {node1}, (allfields, conflict) =>
                    {
                        allfields.AddRange(conflict.FieldsLeft);
                        return allfields;
                    }),
                    FieldsRight = conflicts.Aggregate(new List<FieldSelection> {node2}, (allfields, conflict) =>
                    {
                        allfields.AddRange(conflict.FieldsRight);
                        return allfields;
                    })
                };

            return null;
        }

        private class CachedField
        {
            public List<string> Names { get; set; }
            public Dictionary<string, List<FieldDefPair>> NodeAndDef { get; set; }
        }

        private class Conflict
        {
            public List<FieldSelection> FieldsLeft { get; set; }

            public List<FieldSelection> FieldsRight { get; set; }
            public ConflictReason Reason { get; set; }
        }

        private class ConflictReason
        {
            public Message Message { get; set; }
            public string Name { get; set; }
        }

        private class FieldDefPair
        {
            public FieldSelection Field { get; set; }

            public FieldDefinition? FieldDef { get; set; }
            public TypeDefinition? ParentType { get; set; }
        }

        private class Message
        {
            public string Msg { get; set; }
            public List<ConflictReason> Msgs { get; set; }
        }

        private class ObjMap<T> : Dictionary<string, T>
        {
        }

        private class PairSet
        {
            private readonly ObjMap<ObjMap<bool>> _data;

            public PairSet()
            {
                _data = new ObjMap<ObjMap<bool>>();
            }

            public void Add(string a, string b, bool areMutuallyExclusive)
            {
                PairSetAdd(a, b, areMutuallyExclusive);
                PairSetAdd(b, a, areMutuallyExclusive);
            }

            public bool Has(string a, string b, bool areMutuallyExclusive)
            {
                _data.TryGetValue(a, out var first);

                if (first == null || !first.ContainsKey(b)) return false;

                var result = first[b];

                if (areMutuallyExclusive == false) return result == false;

                return true;
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
    }
}