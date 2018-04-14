// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;

    static partial class ExceptionExtensions
    {
        internal static bool IsPrimaryKeyViolation( this DbException exception ) =>
            ( exception.GetBaseException() as SqlException )?.Errors.Cast<SqlError>().Any( e => e.Number == 2627 ) ?? false;
    }
}