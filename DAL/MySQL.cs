using COMMON;
using COMMON.Entidades;
using COMMON.Interfaces;
using FluentValidation;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    /// <summary>
    /// Implementación de acceso a datos para MySQL aplicable a todas las entidades
    /// del sistema que heredan de CamposControl.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad que implementa CamposControl</typeparam>
    public class MySQL<T> : IDB<T> where T : CamposControl
    {
        /// <summary>
        /// Almacena mensajes de error durante las operaciones
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Cadena de conexión a la base de datos MySQL
        /// </summary>
        private string cadenaDeConexion;

        /// <summary>
        /// Nombre del campo que actúa como identificador único en la tabla
        /// </summary>
        private string campoId;

        /// <summary>
        /// Indica si el campo ID es auto-incrementable en la base de datos
        /// </summary>
        private bool esAutonumerico;

        /// <summary>
        /// Validador para la entidad usando FluentValidation
        /// </summary>
        private AbstractValidator<T> validador;

        /// <summary>
        /// Constructor para la clase de acceso a datos MySQL
        /// </summary>
        /// <param name="cadenaDeConexion">Cadena de conexión a la base de datos</param>
        /// <param name="validador">Validador específico para la entidad</param>
        /// <param name="campoId">Nombre del campo ID en la base de datos</param>
        /// <param name="esAutonumerico">Indica si el ID es autoincrementable</param>
        public MySQL(string cadenaDeConexion, AbstractValidator<T> validador, string campoId, bool esAutonumerico)
        {
            this.cadenaDeConexion = cadenaDeConexion;
            this.campoId = campoId;
            this.esAutonumerico = esAutonumerico;
            this.validador = validador;
            Error = "";
        }

        /// <summary>
        /// Actualiza un registro existente en la base de datos
        /// </summary>
        /// <param name="entidad">Entidad con los datos actualizados</param>
        /// <returns>La entidad actualizada o null si falla</returns>
        public T Actualizar(T entidad)
        {
            Error = "";
            try
            {

                var resultadoValidacion = validador.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {
                    // Crear consulta SQL para actualizar todos los campos excepto el ID
                    string sql = $"UPDATE {typeof(T).Name} SET {string.Join(",",
                    entidad.GetType().GetProperties().Where(p => p.Name !=
                    campoId).Select(p => p.Name + "=@" + p.Name))} WHERE {campoId}=@Id";
                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                    foreach (var propiedad in entidad.GetType().GetProperties().Where(p => p.Name != campoId))
                    {
                        parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad));
                    }
                    parametros.Add("@Id", entidad.GetType().GetProperty(campoId).GetValue(entidad));
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
                    Error = string.Join(",", resultadoValidacion.Errors);
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
        /// Ejecuta un procedimiento almacenado y devuelve una lista de objetos del tipo especificado
        /// </summary>
        /// <typeparam name="M">Tipo del objeto a devolver</typeparam>
        /// <param name="nombre">Nombre del procedimiento almacenado</param>
        /// <param name="parametros">Parámetros para el procedimiento</param>
        /// <returns>Lista de objetos del tipo M</returns>
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
                        comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
                    }
                    var reader = comando.ExecuteReader();
                    List<M> lista = new List<M>();
                    while (reader.Read())
                    {
                        M entidad = Activator.CreateInstance<M>();
                        foreach (var propiedad in entidad.GetType().GetProperties())
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                                propiedad.SetValue(entidad, reader[propiedad.Name]);
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
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        public bool Eliminar(T entidad)
        {
            Error = "";
            try
            {
                string sql = $"DELETE FROM {typeof(T).Name} WHERE {campoId}=@Id";
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
        /// Ejecuta un comando SQL que no devuelve resultados (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="sql">Consulta SQL a ejecutar</param>
        /// <param name="parametros">Parámetros para la consulta</param>
        /// <returns>Número de filas afectadas o -1 si hay error</returns>
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
                            comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
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
        /// <returns>La entidad insertada con su ID generado (si es autonumérico) o null si falla</returns>
        public T Insertar(T entidad)
        {
            Error = "";
            try
            {
                // Establecer campos de control

                var resultadoValidacion = validador.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {
                    string sql;
                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                    if (esAutonumerico)
                    {
                        // Si el ID es autonumérico, no lo incluimos en el INSERT
                        sql = $"INSERT INTO {typeof(T).Name} ({string.Join(",",
                            entidad.GetType().GetProperties().Where(p => p.Name !=
                            campoId).Select(p => p.Name))}) VALUES ({string.Join(",",
                            entidad.GetType().GetProperties().Where(p => p.Name !=
                            campoId).Select(p => "@" + p.Name))})";

                        foreach (var propiedad in entidad.GetType().GetProperties().Where(p => p.Name != campoId))
                        {
                            parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad));
                        }
                    }
                    else
                    {
                        // Si el ID no es autonumérico, lo incluimos en el INSERT
                        sql = $"INSERT INTO {typeof(T).Name} ({string.Join(",",
                            entidad.GetType().GetProperties().Select(p => p.Name))}) VALUES ({string.Join(",",
                            entidad.GetType().GetProperties().Select(p => "@" + p.Name))})";

                        foreach (var propiedad in entidad.GetType().GetProperties())
                        {
                            parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad));
                        }
                    }

                    if (EjecutarComando(sql, parametros) == 1)
                    {
                        if (esAutonumerico)
                        {
                            // Recuperamos la entidad recién insertada con el ID generado
                            sql = $"SELECT * FROM {typeof(T).Name} WHERE {campoId} = LAST_INSERT_ID()";
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
                    Error = string.Join(",", resultadoValidacion.Errors);
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
        /// <returns>La entidad encontrada o null si no existe</returns>
        public T ObtenerPorID(int id)
        {
            try
            {
                string SQL = $"SELECT * FROM {typeof(T).Name} WHERE {campoId}=@Id";
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
        /// <returns>La entidad encontrada o null si no existe</returns>
        public T ObtenerPorID(string id)
        {
            try
            {
                string SQL = $"SELECT * FROM {typeof(T).Name} WHERE {campoId}=@Id";
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
        /// Obtiene todos los registros de la tabla
        /// </summary>
        /// <returns>Lista de todas las entidades</returns>
        public List<T> ObtenerTodas()
        {
            try
            {
                string SQL = $"SELECT * FROM {typeof(T).Name}";
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
        /// Ejecuta una consulta SQL que devuelve resultados (SELECT)
        /// </summary>
        /// <param name="sql">Consulta SQL a ejecutar</param>
        /// <param name="parametros">Parámetros para la consulta</param>
        /// <returns>Lista de entidades que coinciden con la consulta</returns>
        private List<T> EjecutarConsulta(string sql, Dictionary<string, object> parametros)
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaDeConexion))
            {
                conexion.Open();
                using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                {
                    foreach (var parametro in parametros)
                    {
                        comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
                    }
                    var reader = comando.ExecuteReader();
                    List<T> lista = new List<T>();
                    while (reader.Read())
                    {
                        T entidad = Activator.CreateInstance<T>();
                        foreach (var propiedad in entidad.GetType().GetProperties())
                        {
                            // Solo asignamos valores a propiedades que existen en el resultado
                            if (reader.HasRows && !reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                            {
                                var value = reader[propiedad.Name];
                                // Conversiones de tipo cuando sea necesario
                                if (propiedad.PropertyType == typeof(int) && value is string)
                                {
                                    propiedad.SetValue(entidad, Convert.ToInt32(value));
                                }
                                else if (propiedad.PropertyType == typeof(string) && value is int)
                                {
                                    propiedad.SetValue(entidad, value.ToString());
                                }
                                else if (propiedad.PropertyType == typeof(bool) && !(value is bool))
                                {
                                    // Convertir valores numéricos o string a booleano
                                    if (value is int intValue)
                                        propiedad.SetValue(entidad, intValue != 0);
                                    else if (value is string strValue)
                                        propiedad.SetValue(entidad, strValue.ToLower() == "true" || strValue == "1");
                                }
                                else if (propiedad.PropertyType == typeof(DateTime) && value is string strDate)
                                {
                                    propiedad.SetValue(entidad, DateTime.Parse(strDate));
                                }
                                else
                                {
                                    propiedad.SetValue(entidad, value);
                                }
                            }
                        }
                        lista.Add(entidad);
                    }
                    return lista;
                }
            }
        }
    }
}
