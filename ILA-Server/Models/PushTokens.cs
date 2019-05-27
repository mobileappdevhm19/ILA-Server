using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILA_Server.Models
{
    public class PushTokens
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public string DeviceId { get; set; }

        public ILAUser User { get; set; }
    }
}
