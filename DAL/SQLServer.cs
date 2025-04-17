using COMMON;
using COMMON.Entidades;
using COMMON.Interfaces;
using FluentValidation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DAL
{
    public class SQLServer<T> : IDB<T> where T : CamposControl
    {
        public string Error { get; private set; }
        private readonly string cadenaDeConexion;
        private readonly string campoId;
        private readonly bool esAutonumerico;
        private readonly IValidator<T> validador;

        public SQLServer(string cadenaDeConexion, object validador, string campoId, bool esAutonumerico)
        {
            this.cadenaDeConexion = cadenaDeConexion;
            this.campoId = campoId;
            this.esAutonumerico = esAutonumerico;
            this.validador = (IValidator<T>)validador;
            Error = "";
        }

        public T Actualizar(T entidad)
        {
            Error = "";
            try
            {
                // En este proyecto, CamposControl no tiene propiedades de auditoría
                // por lo que se eliminan las líneas: entidad.UsuarioMod y entidad.FechaMod

                // Utilizamos FluentValidation para validar la entidad
                var resultadoValidacion = validador.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {
                    string sql = $"UPDATE {typeof(T).Name} SET {string.Join(",",
                    entidad.GetType().GetProperties().Where(p => p.Name !=
                    campoId).Select(p => p.Name + "=@" + p.Name))} WHERE {campoId}=@Id";

                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                    foreach (var propiedad in entidad.GetType().GetProperties().Where(p => p.Name != campoId))
                    {
                        parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
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

        public List<M> EjecutarProcedimiento<M>(string nombre, Dictionary<string, string> parametros) where M : class
        {
            using (SqlConnection conexion = new SqlConnection(cadenaDeConexion))
            {
                conexion.Open();
                using (SqlCommand comando = new SqlCommand(nombre, conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
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
                            if (!reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                                propiedad.SetValue(entidad, reader[propiedad.Name]);
                        }
                        lista.Add(entidad);
                    }
                    return lista;
                }
            }
        }

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

        private int EjecutarComando(string sql, Dictionary<string, object> parametros)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaDeConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand(sql, conexion))
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

        public T Insertar(T entidad)
        {
            Error = "";
            try
            {
                // En este proyecto, CamposControl no tiene propiedades de auditoría
                // por lo que se eliminan las líneas: entidad.UsuarioAlta y entidad.FechaAlta

                // Utilizamos FluentValidation para validar la entidad
                var resultadoValidacion = validador.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {
                    string sql;
                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                    if (esAutonumerico)
                    {
                        sql = $"INSERT INTO {typeof(T).Name} ({string.Join(",",
                            entidad.GetType().GetProperties().Where(p => p.Name !=
                            campoId).Select(p => p.Name))}) VALUES ({string.Join(",",
                            entidad.GetType().GetProperties().Where(p => p.Name !=
                            campoId).Select(p => "@" + p.Name))})";

                        foreach (var propiedad in entidad.GetType().GetProperties().Where(p => p.Name != campoId))
                        {
                            parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
                        }

                        // En SQL Server usamos SCOPE_IDENTITY() en lugar de LAST_INSERT_ID()
                        sql += "; SELECT * FROM " + typeof(T).Name + " WHERE " + campoId + " = SCOPE_IDENTITY()";

                        using (SqlConnection conexion = new SqlConnection(cadenaDeConexion))
                        {
                            conexion.Open();
                            using (SqlCommand comando = new SqlCommand(sql, conexion))
                            {
                                foreach (var parametro in parametros)
                                {
                                    comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
                                }

                                var reader = comando.ExecuteReader();
                                if (reader.Read())
                                {
                                    return MapearEntidad(reader);
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                    }
                    else
                    {
                        sql = $"INSERT INTO {typeof(T).Name} ({string.Join(",",
                            entidad.GetType().GetProperties().Select(p => p.Name))}) VALUES ({string.Join(",",
                            entidad.GetType().GetProperties().Select(p => "@" + p.Name))})";

                        foreach (var propiedad in entidad.GetType().GetProperties())
                        {
                            parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
                        }

                        if (EjecutarComando(sql, parametros) == 1)
                        {
                            return entidad;
                        }
                        else
                        {
                            return null;
                        }
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

        private List<T> EjecutarConsulta(string sql, Dictionary<string, object> parametros)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaDeConexion))
            {
                conexion.Open();
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    foreach (var parametro in parametros)
                    {
                        comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
                    }
                    var reader = comando.ExecuteReader();
                    List<T> lista = new List<T>();
                    while (reader.Read())
                    {
                        lista.Add(MapearEntidad(reader));
                    }
                    return lista;
                }
            }
        }

        private T MapearEntidad(SqlDataReader reader)
        {
            T entidad = Activator.CreateInstance<T>();
            foreach (var propiedad in entidad.GetType().GetProperties())
            {
                if (!reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                {
                    var value = reader[propiedad.Name];
                    if (propiedad.PropertyType == typeof(int) && value is string)
                    {
                        propiedad.SetValue(entidad, Convert.ToInt32(value));
                    }
                    else if (propiedad.PropertyType == typeof(string) && value is int)
                    {
                        propiedad.SetValue(entidad, value.ToString());
                    }
                    else
                    {
                        propiedad.SetValue(entidad, value);
                    }
                }
            }
            return entidad;
        }
    }
}
