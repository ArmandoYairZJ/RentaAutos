using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using RentaAutos;

namespace RentaAutos
{
    public class OpMongo
    {
    
     private IMongoDatabase _database;


     public OpMongo(string conex, string namedb)
        { 
        var client = new MongoClient(conex);
        _database = client.GetDatabase(namedb);
        

        }

        public IMongoCollection<T> GetCollection<T>( string Collecion)
        {
            return _database.GetCollection<T>(Collecion);
        }
    
    
    }



}
