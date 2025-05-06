using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using TiengAnh.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace TiengAnh.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbService> _logger;
        private static readonly ConcurrentDictionary<string, bool> _registeredCollections = new ConcurrentDictionary<string, bool>();
        private static bool _conventionsRegistered = false;

        public MongoDbService(IOptions<MongoDbSettings> settings, ILogger<MongoDbService> logger)
        {
            _logger = logger;
            
            try
            {
                RegisterConventions();
                
                var client = new MongoClient(settings.Value.ConnectionString);
                _database = client.GetDatabase(settings.Value.DatabaseName);
                
                _logger.LogInformation($"MongoDB connected to database: {settings.Value.DatabaseName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize MongoDB: {ex.Message}");
                throw;
            }
        }
        
        private void RegisterConventions()
        {
            if (_conventionsRegistered) return;
            
            lock (typeof(MongoDbService))
            {
                if (_conventionsRegistered) return;
                
                try
                {
                    // Register conventions
                    var pack = new ConventionPack
                    {
                        new IgnoreExtraElementsConvention(true),
                        new CamelCaseElementNameConvention()
                    };
                    ConventionRegistry.Register("AppConventions", pack, t => true);
                    
                    _conventionsRegistered = true;
                    _logger.LogInformation("MongoDB conventions registered successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to register MongoDB conventions: {ex.Message}");
                    throw;
                }
            }
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            try
            {
                // Register class map if needed for this type
                RegisterClassMap<T>();
                
                return _database.GetCollection<T>(name);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get collection {name}: {ex.Message}");
                throw new Exception($"Failed to get collection {name}: {ex.Message}", ex);
            }
        }
        
        private void RegisterClassMap<T>()
        {
            var typeName = typeof(T).FullName;
            
            if (_registeredCollections.ContainsKey(typeName))
            {
                return; // Already registered
            }
            
            // Try to register the type
            try
            {
                // Only register if not already registered
                if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                {
                    // For UserModel, we need special handling
                    if (typeof(T) == typeof(UserModel))
                    {
                        BsonClassMap.RegisterClassMap<UserModel>(cm =>
                        {
                            cm.AutoMap();
                            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                            cm.IdMemberMap.SetElementName("_id");
                        });
                        _logger.LogInformation("Registered custom class map for UserModel");
                    }
                    // For BaseModel, map the ID explicitly
                    else if (typeof(T) == typeof(BaseModel))
                    {
                        BsonClassMap.RegisterClassMap<BaseModel>(cm =>
                        {
                            cm.AutoMap();
                            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                            cm.IdMemberMap.SetElementName("_id");
                        });
                        _logger.LogInformation("Registered custom class map for BaseModel");
                    }
                    // For other types, use the default mapping
                    else
                    {
                        BsonClassMap.RegisterClassMap<T>(cm =>
                        {
                            cm.AutoMap();
                        });
                        _logger.LogInformation($"Registered default class map for {typeName}");
                    }
                }
                
                _registeredCollections[typeName] = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to register class map for {typeName}: {ex.Message}");
                throw;
            }
        }
    }

    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
