using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProfileService.Domain.ValueObjects
{
    public class Email : IEquatable<Email>
    {
        private static readonly Regex Rx = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        public string Value { get; }

        private Email(string value) => Value = value;

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Rx.IsMatch(value))
                throw new ArgumentException("Invalid email.", nameof(value));
            return new Email(value.Trim());
        }

        public override string ToString() => Value;
        public bool Equals(Email? other) => other is not null &&
            Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object? obj) => obj is Email e && Equals(e);
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }
}
