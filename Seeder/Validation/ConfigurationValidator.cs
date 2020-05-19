using System;
using System.Collections.Generic;
using Seeder.Configuration;

namespace Seeder.Validation
{
    public class ConfigurationValidator
    {
        private readonly DatabaseConfiguration _configuration;

        public ErrorProvider ErrorProvider { get; }

        public ConfigurationValidator(DatabaseConfiguration configuration, bool throwOnError = true)
        {
            _configuration = configuration;
            ErrorProvider = new ErrorProvider(throwOnError);
        }

        public bool Validate()
        {
            if (_configuration == null)
            {
                ErrorProvider.AddError($"{nameof(_configuration)} can't be null!");
                return false;
            }

            if (_configuration.Tables == null || _configuration.Tables.Count <= 0)
            {
                ErrorProvider.AddError($"{nameof(_configuration.Tables)} can't be null or empty!");
                return false;
            }

            foreach (var table in _configuration.Tables)
            {
                if (string.IsNullOrEmpty(table.SchemaName))
                {
                    ErrorProvider.AddError($"{nameof(table.SchemaName)} can't be null or empty!");
                    return false;
                }

                if (string.IsNullOrEmpty(table.TableName))
                {
                    ErrorProvider.AddError($"{nameof(table.TableName)} can't be null or empty!");
                    return false;
                }

                if (table.IdColumns == null || table.IdColumns.Count <= 0)
                {
                    ErrorProvider.AddError($"{nameof(table.IdColumns)} can't be null or empty!");
                    return false;
                }

                if (table.AutoFindColumns == false && (table.Columns == null || table.Columns.Count <= 0))
                {
                    ErrorProvider.AddError($"{nameof(table.Columns)} can't be null or empty!");
                    return false;
                }

                if (table.EnableUpdate == false && table.EnableDelete == false && table.EnableInsert == false)
                {
                    ErrorProvider.AddError($"Table must have at least one enable DDL command!");
                    return false;
                }
            }

            return true;
        }
    }
}
