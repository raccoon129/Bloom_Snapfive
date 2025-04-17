using COMMON.Entidades;
using COMMON.Interfaces;
using COMMON.Validadores;


namespace DAL
{
    public class FabricRepository
    {

        private string _cadenaDeConexion;
        public TipoDB _tipoDB;

        public FabricRepository(string cadenaDeConexion, TipoDB tipoDB)
        {
            _cadenaDeConexion = cadenaDeConexion;
            _tipoDB = tipoDB;
        }
        public IDB<ajuste_usuario> AjustesUsuarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<ajuste_usuario>(_cadenaDeConexion, new AjusteUsuarioValidator(), "id_ajuste", true);
                case TipoDB.MySQL:
                    return new MySQL<ajuste_usuario>(_cadenaDeConexion, new AjusteUsuarioValidator(), "id_ajuste", false);
                case TipoDB.SQLServer:
                    return new SQLServer<ajuste_usuario>(_cadenaDeConexion, new AjusteUsuarioValidator(), "id_ajuste", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<amigo> AmigoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<amigo>(_cadenaDeConexion, new AmigoValidator(), "id_amigo", true);
                case TipoDB.MySQL:
                    return new MySQL<amigo>(_cadenaDeConexion, new AmigoValidator(), "id_amigo", false);
                case TipoDB.SQLServer:
                    return new SQLServer<amigo>(_cadenaDeConexion, new AmigoValidator(), "id_amigo", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<comentario> ComentarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<comentario>(_cadenaDeConexion, new ComentarioValidator(), "id_comentario", true);
                case TipoDB.MySQL:
                    return new MySQL<comentario>(_cadenaDeConexion, new ComentarioValidator(), "id_comentario", false);
                case TipoDB.SQLServer:
                    return new SQLServer<comentario>(_cadenaDeConexion, new ComentarioValidator(), "id_comentario", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<favorito> FavoritoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<favorito>(_cadenaDeConexion, new FavoritoValidator(), "id_favorito", true);
                case TipoDB.MySQL:
                    return new MySQL<favorito>(_cadenaDeConexion, new FavoritoValidator(), "id_favorito", false);
                case TipoDB.SQLServer:
                    return new SQLServer<favorito>(_cadenaDeConexion, new FavoritoValidator(), "id_favorito", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<foto> FotoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<foto>(_cadenaDeConexion, new FotoValidator(), "id_foto", true);
                case TipoDB.MySQL:
                    return new MySQL<foto>(_cadenaDeConexion, new FotoValidator(), "id_foto", false);
                case TipoDB.SQLServer:
                    return new SQLServer<foto>(_cadenaDeConexion, new FotoValidator(), "id_foto", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<alerta> RuidoAlertaRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<alerta>(_cadenaDeConexion, new AlertaValidator(), "id_alerta", true);
                case TipoDB.MySQL:
                    return new MySQL<alerta>(_cadenaDeConexion, new AlertaValidator(), "id_alerta", false);
                case TipoDB.SQLServer:
                    return new SQLServer<alerta>(_cadenaDeConexion, new AlertaValidator(), "id_alerta", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<usuario> UsuarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<usuario>(_cadenaDeConexion, new UsuarioValidator(), "id_usuario", true);
                case TipoDB.MySQL:
                    return new MySQL<usuario>(_cadenaDeConexion, new UsuarioValidator(), "id_usuario", false);
                case TipoDB.SQLServer:
                    return new SQLServer<usuario>(_cadenaDeConexion, new UsuarioValidator(), "id_usuario", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }


        public IDB<publicacion> PublicacionRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<publicacion>(_cadenaDeConexion, new PublicacionValidator(), "id_publicacion", true);
                case TipoDB.MySQL:
                    return new MySQL<publicacion>(_cadenaDeConexion, new PublicacionValidator(), "id_publicacion", false);
                case TipoDB.SQLServer:
                    return new SQLServer<publicacion>(_cadenaDeConexion, new PublicacionValidator(), "id_publicacion", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }
    }

    public enum TipoDB
    {
        Postgress,
        MySQL,
        SQLServer
    }
}
