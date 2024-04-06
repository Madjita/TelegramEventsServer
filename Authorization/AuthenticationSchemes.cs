using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization;

public static class AuthenticationSchemes
{
    public const string Default = "Default";
    public const string JsonWebToken = "JsonWebToken";
    public const string AccessToken = "AccessToken";
    public const string SessionCookie = "SessionCookie";
}