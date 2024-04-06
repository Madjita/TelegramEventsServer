using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using AuthDAL.Entities;
using Authorization.Dto;
using DataBase;
using DataBase.Contexts;
//using DataBase.Repository.Base;
using MyLoggerNamespace.Enums;
using MyLoggerNamespace;
using Newtonsoft.Json;
using Utils;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DataBase.Entities.Entities_DBContext;
//using DataBase.PaternRepositoryBase.Repository;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using Couchbase.Extensions.DependencyInjection;
using Authorization;

namespace Authorization
{
    public class BackendUserSession
    {
        public DataBase.Entities.Entities_DBContext.User BackendUser { get; set; }
        public DateTime? LastAccess { get; set; }
        public string SessionId { get; set; }
    }

    /*
    public static class BackendApiSession
    {
        private static readonly string _USER_SESSION_NAME = "UserSession";
        private static IMyLogger _logger = new MyLoggerNamespace.Logger(typeof(BackendApiSession).Name);

        public static string USER_SESSION_NAME => _USER_SESSION_NAME;

        #region Sessions
        /// <summary>
        /// Delete session in Repository.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private static bool DelecteSessionInRepository(BackendUserSession session)
        {
            var dataManager = (IRepositoryBase<AuthDbContext, User>)DataManager.GetUserRepository();
            dataManager.DeleteAndCommit(session.BackendUser);

            return true;
        }

        /// <summary>
        /// Check session in Repository.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private static bool CheckSessionInRepository(string key, out BackendUserSession? userSession)
        {
            var dataManager = (IRepositoryBase<AuthDbContext, User>)DataManager.GetUserRepository();

            userSession = null;
            //Проверям сессию в Repository
            var userSessionFromRepos = dataManager.FirstOrDefault(_ => _.Session == key);

            if (userSession is not null)
            {
                userSession = new BackendUserSession()
                {
                    BackendUser = userSessionFromRepos,
                    LastAccess = userSessionFromRepos.LastAccess,
                    SessionId = userSessionFromRepos.Session
                };

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check session in Repository.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private static bool CheckSessionInRepository(LoginDto loginDto, out BackendUserSession? userSession)
        {
            var dataManager = DataManager.GetUserRepository();

            userSession = null;

            //Проверям сессию в Repository
            var userSessionFromRepos = dataManager.FirstOrDefault(_ => _.Login == loginDto.Login && _.HPassword == loginDto.HPassword);

            if (userSession is not null)
            {
                userSession = new BackendUserSession()
                {
                    BackendUser = userSessionFromRepos,
                    LastAccess = userSessionFromRepos.LastAccess,
            };

                return true;
            }

            return false;
        }

        public static BackendUserSession? GetUserSession(LoginDto loginDto)
        {
            BackendUserSession? userSession;

            //идем в CoutchBase
            if (CheckSessionInCoutchBase(loginDto, out var sessionCash))
            {
                userSession = new BackendUserSession
                {
                    BackendUser = sessionCash.User,
                    SessionId = sessionId
                };
                return userSession;
            }

            //Идем в Repository
            if (CheckSessionInRepository(loginDto, out userSession))
            {
                //Если там есть то сохраняем в память.
                //AddSessionInMemory(sessionId, userSession!);
            }

            return userSession;
        }

        /// <summary>
        /// Get User session from memory or cautchBase.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static BackendUserSession? GetUserSession(string sessionId)
        {

            BackendUserSession? userSession;

            //идем в CoutchBase
            if (CheckSessionInCoutchBase(sessionId, out var sessionCash))
            {
                userSession = new BackendUserSession
                {
                    BackendUser = sessionCash.User,
                    SessionId = sessionId
                };
                return userSession;
            }

            //Если в CoutchBase нет, то идем в Repository
            if (CheckSessionInRepository(sessionId, out userSession))
            {
                return userSession;
            }

            return userSession;
        }

        private static bool CheckSessionInCoutchBase(LoginDto reqest, out LoginDtoCautchbase? userSession)
        {
            var _couchbaseService = DataManager.GetCouchbaseService();

            var key = $"Session_{sessionId}";

            LoginDtoCautchbase? userSession_cash = null;

            try
            {
                userSession_cash = (_couchbaseService.GetAsync(key).ConfigureAwait(false).GetAwaiter().GetResult())?.ContentAs<LoginDtoCautchbase>();
            }
            catch (Exception ex)
            {

            }

            if (userSession_cash is not null)
            {
                userSession = userSession_cash;
                return true;
            }

            throw new NotImplementedException();
        }

        private static bool CheckSessionInCoutchBase(string sessionId, out LoginDtoCautchbase? userSession)
        {
            var _couchbaseService = DataManager.GetCouchbaseService();

            var key = $"Session_{sessionId}";

            LoginDtoCautchbase? userSession_cash = null;

            try
            {
                userSession_cash = (_couchbaseService.GetAsync(key).ConfigureAwait(false).GetAwaiter().GetResult())?.ContentAs<LoginDtoCautchbase>();
            }
            catch (Exception ex)
            {

            }

            if (userSession_cash is not null)
            {
                userSession = userSession_cash;
                return true;
            }

            throw new NotImplementedException();
        }


        /// <summary>
        /// Delete User session from memory and cautchBase.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static bool DeleteUserSession(string sessionId)
        {
            var userSession = GetUserSession(sessionId);

            if (userSession is null)
            {
                return false;
            }

            var deleteFromCautchBase = DelecteSessionInRepository(userSession);

            return deleteFromCautchBase;
        }

        /// <summary>
        /// Create User session for memory and cautchBase.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static string? CreateBackendUserSession(User backendUser, string IPFrom)
        {
            if (backendUser != null)
            {
                string? sessionId;
                BackendUserSession? session;

                //Проверяем юзера в  CautchBase
                if (CheckSessionInRepository(backendUser.Id.ToString(), out session))
                {
                    sessionId = session!.SessionId;
                }
                else
                {
                    //SessionId тут
                    sessionId = Guid.NewGuid().ToString();

                    backendUser.SetLastAccess();

                    session = new BackendUserSession()
                    {
                        BackendUser = backendUser,
                        LastAccess = DateTime.Now,
                        SessionId = sessionId,
                    };

                    var dataManager = DataManager.GetUserRepository();
                    dataManager.UpdateUser(session.BackendUser);

                }

                return sessionId;
            }
            else
            {
                _logger.WriteLine(MessageType.Error, "[CreateBackendUserSession] error: [{0}] from IP = [{1}]", "object BackendUser is null", IPFrom);
                return null;
            }
        }

        public static bool GetUserSessionFromTelegram(string requestLoginTelegram, out LoginDtoCautchbase? userSession)
        {
            var dataManager = DataManager.GetTelegramRepository();
            var dataManager2 = DataManager.GetUserRepository();


            userSession = null;
            //Проверям сессию в Repository
            var telegramUser = dataManager.FirstOrDefault(_ => _.Username == requestLoginTelegram);


            if (telegramUser is not null)
            {
                var user = dataManager2.FirstOrDefault(_ => _.Id == telegramUser.UserId);

                userSession = new LoginDtoCautchbase(Guid.NewGuid().ToString("N"), user.Login, user.HPassword);

                return true;
            }

            return false;
        }
        #endregion
    }
    */
}
