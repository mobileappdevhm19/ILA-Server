using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ILA_Server.Areas.Identity.Models
{
    public class UserException : Exception
    {
        public List<string> UserErrors { get; }

        private UserException()
        {
            UserErrors = new List<string>();
        }

        public UserException(string[] userErrors) : this()
        {
            if (userErrors != null && userErrors.Length != 0)
            {
                UserErrors.AddRange(userErrors);
            }
        }

        public UserException(string userError) : this()
        {
            if (!string.IsNullOrWhiteSpace(userError))
            {
                UserErrors.Add(userError);
            }
        }

        public UserException(List<string> userErrors) : this()
        {
            if (userErrors != null && userErrors.Count != 0)
            {
                UserErrors.AddRange(userErrors);
            }
        }

        public UserException(ModelStateDictionary modelState) : this()
        {
            UserErrors.AddRange(modelState
                .Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .SelectMany(z => z)
                .Select(e => e.ErrorMessage));
        }

        public UserException(IdentityResult result) : this()
        {
            UserErrors.AddRange(result.Errors.Select(x => x.Description));
        }
    }
}
