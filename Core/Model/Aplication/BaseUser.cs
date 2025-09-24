
using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson;

namespace Core.Model.Aplication
{
    public class BaseUser : MongoUser<string>
    {
    }
}
