using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using USSDApp.Common.Constants;
using USSDApp.Data;
using USSDTest.Models;

namespace USSDApp.Services;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public UserService(AppDbContext context, IMemoryCache cache)
    {
        (_context, _cache) = (context, cache);
    }

    public async ValueTask<User?> GetUserFromPhoneNumber(string phoneNumber)
    {
        var key = CacheKeys.User(phoneNumber);

        if (_cache.TryGetValue(key, out User cachedUser))
            return cachedUser;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

        return _cache.Set(key, user);
    }
}