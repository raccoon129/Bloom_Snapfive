using COMMON;
using COMMON.Entidades;
using COMMON.Interfaces;
using FluentValidation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    /// <summary>
    /// Implementación de acceso a datos para SQL Server aplicable a todas las entidades
    /// del sistema que heredan de CamposControl.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad que implementa CamposControl</typeparam>
    public class SQLServer<T> : IDB<T> where T : CamposControl
    {
        /// <summary>
        /// Almacena mensajes de error durante las operaciones
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Cadena de conexión a la base de datos SQL Server
        /// </summary>
        private readonly string _cadenaDeConexion;

        /// <summary>
        /// Nombre del campo que actúa como identificador único en la tabla
        /// </summary>
        private readonly string _campoId;

        /// <summary>
        /// Indica si el campo ID es auto-incrementable en la base de datos
        /// </summary>
        private readonly bool _esAutonumerico;

        /// <summary>
        /// Validador para la entidad usando FluentValidation
        /// </summary>
        private readonly AbstractValidator<T> _validador;

        /// <summary>
        /// Constructor para la clase de acceso a datos SQL Server
        /// </summary>
        /// <param name="cadenaDeConexion">Cadena de conexión a la base de datos</param>
        /// <param name="validador">Validador específico para la entidad</param>
        /// <param name="campoId">Nombre del campo ID en la base de datos</param>
        /// <param name="esAutonumerico">Indica si el ID es autoincrementable</param>
        public SQLServer(string cadenaDeConexion, AbstractValidator<T> validador, string campoId, bool esAutonumerico)
        {
            _cadenaDeConexion = cadenaDeConexion;
            _campoId = campoId;
            _esAutonumerico = esAutonumerico;
            _validador = validador;
            Error = string.Empty;
        }

        /// <summary>
        /// Actualiza un registro existente en la base de datos
        /// </summary>
        /// <param name="entidad">Entidad con los datos actualizados</param>
        /// <returns>La entidad actualizada o null si falla</returns>
        public T Actualizar(T entidad)
        {
            Error = string.Empty;
            try
            {

                var validationResult = _validador.Validate(entidad);
                if (!validationResult.IsValid)
                {
                    Error = string.Join(", ", validationResult.Errors);
                    return null;
                }

                var propiedades = entidad.GetType().GetProperties()
                    .Where(p => p.Name != _campoId)
                    .ToList();

                var sql = $"UPDATE {typeof(T).Name} SET " +
                          $"{string.Join(", ", propiedades.Select(p => $"{p.Name} = @{p.Name}"))} " +
                          $"WHERE {_campoId} = @Id";

                var parametros = new Dictionary<string, object>();
                propiedades.ForEach(p => parametros.Add($"@{p.Name}", p.GetValue(entidad) ?? DBNull.Value));
                parametros.Add("@Id", entidad.GetType().GetProperty(_campoId).GetValue(entidad));

                return EjecutarComando(sql, parametros) == 1 ? entidad : null;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado y devuelve una lista de objetos del tipo especificado
        /// </summary>
        /// <typeparam name="M">Tipo del objeto a devolver</typeparam>
        /// <param name="nombre">Nombre del procedimiento almacenado</param>
        /// <param name="parametros">Parámetros para el procedimiento</param>
        /// <returns>Lista de objetos del tipo M</returns>
        public List<M> EjecutarProcedimiento<M>(string nombre, Dictionary<string, string> parametros) where M : class
        {
            using var conexion = new SqlConnection(_cadenaDeConexion);
            conexion.Open();

            using var comando = new SqlCommand(nombre, conexion)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            foreach (var param in parametros)
            {
                comando.Parameters.AddWithValue(param.Key, param.Value ?? (object)DBNull.Value);
            }

            var reader = comando.ExecuteReader();
            var lista = new List<M>();

            while (reader.Read())
            {
                var entidad = Activator.CreateInstance<M>();
                foreach (var propiedad in entidad.GetType().GetProperties())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                    {
                        propiedad.SetValue(entidad, reader[propiedad.Name]);
                    }
                }
                lista.Add(entidad);
            }

            return lista;
        }

        /// <summary>
        /// Elimina un registro de la base de datos
        /// </summary>
        /// <param name="entidad">Entidad a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        public bool Eliminar(T entidad)
        {
            Error = string.Empty;
            try
            {
                var sql = $"DELETE FROM {typeof(T).Name} WHERE {_campoId} = @Id";
                var parametros = new Dictionary<string, object>
                {
                    { "@Id", entidad.GetType().GetProperty(_campoId).GetValue(entidad) }
                };
                return EjecutarComando(sql, parametros) == 1;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Ejecuta un comando SQL que no devuelve resultados (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="sql">Consulta SQL a ejecutar</param>
        /// <param name="parametros">Parámetros para la consulta</param>
        /// <returns>Número de filas afectadas o -1 si hay error</returns>
        private int EjecutarComando(string sql, Dictionary<string, object> parametros)
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaDeConexion);
                conexion.Open();
                using var comando = new SqlCommand(sql, conexion);

                foreach (var param in parametros)
                {
                    comando.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }

                return comando.ExecuteNonQuery();
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
        /// <returns>La entidad insertada con su ID generado (si es autonumérico) o null si falla</returns>
        public T Insertar(T entidad)
        {
            Error = string.Empty;
            try
            {

                var validationResult = _validador.Validate(entidad);
                if (!validationResult.IsValid)
                {
                    Error = string.Join(", ", validationResult.Errors);
                    return null;
                }

                var propiedades = entidad.GetType().GetProperties()
                    .Where(p => _esAutonumerico ? p.Name != _campoId : true)
                    .ToList();

                var sql = $"INSERT INTO {typeof(T).Name} (" +
                          $"{string.Join(", ", propiedades.Select(p => p.Name))}) " +
                          $"VALUES ({string.Join(", ", propiedades.Select(p => $"@{p.Name}"))})";

                if (_esAutonumerico)
                {
                    sql += $"; SELECT * FROM {typeof(T).Name} WHERE {_campoId} = SCOPE_IDENTITY()";
                }

                var parametros = new Dictionary<string, object>();
                propiedades.ForEach(p => parametros.Add($"@{p.Name}", p.GetValue(entidad) ?? DBNull.Value));

                if (_esAutonumerico)
                {
                    using var conexion = new SqlConnection(_cadenaDeConexion);
                    conexion.Open();
                    using var comando = new SqlCommand(sql, conexion);
                    propiedades.ForEach(p => comando.Parameters.AddWithValue($"@{p.Name}", p.GetValue(entidad) ?? DBNull.Value));
                    var reader = comando.ExecuteReader();
                    return reader.Read() ? MapearEntidad(reader) : null;
                }
                else
                {
                    return EjecutarComando(sql, parametros) == 1 ? entidad : null;
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
        /// <returns>La entidad encontrada o null si no existe</returns>
        public T ObtenerPorID(int id)
        {
            try
            {
                var sql = $"SELECT * FROM {typeof(T).Name} WHERE {_campoId} = @Id";
                var parametros = new Dictionary<string, object> { { "@Id", id } };
                return EjecutarConsulta(sql, parametros).FirstOrDefault();
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
        /// <returns>La entidad encontrada o null si no existe</returns>
        public T ObtenerPorID(string id)
        {
            try
            {
                var sql = $"SELECT * FROM {typeof(T).Name} WHERE {_campoId} = @Id";
                var parametros = new Dictionary<string, object> { { "@Id", id } };
                return EjecutarConsulta(sql, parametros).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Obtiene todos los registros de la tabla
        /// </summary>
        /// <returns>Lista de todas las entidades</returns>
        public List<T> ObtenerTodas()
        {
            try
            {
                var sql = $"SELECT * FROM {typeof(T).Name}";
                return EjecutarConsulta(sql, new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Ejecuta una consulta SQL que devuelve resultados (SELECT)
        /// </summary>
        /// <param name="sql">Consulta SQL a ejecutar</param>
        /// <param name="parametros">Parámetros para la consulta</param>
        /// <returns>Lista de entidades que coinciden con la consulta</returns>
        private List<T> EjecutarConsulta(string sql, Dictionary<string, object> parametros)
        {
            using var conexion = new SqlConnection(_cadenaDeConexion);
            conexion.Open();
            using var comando = new SqlCommand(sql, conexion);

            foreach (var param in parametros)
            {
                comando.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            var reader = comando.ExecuteReader();
            var lista = new List<T>();

            while (reader.Read())
            {
                lista.Add(MapearEntidad(reader));
            }

            return lista;
        }

        /// <summary>
        /// Mapea un registro del SqlDataReader a un objeto de tipo T
        /// </summary>
        /// <param name="reader">SqlDataReader con los datos del registro</param>
        /// <returns>Objeto de tipo T con los datos mapeados</returns>
        private T MapearEntidad(SqlDataReader reader)
        {
            var entidad = Activator.CreateInstance<T>();
            foreach (var propiedad in entidad.GetType().GetProperties())
            {
                try
                {
                    // Verificar si la propiedad existe en el reader
                    if (HasColumn(reader, propiedad.Name) && !reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                    {
                        var value = reader[propiedad.Name];

                        // Manejar conversiones específicas de tipos
                        if (propiedad.PropertyType == typeof(int) && value is string strValue)
                        {
                            propiedad.SetValue(entidad, Convert.ToInt32(strValue));
                        }
                        else if (propiedad.PropertyType == typeof(string) && value is int intValue)
                        {
                            propiedad.SetValue(entidad, intValue.ToString());
                        }
                        else if (propiedad.PropertyType == typeof(bool) && !(value is bool))
                        {
                            // Convertir valores numéricos o string a booleano
                            if (value is int intValue2)
                                propiedad.SetValue(entidad, intValue2 != 0);
                            else if (value is string strValue2)
                                propiedad.SetValue(entidad, strValue2.ToLower() == "true" || strValue2 == "1");
                        }
                        else if (propiedad.PropertyType == typeof(DateTime) && value is string strDate)
                        {
                            propiedad.SetValue(entidad, DateTime.Parse(strDate));
                        }
                        else if (propiedad.PropertyType.IsEnum && value is string strEnum)
                        {
                            propiedad.SetValue(entidad, Enum.Parse(propiedad.PropertyType, strEnum, true));
                        }
                        else
                        {
                            // Intentar la conversión directa
                            if (value != null && value != DBNull.Value)
                            {
                                propiedad.SetValue(entidad, Convert.ChangeType(value, Nullable.GetUnderlyingType(propiedad.PropertyType) ?? propiedad.PropertyType));
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // En caso de error al convertir, continuar con la siguiente propiedad
                    continue;
                }
            }
            return entidad;
        }

        /// <summary>
        /// Verifica si una columna existe en el SqlDataReader
        /// </summary>
        /// <param name="reader">SqlDataReader a verificar</param>
        /// <param name="columnName">Nombre de la columna</param>
        /// <returns>True si la columna existe, False en caso contrario</returns>
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
