# PLANNING.md

## Docs revision

### Structure Assessment
The documentation is well-organized with a logical flow:
- **0-getting-started/** - Clear onboarding path
- **2-language/** - Parser and document handling
- **3-server/** - HTTP/WebSocket implementation
- **4-code-generator/** - Source generator examples
- **6-executor/** - Core execution pipeline
- **7-type-system/** - Type system and schema building
- **10-extensions/** - Apollo Federation and tracing

### Content Issues Identified

1. **Typo in nav.md:6** - "Depedency Injection" should be "Dependency Injection"

2. **Package naming inconsistency**:
   - docs/3-server/00-index.md:11 uses `tanka.graphql.server` (lowercase)
   - Should be `Tanka.GraphQL.Server` per package conventions

3. **Missing navigation sections**:
   - No section 1, 5, 8, 9 in main structure
   - Gap between language (2) and server (3)
   - Missing sections could cover validation, middleware, error handling

4. **Apollo Federation demo links**:
   - References outdated fork URLs that may not be maintained
   - Demo instructions reference specific git branches that may not exist

5. **Code includes**:
   - Heavy reliance on `#include::xref://` for code samples
   - No inline examples make docs hard to read standalone

### Recommendations

**Immediate fixes needed:**
- Fix "Depedency" typo in docs/0-getting-started/nav.md:6
- Standardize package name casing to `Tanka.GraphQL.Server`

**Structure improvements:**
- Consider adding missing numbered sections (1, 5, 8, 9) or renumber existing ones
- Add validation/middleware/error handling documentation sections

**Content enhancements:**
- Include inline code examples alongside xref includes
- Update Apollo Federation demo links and verify they work
- Add troubleshooting/FAQ section

The documentation provides good coverage of core functionality but could benefit from better organization and some content updates.