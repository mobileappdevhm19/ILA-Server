﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Data;
using ILA_Server.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ILA_Server.Services
{
    public interface IPushService
    {
        Task<PushTokens> SaveToken(string userId, string token, string deviceId);

        Task DeleteToken(string userId, string deviceId);
    }

    public class PushService : IPushService
    {
        private readonly ILADbContext _context;

        public PushService(ILADbContext context) : base()
        {
            _context = context;
        }


        public async Task<PushTokens> SaveToken(string userId, string token, string deviceId)
        {
            ILAUser user = await _context.Users.FindAsync(userId);

            PushTokens pushToken = await _context.PushTokens.Where(x => x.DeviceId == deviceId).SingleOrDefaultAsync(x => x.User.Id == userId);

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

            pushToken.User = null;

            return pushToken;
        }

        public async Task DeleteToken(string userId, string deviceId)
        {
            PushTokens pushToken = await _context.PushTokens.Where(x => x.DeviceId == deviceId).SingleOrDefaultAsync(x => x.User.Id == userId);
            if (pushToken == null)
            {
                throw new UserException(404);
            }

            _context.PushTokens.Remove(pushToken);
            await _context.SaveChangesAsync();
        }
    }
}
