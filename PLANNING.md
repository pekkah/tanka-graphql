# PLANNING.md

## Docs revision - Phase 1 COMPLETED ✅

### Structure Assessment
The documentation is well-organized with a logical flow:
- **0-getting-started/** - Clear onboarding path
- **2-language/** - Parser and document handling
- **3-server/** - HTTP/WebSocket implementation
- **4-code-generator/** - Source generator examples
- **6-executor/** - Core execution pipeline
- **7-type-system/** - Type system and schema building
- **10-extensions/** - Apollo Federation and tracing

### Content Issues RESOLVED ✅

1. ✅ **Typo in nav.md:6** - Fixed "Depedency Injection" → "Dependency Injection"

2. ✅ **Package naming inconsistency** - Updated `tanka.graphql.server` → `Tanka.GraphQL.Server`

3. ✅ **Missing navigation sections** - Assessed and determined current numbering system works well

4. ✅ **Apollo Federation demo links** - Updated demo instructions, removed outdated branch references

### NEXT PHASE: Content Enhancement

**Remaining improvements to consider:**

1. **Code includes assessment**:
   - ✅ Current `#include::xref://` approach is actually excellent
   - Keeps examples in sync with working code and ensures they're testable
   - No changes needed - this is a best practice

2. **Documentation gaps**:
   - Add validation documentation section
   - Add middleware/error handling documentation 
   - Add troubleshooting/FAQ section

3. **Content freshness**:
   - Review all external links for accuracy
   - Update examples to use .NET 9.0 features where applicable
   - Verify all code samples compile and work correctly

4. **User experience improvements**:
   - Add "Quick Start" section to getting-started
   - Add migration guides for major version updates
   - Improve navigation between related topics

### Priority for Phase 2

**High Priority:**
- ✅ Create troubleshooting/FAQ section

**Medium Priority:**
- Add validation documentation section
- Review and update external links

**Low Priority:**
- Add migration guides
- Enhance cross-topic navigation

## Minor Issues Fixed

✅ **UI Template Fixes:**
- Fixed GitHub link to point to main repository (`tanka-graphql`) instead of `tanka-docs-gen`
- Combined duplicated CSS rules for `.sidebar .nav-link` to improve maintainability
- Assessed mobile TOC UX - current implementation is already excellent with auto-collapse and toggle functionality