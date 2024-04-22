using System.Data;

namespace TibiaStalker.Application.Dapper;

public interface IDapperConnectionProvider
{
    IDbConnection GetConnection(EDataBaseType eModule);
}