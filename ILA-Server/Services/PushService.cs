using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using ILA_Server.Data;
using ILA_Server.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

            PushTokens pushToken = await _context.PushTokens.Where(x => x.DeviceId == deviceId).SingleOrDefaultAsync();

            if (pushToken == null)
            {
                pushToken = new PushTokens { DeviceId = deviceId, User = user, Token = token };
                await _context.PushTokens.AddAsync(pushToken);
            }
            else
            {
                pushToken.Token = token;
                pushToken.User = user;
                _context.PushTokens.Update(pushToken);
            }

            await _context.SaveChangesAsync();
        }

    }
}
