﻿using System;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ILA_Server.Areas.Identity.Services
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(context, exception);
            }
        }

        private static Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            var payload = exception is UserException userException
                ? JsonConvert.SerializeObject(new { errors = userException.UserErrors })
                : (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development") ?
                    JsonConvert.SerializeObject(new { errors = exception }) 
                    : JsonConvert.SerializeObject(new { errors = new UserException("Unexpected error occurred").UserErrors });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception is UserException userException1 ? userException1.Code : 400;

            return context.Response.WriteAsync(payload);
        }
    }
}
