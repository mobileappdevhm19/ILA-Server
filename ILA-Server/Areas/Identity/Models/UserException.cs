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
        public int Code { get; }

        public UserException(int code = 400)
        {
            UserErrors = new List<string>();
            Code = code;
        }

        public UserException(string[] userErrors,int code = 400) : this(code)
        {
            if (userErrors != null && userErrors.Length != 0)
            {
                UserErrors.AddRange(userErrors);
            }
        }

        public UserException(string userError, int code = 400) : this(code)
        {
            if (!string.IsNullOrWhiteSpace(userError))
            {
                UserErrors.Add(userError);
            }
        }

        public UserException(List<string> userErrors, int code = 400) : this(code)
        {
            if (userErrors != null && userErrors.Count != 0)
            {
                UserErrors.AddRange(userErrors);
            }
        }

        public UserException(ModelStateDictionary modelState, int code = 400) : this(code)
        {
            UserErrors.AddRange(modelState
                .Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .SelectMany(z => z)
                .Select(e => e.ErrorMessage));
        }

        public UserException(IdentityResult result, int code = 400) : this(code)
        {
            UserErrors.AddRange(result.Errors.Select(x => x.Description));
        }
    }
}
