using COMMON;
using COMMON.Entidades;
using COMMON.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL
{
    public class Postgress<T> : IDB<T> where T : CamposControl
    {
        public string Error { get; private set; }
        private string cadenaDeConexion;
        private string campoId;
        private bool esAutonumerico;
        private object validador;

        public Postgress(string cadenaDeConexion, object validador, string campoId, bool esAutonumerico)
        {
            this.cadenaDeConexion = cadenaDeConexion;
            this.campoId = campoId;
            this.esAutonumerico = esAutonumerico;
            this.validador = validador;
            Error = "";
        }

        public T Actualizar(T entidad)
        {
            Error = "";
            try
            {
                string sql = $"UPDATE \"{typeof(T).Name}\" SET {string.Join(",",
                entidad.GetType().GetProperties().Where(p => p.Name !=
                campoId).Select(p => "\"" + p.Name + "\"=@" + p.Name))} WHERE \"{campoId}\"=@Id";

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
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        public List<M> EjecutarProcedimiento<M>(string nombre, Dictionary<string, string> parametros) where M : class
        {
            using (NpgsqlConnection conexion = new NpgsqlConnection(cadenaDeConexion))
            {
                conexion.Open();
                using (NpgsqlCommand comando = new NpgsqlCommand(nombre, conexion))
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
                            try
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                                    propiedad.SetValue(entidad, reader[propiedad.Name]);
                            }
                            catch
                            {
                                // Columna no existe en el resultado
                                continue;
                            }
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
                string sql = $"DELETE FROM \"{typeof(T).Name}\" WHERE \"{campoId}\"=@Id";
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
                using (NpgsqlConnection conexion = new NpgsqlConnection(cadenaDeConexion))
                {
                    conexion.Open();
                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
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

        public T Insertar(T entidad)
        {
            Error = "";
            try
            {
                string sql;
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                if (esAutonumerico)
                {
                    // PostgreSQL usa RETURNING para obtener el ID generado
                    sql = $"INSERT INTO \"{typeof(T).Name}\" ({string.Join(",",
                        entidad.GetType().GetProperties().Where(p => p.Name !=
                        campoId).Select(p => "\"" + p.Name + "\""))}) VALUES ({string.Join(",",
                        entidad.GetType().GetProperties().Where(p => p.Name !=
                        campoId).Select(p => "@" + p.Name))}) RETURNING \"{campoId}\"";

                    foreach (var propiedad in entidad.GetType().GetProperties().Where(p => p.Name != campoId))
                    {
                        parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
                    }

                    // Ejecutar comando y obtener ID generado
                    using (NpgsqlConnection conexion = new NpgsqlConnection(cadenaDeConexion))
                    {
                        conexion.Open();
                        using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                        {
                            foreach (var parametro in parametros)
                            {
                                comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
                            }

                            // Obtener el ID generado
                            var idGenerado = comando.ExecuteScalar();
                            if (idGenerado != null)
                            {
                                // Asignar el ID generado a la entidad
                                var propiedadId = entidad.GetType().GetProperty(campoId);
                                if (propiedadId.PropertyType == typeof(int))
                                {
                                    propiedadId.SetValue(entidad, Convert.ToInt32(idGenerado));
                                }
                                else
                                {
                                    propiedadId.SetValue(entidad, idGenerado);
                                }

                                return entidad;
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
                    sql = $"INSERT INTO \"{typeof(T).Name}\" ({string.Join(",",
                        entidad.GetType().GetProperties().Select(p => "\"" + p.Name + "\""))}) VALUES ({string.Join(",",
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
                string SQL = $"SELECT * FROM \"{typeof(T).Name}\" WHERE \"{campoId}\"=@Id";
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
                string SQL = $"SELECT * FROM \"{typeof(T).Name}\" WHERE \"{campoId}\"=@Id";
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
                string SQL = $"SELECT * FROM \"{typeof(T).Name}\"";
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
            using (NpgsqlConnection conexion = new NpgsqlConnection(cadenaDeConexion))
            {
                conexion.Open();
                using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
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
                            try
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
                                    else if (propiedad.PropertyType == typeof(DateTime) && value is string)
                                    {
                                        propiedad.SetValue(entidad, DateTime.Parse(value.ToString()));
                                    }
                                    else
                                    {
                                        propiedad.SetValue(entidad, value);
                                    }
                                }
                            }
                            catch
                            {
                                // Columna no existe o error de conversión
                                continue;
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
