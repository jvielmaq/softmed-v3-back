namespace Softmed.V3.Common.Util;

using MySqlConnector;

/// <summary>
/// Singleton que provee conexiones MySQL leyendo los parámetros
/// desde variables de entorno Lambda (DB_HOST, DB_PORT, DB_NAME,
/// DB_USER, DB_PASSWORD). SSL requerido, pooling habilitado, max 10.
/// </summary>
public sealed class Conexion
{
    private static Conexion? _instance;
    private static readonly object _lock = new();

    private readonly string _connectionString;

    private Conexion()
    {
        var host     = Environment.GetEnvironmentVariable("DB_HOST")
                       ?? throw new InvalidOperationException("Variable de entorno DB_HOST no definida.");
        var port     = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
        var database = Environment.GetEnvironmentVariable("DB_NAME")
                       ?? throw new InvalidOperationException("Variable de entorno DB_NAME no definida.");
        var user     = Environment.GetEnvironmentVariable("DB_USER")
                       ?? throw new InvalidOperationException("Variable de entorno DB_USER no definida.");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD")
                       ?? throw new InvalidOperationException("Variable de entorno DB_PASSWORD no definida.");

        _connectionString =
            $"Server={host};" +
            $"Port={port};" +
            $"Database={database};" +
            $"User ID={user};" +
            $"Password={password};" +
            "SslMode=Required;" +
            "Pooling=true;" +
            "MaximumPoolSize=10;";
    }

    /// <summary>Instancia singleton (double-check lock).</summary>
    public static Conexion Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    _instance ??= new Conexion();
                }
            }
            return _instance;
        }
    }

    /// <summary>Retorna una conexión abierta (síncrono).</summary>
    public MySqlConnection GetConnexion()
    {
        var conn = new MySqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    /// <summary>Retorna una conexión abierta (async).</summary>
    public async Task<MySqlConnection> GetConnexionAsync()
    {
        var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}
