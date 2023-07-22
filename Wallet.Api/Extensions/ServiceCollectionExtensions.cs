using FluentValidation;
using Npgsql;
using StackExchange.Redis;
using System.Data;
using WalletApi.Models;
using WalletApi.Options;
using WalletApi.Repositories;
using WalletApi.Services;
using WalletApi.Validators;
using Transaction = WalletApi.Models.Transaction;

namespace WalletApi.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddWalletServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IValidator<TransactionRequest>, TransactionRequestValidator>();
        services.AddScoped<IValidator<LockFundsTransactionRequest>, LockFundsTransactionRequestValidator>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserCacheService, UserCacheService>();
        services.AddScoped<ITransactionIdempotentService, TransactionIdempotentService>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<WalletValidatorService>();
        services.AddScoped<IWalletService, WalletService>();
        return services;
    }

    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Redis").Get<RedisConfiguration>();
        var redisOptions = ConfigurationOptions.Parse(redisConfig.ConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig.ConnectionString;
            options.InstanceName = redisConfig.InstanceName;
        });

        return services;
    }

    public static IServiceCollection AddDbConnection(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("wallet");
        services.AddScoped<IDbConnection>(provider => new NpgsqlConnection(connectionString));

        return services;
    }
}