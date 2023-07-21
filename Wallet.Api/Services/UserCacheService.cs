using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WalletApi.Models;
using WalletApi.Repositories;

namespace WalletApi.Services;

public class UserCacheService : IUserCacheService
{
    private readonly IUserRepository _userRepository;
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _options;

    public UserCacheService(IUserRepository userRepository, IDistributedCache cache)
    {
        _userRepository = userRepository;
        _cache = cache;
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<User> GetUserById(long id)
    {
        var cacheKey = $"user:id:{id}";
        var cachedUser = await _cache.GetStringAsync(cacheKey);

        if (cachedUser != null)
        {
            return JsonSerializer.Deserialize<User>(cachedUser, _options);
        }

        var user = await _userRepository.GetUserById(id);

        if (user != null)
        {
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user, _options));
        }

        return user;
    }

    public async Task<User> GetUserByUuid(Guid userUuid)
    {
        var cacheKey = $"user:uuid:{userUuid}";
        var cachedUser = await _cache.GetStringAsync(cacheKey);

        if (cachedUser != null)
        {
            return JsonSerializer.Deserialize<User>(cachedUser, _options);
        }

        var user = await _userRepository.GetUserByUuid(userUuid);

        if (user != null)
        {
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user, _options));
        }

        return user;
    }

    public async Task<User> AddUser(User user)
    {
        var addedUser = await _userRepository.AddUser(user);

        if (addedUser != null)
        {
            await _cache.SetStringAsync($"user:id:{addedUser.Id}", JsonSerializer.Serialize(addedUser, _options));
            await _cache.SetStringAsync($"user:uuid:{addedUser.UserUuid}", JsonSerializer.Serialize(addedUser, _options));
        }
        else
        {
            throw new Exception("Exception in adding User ");
        }

        return addedUser;
    }

    public async Task<bool> DeleteUser(long id)
    {
        var deleted = await _userRepository.DeleteUser(id);

        if (deleted)
        {
            await _cache.RemoveAsync($"user:id:{id}");
            // TODO: Remove user by uuid from cache
        }

        return deleted;
    }

    public async Task<bool> UpdateUser(User user)
    {
        var updated = await _userRepository.UpdateUser(user);

        if (updated)
        {
            await _cache.SetStringAsync($"user:id:{user.Id}", JsonSerializer.Serialize(user, _options));
            await _cache.SetStringAsync($"user:uuid:{user.UserUuid}", JsonSerializer.Serialize(user, _options));
        }

        return updated;
    }

    public async Task<bool> LockUser(long id)
    {
        var locked = await _userRepository.LockUser(id);

        if (locked)
        {
            var user = await GetUserById(id);
            user.Locked = true;

            await _cache.SetStringAsync($"user:id:{id}", JsonSerializer.Serialize(user, _options));
            await _cache.SetStringAsync($"user:uuid:{user.UserUuid}", JsonSerializer.Serialize(user, _options));
        }

        return locked;
    }

    public async Task<bool> UnlockUser(long id)
    {
        var unlocked = await _userRepository.UnlockUser(id);

        if (unlocked)
        {
            var user = await GetUserById(id);
            user.Locked = false;

            await _cache.SetStringAsync($"user:id:{id}", JsonSerializer.Serialize(user, _options));
            await _cache.SetStringAsync($"user:uuid:{user.UserUuid}", JsonSerializer.Serialize(user, _options));
        }

        return unlocked;
    }
}