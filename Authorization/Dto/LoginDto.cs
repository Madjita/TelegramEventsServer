using DataBase.Entities.Entities_DBContext;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataBase.Entities.Entities_DBContext;
using Utils;

namespace Authorization.Dto
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string HPassword { get; set; }

        public LoginDto(string email, string password)
        {
            Email = email;
            Password = password;
            HPassword = MyCrypt.GetHash(password, email);
        }
    }

    public class LoginDtoCautchbase
    {
        public User User { get; set; }
        public List<string> Sessions { get; private set; } = new List<string>();

        public LoginDtoCautchbase(string session, User user)
        {
            Sessions.Add(session);
            User = user;
        }

        [JsonConstructor]
        public LoginDtoCautchbase(User user, List<string> sessions)
        {
            User = user;
            Sessions = sessions;
        }

        public void InsertSession(string session)
        {
            Sessions.Add(session);
        }
    }

}
