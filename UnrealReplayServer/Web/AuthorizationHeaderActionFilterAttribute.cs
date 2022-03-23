using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using UnrealReplayServer.Connectors;
using UnrealReplayServer.Databases.Models;

namespace UnrealReplayServer.Web
{
    public class AuthorizationHeaderActionFilterAttribute : ActionFilterAttribute
    {
        private readonly ApplicationDefaults _applicationDefaults;

        private readonly MongoClient client;
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<AuthorizationHeader> AuthHeaderList;

        public AuthorizationHeaderActionFilterAttribute(IOptions<ApplicationDefaults> options)
        {
            _applicationDefaults = options.Value;

            string ConnectionString = _applicationDefaults.MongoDB.bUseEnvVariable_Connection ? Environment.GetEnvironmentVariable("MONGO_CON_URL") : _applicationDefaults.MongoDB.MongoDBConnection;
            string DatabaseName = _applicationDefaults.MongoDB.bUseEnvVariable_DatabaseName ? Environment.GetEnvironmentVariable("MONGO_DB_NAME") : _applicationDefaults.MongoDB.MongoDBDatabaseName;

            client = new MongoClient(ConnectionString);
            database = client.GetDatabase(DatabaseName);
            AuthHeaderList = database.GetCollection<AuthorizationHeader>("AuthHeaderList");
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_applicationDefaults.bUseAuthorizationHeader)
            {
                base.OnActionExecuting(context);
                return;
            }

            string AuthorizationHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

            var filter = Builders<AuthorizationHeader>.Filter.And(
                Builders<AuthorizationHeader>.Filter.Eq(x => x.AuthorizationHeaderValue, AuthorizationHeader),
                Builders<AuthorizationHeader>.Filter.Or(
                    Builders<AuthorizationHeader>.Filter.Eq(x => x.bUseRemainingUse, false),
                    Builders<AuthorizationHeader>.Filter.Lt(x => x.RemainingUse, 0)
                )
            );

            var result = await AuthHeaderList.Find(filter).ToListAsync();

            
        }
    }
}
