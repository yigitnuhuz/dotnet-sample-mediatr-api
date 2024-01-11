using Core.Helpers;

namespace Data.Utils;

public interface IDbHelper : ISqlHelper
{
}

public class DbHelper(string connectionString, string readOnlyConnectionString)
    : SqlHelper(connectionString, readOnlyConnectionString), IDbHelper;