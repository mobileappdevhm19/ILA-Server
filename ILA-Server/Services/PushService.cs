using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ILA_Server.Data;
using ILA_Server.Models;

namespace ILA_Server.Services
{
    public interface IPushService
    {
        Task SaveToken(string userId, string token, string deviceId);
    }

    public class PushService : IPushService
    {
        private readonly ILADbContext _context;

        public PushService(ILADbContext context) : base()
        {
            _context = context;
        }


        public async Task SaveToken(string userId, string token, string deviceId)
        {
            ILAUser user = await _context.Users.FindAsync(userId);

            PushTokens pushToken = new PushTokens { DeviceId = deviceId, User = user, Token = token };

            await _context.PushTokens.AddAsync(pushToken);
            await _context.SaveChangesAsync();
        }
    }
}
