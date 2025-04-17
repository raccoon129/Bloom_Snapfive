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
        public IDB<Ajuste_Usuario> AjustesUsuarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<Ajuste_Usuario>(_cadenaDeConexion, new AjusteUsuarioValidator(), "IdAjuste", true);
                case TipoDB.MySQL:
                    return new MySQL<Ajuste_Usuario>(_cadenaDeConexion, new AjusteUsuarioValidator(), "IdAjuste", false);
                case TipoDB.SQLServer:
                    return new SQLServer<Ajuste_Usuario>(_cadenaDeConexion, new AjusteUsuarioValidator(), "IdAjuste", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<Amigo> AmigoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<Amigo>(_cadenaDeConexion, new AmigoValidator(), "IdAmigo", true);
                case TipoDB.MySQL:
                    return new MySQL<Amigo>(_cadenaDeConexion, new AmigoValidator(), "IdAmigo", false);
                case TipoDB.SQLServer:
                    return new SQLServer<Amigo>(_cadenaDeConexion, new AmigoValidator(), "IdAmigo", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<Comentario> ComentarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<Comentario>(_cadenaDeConexion, new ComentarioValidator(), "IdComentario", true);
                case TipoDB.MySQL:
                    return new MySQL<Comentario>(_cadenaDeConexion, new ComentarioValidator(), "IdComentario", false);
                case TipoDB.SQLServer:
                    return new SQLServer<Comentario>(_cadenaDeConexion, new ComentarioValidator(), "IdComentario", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<Favorito> FavoritoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<Favorito>(_cadenaDeConexion, new FavoritoValidator(), "IdFavorito", true);
                case TipoDB.MySQL:
                    return new MySQL<Favorito>(_cadenaDeConexion, new FavoritoValidator(), "IdFavorito", false);
                case TipoDB.SQLServer:
                    return new SQLServer<Favorito>(_cadenaDeConexion, new FavoritoValidator(), "IdFavorito", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<Foto> FotoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<Foto>(_cadenaDeConexion, new FotoValidator(), "IdFoto", true);
                case TipoDB.MySQL:
                    return new MySQL<Foto>(_cadenaDeConexion, new FotoValidator(), "IdFoto", false);
                case TipoDB.SQLServer:
                    return new SQLServer<Foto>(_cadenaDeConexion, new FotoValidator(), "IdFoto", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<Ruido_Alerta> RuidoAlertaRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<Ruido_Alerta>(_cadenaDeConexion, new RuidoAlertaValidator(), "IdAlerta", true);
                case TipoDB.MySQL:
                    return new MySQL<Ruido_Alerta>(_cadenaDeConexion, new RuidoAlertaValidator(), "IdAlerta", false);
                case TipoDB.SQLServer:
                    return new SQLServer<Ruido_Alerta>(_cadenaDeConexion, new RuidoAlertaValidator(), "IdAlerta", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<Usuario> UsuarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.Postgress:
                    return new Postgress<Usuario>(_cadenaDeConexion, new UsuarioValidator(), "IdUsuario", true);
                case TipoDB.MySQL:
                    return new MySQL<Usuario>(_cadenaDeConexion, new UsuarioValidator(), "IdUsuario", false);
                case TipoDB.SQLServer:
                    return new SQLServer<Usuario>(_cadenaDeConexion, new UsuarioValidator(), "IdUsuario", true);
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
