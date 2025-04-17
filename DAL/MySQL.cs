using COMMON;
using COMMON.Entidades;
using COMMON.Interfaces;
using FluentValidation;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

//ESTE YA FUNCIONA. POSTGRES Y SQLSERVER NO ES FUNCIONAL
namespace DAL
{
    /// <summary>
    /// Implementación de IDB para MySQL que maneja la conversión entre nomenclatura PascalCase (.NET)
    /// y snake_case (MySQL) de manera automática.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad que debe heredar de CamposControl</typeparam>
    public class MySQL<T> : IDB<T> where T : CamposControl
    {
        public string Error { get; private set; }
        private string cadenaDeConexion;
        private string campoId;
        private bool esAutonumerico;
        private IValidator<T> validador;

        /// <summary>
        /// Constructor para la clase MySQL
        /// </summary>
        /// <param name="cadenaDeConexion">Cadena de conexión a la base de datos MySQL</param>
        /// <param name="validador">Validador de FluentValidation para la entidad</param>
        /// <param name="campoId">Nombre de la propiedad que representa la clave primaria</param>
        /// <param name="esAutonumerico">Indica si la clave primaria es autonumérica</param>
        public MySQL(string cadenaDeConexion, object validador, string campoId, bool esAutonumerico)
        {
            this.cadenaDeConexion = cadenaDeConexion;
            this.campoId = campoId;
            this.esAutonumerico = esAutonumerico;
            this.validador = (IValidator<T>)validador;
            Error = "";
        }

        /// <summary>
        /// Actualiza un registro en la base de datos
        /// </summary>
        /// <param name="entidad">Entidad con los datos actualizados</param>
        /// <returns>La entidad actualizada o null si hay error</returns>
        public T Actualizar(T entidad)
        {
            Error = "";
            try
            {
                // Validamos la entidad con FluentValidation
                var resultadoValidacion = validador.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {
                    // Convertimos los nombres de tabla y columnas a snake_case para MySQL
                    string tableName = ConvertToSnakeCase(typeof(T).Name);
                    string idColumn = ConvertToSnakeCase(campoId);

                    // Preparamos la parte SET de la consulta con nombres en snake_case
                    var setClauses = entidad.GetType().GetProperties()
                        .Where(p => p.Name != campoId)
                        .Select(p => $"{ConvertToSnakeCase(p.Name)}=@{p.Name}");

                    string sql = $"UPDATE {tableName} SET {string.Join(",", setClauses)} WHERE {idColumn}=@Id";

                    // Preparamos los parámetros
                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                    foreach (var propiedad in entidad.GetType().GetProperties().Where(p => p.Name != campoId))
                    {
                        parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
                    }
                    parametros.Add("@Id", entidad.GetType().GetProperty(campoId).GetValue(entidad));

                    // Ejecutamos el comando
                    var r = EjecutarComando(sql, parametros);
                    if (r == 1)
                    {
                        return entidad;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Error = string.Join(", ", resultadoValidacion.Errors.Select(e => e.ErrorMessage));
                    return null;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos
        /// </summary>
        /// <typeparam name="M">Tipo de entidad de retorno</typeparam>
        /// <param name="nombre">Nombre del procedimiento almacenado</param>
        /// <param name="parametros">Diccionario con los parámetros del procedimiento</param>
        /// <returns>Lista de entidades del tipo M</returns>
        public List<M> EjecutarProcedimiento<M>(string nombre, Dictionary<string, string> parametros) where M : class
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaDeConexion))
            {
                conexion.Open();
                using (MySqlCommand comando = new MySqlCommand(nombre, conexion))
                {
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    foreach (var parametro in parametros)
                    {
                        comando.Parameters.AddWithValue(parametro.Key, parametro.Value ?? (object)DBNull.Value);
                    }
                    var reader = comando.ExecuteReader();
                    List<M> lista = new List<M>();
                    while (reader.Read())
                    {
                        M entidad = Activator.CreateInstance<M>();
                        foreach (var propiedad in entidad.GetType().GetProperties())
                        {
                            // Convertimos el nombre de la propiedad a snake_case para MySQL
                            string columnName = ConvertToSnakeCase(propiedad.Name);

                            try
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                                {
                                    propiedad.SetValue(entidad, reader[columnName]);
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                // Intentamos con el nombre original si la conversión no funciona
                                try
                                {
                                    if (!reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                                    {
                                        propiedad.SetValue(entidad, reader[propiedad.Name]);
                                    }
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    // La columna no existe, continuamos con la siguiente propiedad
                                    continue;
                                }
                            }
                        }
                        lista.Add(entidad);
                    }
                    return lista;
                }
            }
        }

        /// <summary>
        /// Elimina un registro de la base de datos
        /// </summary>
        /// <param name="entidad">Entidad a eliminar</param>
        /// <returns>true si se eliminó correctamente, false en caso contrario</returns>
        public bool Eliminar(T entidad)
        {
            Error = "";
            try
            {
                // Convertimos los nombres de tabla y columnas a snake_case para MySQL
                string tableName = ConvertToSnakeCase(typeof(T).Name);
                string idColumn = ConvertToSnakeCase(campoId);

                string sql = $"DELETE FROM {tableName} WHERE {idColumn}=@Id";
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Id", entidad.GetType().GetProperty(campoId).GetValue(entidad));
                return EjecutarComando(sql, parametros) == 1;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Método privado para ejecutar comandos SQL que no devuelven resultados
        /// </summary>
        /// <param name="sql">Consulta SQL a ejecutar</param>
        /// <param name="parametros">Parámetros de la consulta</param>
        /// <returns>Número de filas afectadas o -1 en caso de error</returns>
        private int EjecutarComando(string sql, Dictionary<string, object> parametros)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaDeConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                    {
                        foreach (var parametro in parametros)
                        {
                            comando.Parameters.AddWithValue(parametro.Key, parametro.Value ?? DBNull.Value);
                        }
                        return comando.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return -1;
            }
        }

        /// <summary>
        /// Inserta un nuevo registro en la base de datos
        /// </summary>
        /// <param name="entidad">Entidad a insertar</param>
        /// <returns>La entidad insertada con su ID generado o null si hay error</returns>
        public T Insertar(T entidad)
        {
            Error = "";
            try
            {
                // Validamos la entidad con FluentValidation
                var resultadoValidacion = validador.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {
                    string sql;
                    Dictionary<string, object> parametros = new Dictionary<string, object>();

                    // Convertir el nombre de la tabla a snake_case
                    string tableName = ConvertToSnakeCase(typeof(T).Name);

                    if (esAutonumerico)
                    {
                        // Preparamos las columnas en snake_case excluyendo el ID
                        var columnas = entidad.GetType().GetProperties()
                            .Where(p => p.Name != campoId)
                            .Select(p => ConvertToSnakeCase(p.Name));

                        var parametrosSQL = entidad.GetType().GetProperties()
                            .Where(p => p.Name != campoId)
                            .Select(p => "@" + p.Name);

                        sql = $"INSERT INTO {tableName} ({string.Join(",", columnas)}) " +
                              $"VALUES ({string.Join(",", parametrosSQL)})";

                        foreach (var propiedad in entidad.GetType().GetProperties().Where(p => p.Name != campoId))
                        {
                            parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
                        }
                    }
                    else
                    {
                        // Preparamos las columnas en snake_case incluyendo el ID
                        var columnas = entidad.GetType().GetProperties()
                            .Select(p => ConvertToSnakeCase(p.Name));

                        var parametrosSQL = entidad.GetType().GetProperties()
                            .Select(p => "@" + p.Name);

                        sql = $"INSERT INTO {tableName} ({string.Join(",", columnas)}) " +
                              $"VALUES ({string.Join(",", parametrosSQL)})";

                        foreach (var propiedad in entidad.GetType().GetProperties())
                        {
                            parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
                        }
                    }

                    if (EjecutarComando(sql, parametros) == 1)
                    {
                        if (esAutonumerico)
                        {
                            // Convertir el nombre de la columna ID a snake_case para LAST_INSERT_ID()
                            string idColumn = ConvertToSnakeCase(campoId);
                            sql = $"SELECT * FROM {tableName} WHERE {idColumn} = LAST_INSERT_ID()";
                            var consulta = EjecutarConsulta(sql, new Dictionary<string, object>());
                            if (consulta.Count == 1)
                            {
                                return consulta.First();
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return entidad;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Error = string.Join(", ", resultadoValidacion.Errors.Select(e => e.ErrorMessage));
                    return null;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Obtiene un registro por su ID numérico
        /// </summary>
        /// <param name="id">ID del registro a obtener</param>
        /// <returns>La entidad con el ID especificado o null si no existe</returns>
        public T ObtenerPorID(int id)
        {
            try
            {
                // Convertir el nombre de la tabla y la columna a snake_case
                string tableName = ConvertToSnakeCase(typeof(T).Name);
                string idColumn = ConvertToSnakeCase(campoId);

                string SQL = $"SELECT * FROM {tableName} WHERE {idColumn}=@Id";
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Id", id);
                return EjecutarConsulta(SQL, parametros).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Obtiene un registro por su ID en formato string
        /// </summary>
        /// <param name="id">ID del registro a obtener</param>
        /// <returns>La entidad con el ID especificado o null si no existe</returns>
        public T ObtenerPorID(string id)
        {
            try
            {
                // Convertir el nombre de la tabla y la columna a snake_case
                string tableName = ConvertToSnakeCase(typeof(T).Name);
                string idColumn = ConvertToSnakeCase(campoId);

                string SQL = $"SELECT * FROM {tableName} WHERE {idColumn}=@Id";
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Id", id);
                return EjecutarConsulta(SQL, parametros).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Obtiene todos los registros de la entidad
        /// </summary>
        /// <returns>Lista con todas las entidades o null si hay error</returns>
        public List<T> ObtenerTodas()
        {
            try
            {
                // Convertir el nombre de la tabla a snake_case
                string tableName = ConvertToSnakeCase(typeof(T).Name);

                string SQL = $"SELECT * FROM {tableName}";
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                return EjecutarConsulta(SQL, parametros);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Método para ejecutar consultas SQL que devuelven resultados
        /// </summary>
        /// <param name="sql">Consulta SQL a ejecutar</param>
        /// <param name="parametros">Parámetros de la consulta</param>
        /// <returns>Lista de entidades resultantes de la consulta</returns>
        private List<T> EjecutarConsulta(string sql, Dictionary<string, object> parametros)
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaDeConexion))
            {
                conexion.Open();
                using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                {
                    foreach (var parametro in parametros)
                    {
                        comando.Parameters.AddWithValue(parametro.Key, parametro.Value ?? DBNull.Value);
                    }
                    var reader = comando.ExecuteReader();
                    List<T> lista = new List<T>();
                    while (reader.Read())
                    {
                        T entidad = Activator.CreateInstance<T>();
                        foreach (var propiedad in entidad.GetType().GetProperties())
                        {
                            // Convertir el nombre de la propiedad de PascalCase a snake_case
                            string columnName = ConvertToSnakeCase(propiedad.Name);

                            // Comprobar si la columna existe en el resultado
                            try
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                                {
                                    var value = reader[columnName];
                                    if (propiedad.PropertyType == typeof(int) && value is string)
                                    {
                                        propiedad.SetValue(entidad, Convert.ToInt32(value));
                                    }
                                    else if (propiedad.PropertyType == typeof(string) && value is int)
                                    {
                                        propiedad.SetValue(entidad, value.ToString());
                                    }
                                    else if (propiedad.PropertyType == typeof(DateTime) && value is string)
                                    {
                                        propiedad.SetValue(entidad, DateTime.Parse(value.ToString()));
                                    }
                                    else if (propiedad.PropertyType == typeof(bool) && value is int)
                                    {
                                        // MySQL almacena booleanos como 0/1
                                        propiedad.SetValue(entidad, Convert.ToInt32(value) != 0);
                                    }
                                    else
                                    {
                                        propiedad.SetValue(entidad, value);
                                    }
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                // Intenta con el nombre original si la conversión no funciona
                                try
                                {
                                    if (!reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                                    {
                                        var value = reader[propiedad.Name];
                                        propiedad.SetValue(entidad, value);
                                    }
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    // La columna no existe, continúa con la siguiente propiedad
                                    continue;
                                }
                            }
                        }
                        lista.Add(entidad);
                    }
                    return lista;
                }
            }
        }

        /// <summary>
        /// Método para convertir nombres de PascalCase a snake_case
        /// Ejemplo: "IdUsuario" se convierte a "id_usuario"
        /// </summary>
        /// <param name="input">Nombre en formato PascalCase</param>
        /// <returns>Nombre convertido a snake_case</returns>
        private string ConvertToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Para manejar casos especiales como "Ruido_Alerta"
            // primero dividimos por los guiones bajos existentes
            string[] parts = input.Split('_');
            var result = new System.Text.StringBuilder();

            for (int p = 0; p < parts.Length; p++)
            {
                if (p > 0) result.Append('_');

                string part = parts[p];
                if (string.IsNullOrEmpty(part)) continue;

                result.Append(char.ToLower(part[0]));

                for (int i = 1; i < part.Length; i++)
                {
                    if (char.IsUpper(part[i]))
                    {
                        result.Append('_');
                        result.Append(char.ToLower(part[i]));
                    }
                    else
                    {
                        result.Append(part[i]);
                    }
                }
            }

            return result.ToString();
        }
    }
}
