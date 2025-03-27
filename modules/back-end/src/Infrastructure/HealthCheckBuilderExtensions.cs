﻿using Infrastructure.Caches;
using Infrastructure.MQ;
using Infrastructure.MQ.Kafka;
using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class HealthCheckBuilderExtensions
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public const string ReadinessTag = "Readiness";

    public static IHealthChecksBuilder AddReadinessChecks(
        this IHealthChecksBuilder builder,
        IConfiguration configuration)
    {
        var tags = new[] { ReadinessTag };

        var dbProvider = configuration.GetDbProvider();
        if (dbProvider.Name == DbProvider.MongoDb)
        {
            builder.AddMongoDb(
                dbProvider.ConnectionString,
                tags: tags,
                timeout: Timeout
            );
        }
        else
        {
            builder.AddDbContextCheck<AppDbContext>(tags: tags);
        }

        var mqProvider = configuration.GetMqProvider();
        if (mqProvider == MqProvider.Kafka)
        {
            builder.AddKafka(configuration, tags, Timeout);
        }

        var cacheProvider = configuration.GetCacheProvider();
        if (cacheProvider == CacheProvider.Redis)
        {
            var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString")!;
            builder.AddRedis(
                redisConnectionString,
                tags: tags,
                timeout: Timeout
            );
        }

        return builder;
    }
}