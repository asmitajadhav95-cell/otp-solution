using System.Collections.Concurrent;
using Otp.Application.Abstractions;
using Otp.Domain.Entities;

namespace Otp.Infrastructure.Persistence;

// Thread-safe dictionary. Swap for a RedisOtpRepository later — nothing else changes.
public sealed class InMemoryOtpRepository : IOtpRepository
{
    private readonly ConcurrentDictionary<string, OneTimePassword> _store =
        new(StringComparer.OrdinalIgnoreCase);

    public void Save(OneTimePassword otp) => _store[otp.UserId] = otp;

    public OneTimePassword? Get(string userId) =>
        _store.TryGetValue(userId, out var otp) ? otp : null;

    public void Remove(string userId) => _store.TryRemove(userId, out _);
}