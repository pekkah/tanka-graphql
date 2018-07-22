using System.Collections.Generic;
using System.Linq;

namespace fugu.graphql.validation
{
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();

        public IEnumerable<ValidationError> Errors { get; set; } = new ValidationError[0];
    }
}