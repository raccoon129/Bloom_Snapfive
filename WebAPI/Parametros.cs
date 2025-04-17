using DAL;

namespace WebAPI
{
    public static class Parametros
    {
        //Conexión a la base de datos

#if DEBUG
        // Mientras sigue en debug
        public static string CadenaDeConexion = @"Server=localhost;Database=bloom_app;Uid=root;Pwd=;";
        public static TipoDB TipoDB = TipoDB.MySQL;
#else
        //En producción con Release

        //Postgres
        //public static string CadenaDeConexion = @"Host=db.vjtcjxyqydppjaxxxnxn.supabase.co;Database=postgres;Username=postgres;Password=MBnd~7SDciN&Xr(;SSL Mode=Require;Trust Server Certificate=true";

        //Server
        public static string CadenaDeConexion = @"Server=db17450.databaseasp.net; Database=db17450; User Id=db17450; Password=4e?WK6x+_fP5; Encrypt=False; MultipleActiveResultSets=True;";

        //MySQL AIVEN
        //public static string CadenaDeConexion = @"Server=mysql-bloom-username818921.i.aivencloud.com;Database=defaultdb;Uid=avnadmin;Pwd=AVNS_ivcCkK8_ifPvFH_uuR7;";
        
        
        //mysql monster
        //public static string CadenaDeConexion = @"Server=mysql-bloom-username818921.i.aivencloud.com;Database=defaultdb;Uid=avnadmin;Pwd=AVNS_ivcCkK8_ifPvFH_uuR7;";
        

        public static TipoDB TipoDB = TipoDB.SQLServer;

#endif
        public static FabricRepository FabricaRepository = new FabricRepository(CadenaDeConexion, TipoDB);
        //public static FabricRepository FabricaRepository = new FabricRepository(CadenaDeConexion, TipoDB);
    }
}

