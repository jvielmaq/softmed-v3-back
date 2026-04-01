namespace Softmed.V3.Login.Business.Repositorio;

using MySqlConnector;
using Softmed.V3.Login.Business.Modelo;

public static class RepoLogin
{
    /// <summary>
    /// Obtiene el usuario por email con todos los datos necesarios para la autenticación.
    /// JOIN: TBL_USUARIO → TBL_PERSONA → TBL_ROL → TBL_EMPLEADO → TBL_INSTITUCION → TBL_TENANT
    /// </summary>
    /// <returns><see cref="UsuarioLogin"/> o null si no existe.</returns>
    public static async Task<UsuarioLogin?> ObtenerUsuario(string email, MySqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT
                u.USUAR_CDG_ID                          AS id_usuario,
                p.PERSO_CAR_NOMBRES                     AS nombres,
                p.PERSO_CAR_APELLIDOS                   AS apellidos,
                u.USUAR_CAR_EMAIL                       AS email,
                u.USUAR_CAR_CLAVE                       AS clave,
                u.USUAR_CAR_KEY                         AS `key`,
                r.ROL_CAR_NOMBRE                        AS nombre_rol,
                u.USUAR_BOO_ACTIVO                      AS activo,
                COALESCE(e.TBL_INSTITUCION_INSTI_CDG_ID, 0) AS id_institucion,
                COALESCE(t.TENAN_CDG_ID, 0)             AS id_tenant
            FROM       TBL_USUARIO      u
            INNER JOIN TBL_PERSONA      p  ON p.PERSO_CDG_ID                = u.TBL_PERSONA_PERSO_CDG_ID
            INNER JOIN TBL_ROL          r  ON r.ROL_CDG_ID                  = u.TBL_ROL_ROL_CDG_ID
            LEFT  JOIN TBL_EMPLEADO     e  ON e.TBL_PERSONA_PERSO_CDG_ID    = p.PERSO_CDG_ID
            LEFT  JOIN TBL_INSTITUCION  i  ON i.INSTI_CDG_ID                = e.TBL_INSTITUCION_INSTI_CDG_ID
            LEFT  JOIN TBL_TENANT       t  ON t.TENAN_CDG_ID                = (SELECT FEATU_CDG_TENANT FROM TBL_FEATURE_FLAG WHERE FEATU_BOO_ACTIVO = 1 LIMIT 1)
            WHERE u.USUAR_CAR_EMAIL = @email
            LIMIT 1";

        cmd.Parameters.AddWithValue("@email", email);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new UsuarioLogin
        {
            IdUsuario     = reader.GetInt32("id_usuario"),
            Nombres       = reader.GetString("nombres"),
            Apellidos     = reader.GetString("apellidos"),
            Email         = reader.GetString("email"),
            Clave         = reader.GetString("clave"),
            Key           = reader.GetString("key"),
            NombreRol     = reader.GetString("nombre_rol"),
            Activo        = reader.GetBoolean("activo"),
            IdInstitucion = reader.GetInt32("id_institucion"),
            TenantId      = reader.GetInt32("id_tenant")
        };
    }
}
