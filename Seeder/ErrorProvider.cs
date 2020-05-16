using System;
using System.Collections.Generic;

namespace Seeder
{
    public class ErrorProvider
    {
        private readonly List<string> _errors;
        private readonly bool _throwOnError;

        public IReadOnlyList<string> Errors => _errors.AsReadOnly();

        public ErrorProvider(bool throwOnError)
        {
            _errors = new List<string>();
            _throwOnError = throwOnError;
        }

        public void AddError(string error)
        {
            _errors.Add(error);

            if (_throwOnError)
                throw new Exception(error);
        }
    }
}
