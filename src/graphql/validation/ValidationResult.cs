using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tanka.graphql.validation
{
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();

        public IEnumerable<ValidationError> Errors { get; set; } = new ValidationError[0];

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