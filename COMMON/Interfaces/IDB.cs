using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Interfaces
{
    public interface IDB<T>
    {
        /// <summary>
        /// Interfaz que fefine el comportamiento de unaclase para la comunicacion coon una base de datos 
        /// </summary>

        string Error { get; }
        /// <summary>
        /// Obtine el error si es que existe
        /// </summary>
        /// <returns></returns>
        List<T> ObtenerTodas();
        /// <summary>
        /// Obtine el regisro corresponidente
        /// </summary>
        /// <param name="id">id del registro a obtener</param>
        /// <returns>Objeto corresondiente al id proporcionado </returns>
        T ObtenerPorID(int id);
        T ObtenerPorID(string id);
        bool Eliminar(T entidad);
        T Insertar(T entidad);
        T Actualizar(T entidad);
        List<M> EjecutarProcedimiento<M>
            (string nombre, Dictionary<string, string> parametros)
            where M : class;

    }

}
