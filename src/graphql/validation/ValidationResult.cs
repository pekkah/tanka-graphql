using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tanka.GraphQL.Validation
{
    public class ValidationResult
    {
        public static ValidationResult Success => new ValidationResult();

        public bool IsValid => !Errors.Any();

        public IEnumerable<ValidationError> Errors { get; set; } = new ValidationError[0];

        public IReadOnlyDictionary<string, object> Extensions { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"IsValid: {IsValid}");

            if (!IsValid)
            {
                builder.AppendLine("ExecutionErrors:");
                foreach (var validationError in Errors)
                {
                    builder.AppendLine(validationError.ToString());
                }
            }


            return builder.ToString();
        }
    }
}