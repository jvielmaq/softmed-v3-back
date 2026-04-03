namespace Softmed.V3.Login.Business.Repositorio;

using MySqlConnector;

public static class RepoRecovery
{
    public static async Task InsertarToken(int idUsuario, string token, string ip, MySqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO TBL_LOG_RECOVERY_PASS (id_usuario, token, ip, expires_at)
            VALUES (@idUsuario, @token, @ip, DATE_ADD(UTC_TIMESTAMP(), INTERVAL 1 HOUR))";
        cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
        cmd.Parameters.AddWithValue("@token", token);
        cmd.Parameters.AddWithValue("@ip", ip);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<int?> ValidarToken(string token, MySqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id_usuario FROM TBL_LOG_RECOVERY_PASS
            WHERE token = @token AND utilizado = 0 AND expires_at > UTC_TIMESTAMP()
            LIMIT 1";
        cmd.Parameters.AddWithValue("@token", token);
        var result = await cmd.ExecuteScalarAsync();
        return result is null ? null : Convert.ToInt32(result);
    }

    public static async Task MarcarUtilizado(string token, MySqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE TBL_LOG_RECOVERY_PASS SET utilizado = 1 WHERE token = @token";
        cmd.Parameters.AddWithValue("@token", token);
        await cmd.ExecuteNonQueryAsync();
    }
}
