using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Dto
{
    internal abstract class DTO
    {
        private long _id;
        private DateTime _date;
        protected DTO(long id, DateTime date) 
        {
            _id = id;
            _date = date;
        }
    }


    internal class BackendUserSessionDTO : DTO
    {
        public string Session => _session;
        public string UserJson => _userJson;

        private readonly string _session;
        private readonly string _userJson;

        public BackendUserSessionDTO(
            long userId,
            string session,
            DateTime date,
            object user
        )
        : base(userId, date)
        {
            _session = session;
            _userJson = JsonConvert.SerializeObject(user);
        }

        [JsonConstructor]
        protected BackendUserSessionDTO(
            long id,
            string session,
            DateTime date,
            string userJson)
            : base(id, date)
        {
            _session = session;
            _userJson = userJson;
        }
    }
}
