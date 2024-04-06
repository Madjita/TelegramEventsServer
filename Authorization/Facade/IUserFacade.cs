using Authorization;
using Authorization.Dto;
using Couchbase.KeyValue;
using CQRS;
using CQRS.Query;
using DataBase.Coutchbase;
using MediatR;
using Newtonsoft.Json;
using DataBase.Entities.Entities_DBContext;

namespace TelegramEvents.Fasad
{
    public interface IUserFacade
    {
        Task<User?> LogInAsync(string sessionId);
        Task<(bool succes, string session, User? user)> AuthenticateAsync(LoginDto login_request);
        Task<(bool succes, string session, User? user)> AuthenticateTelegramAsync(string requestLoginTelegram);
        Task LogoutAsync(string sessionId);

    }

    public class UserFacade : IUserFacade
    {
        private readonly ICouchBaseAdapter _couchBaseAdapter;
        private readonly IMediator _mediator;
        public UserFacade(IMediator mediator,ICouchBaseAdapter couchBaseAdapter)
        {
            _couchBaseAdapter = couchBaseAdapter;
            _mediator = mediator;
        }

        private string GetUserKey(int id) => $"UserId_{id}";
        private string GetSessionKey(string sessionId) => $"Session_{sessionId}";

        public async Task<(bool succes, string session, User? user)> AuthenticateAsync(LoginDto login_request)
        {
            //TODO найти пользователя в NoSql базе
            var response = await CheckSessionInRepository(login_request);

            if (response.Success)
            {
                //Если в базе данных он есть значит пользователь был зарегистрирован.
                //Создаем сессию на вход
                string session = Guid.NewGuid().ToString("N");
                try
                {
                    InsertOptions insertOption = new InsertOptions();
                    insertOption.Expiry(TimeSpan.FromDays(1));

                    var userID = response.userSession!.BackendUser.Id;

                    var userKey = GetUserKey(userID);
                    var sessionKey = GetSessionKey(session);

                    //Проверяем были ли пользователь уже добавлен?
                    var checkUserKey = await _mediator.Send(new ExistsCommandCoutchBase { Key = userKey });

                    if (checkUserKey)
                    {
                        var userKeyLock = await _mediator.Send(new GetAndLockQueryCoutchBase { Key = userKey, lockTimeSpan = TimeSpan.FromSeconds(30) });
                        var json = userKeyLock?.ContentAs<string>();
                        var userKeyData = JsonConvert.DeserializeObject<LoginDtoCautchbase>(json);

                        if (userKeyData is not null)
                        {
                            userKeyData.InsertSession(session);
                            userKeyData.User = response.userSession!.BackendUser;

                            json = JsonConvert.SerializeObject(userKeyData, new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });

                            var upsertOptions = new UpsertOptions();
                            upsertOptions.Expiry(TimeSpan.FromDays(1));

                            var updateTTL = await _mediator.Send(new UpserWithUnlockCommandCoutchBase { Key = userKey, Json = json, lockItem = userKeyLock, Options = upsertOptions });

                        }
                    }
                    else
                    {
                        string? json = JsonConvert.SerializeObject(new LoginDtoCautchbase(session, response.userSession!.BackendUser), new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                        await _mediator.Send(new InsertCommandCoutchBase { Key = userKey, Json = json, Options = insertOption });
                    }

                    await _couchBaseAdapter.InsertAsync(sessionKey, userKey, insertOption);

                    return (true, session, response.userSession!.BackendUser);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            }


            return (false, "", null);
        }

        private async Task<(bool Success, BackendUserSession? userSession)> CheckSessionInRepository(LoginDto loginDto)
        {
            //Проверям сессию в Repository
            var userSessionFromRepos =  await _mediator.Send(new GetUserByEmailORLoginQuery { Email = loginDto.Email, HPassword = loginDto.HPassword });

            if (userSessionFromRepos is not null)
            {
                var userSession = new BackendUserSession()
                {
                    BackendUser = userSessionFromRepos,
                };

                return (true, userSession);
            }

            return (false, null);
        }

        public async Task<(bool succes, string session, User? user)> AuthenticateTelegramAsync(string requestLoginTelegram)
        {
            var CheckUserSession = false;

            //Ищем авторизованного телеграм юзера.
            var telegramUser = await _mediator.Send(new GetUserByEmailORLoginQuery { Email = requestLoginTelegram });

            if (telegramUser is not null)
            {
                var result = await AuthenticateAsync(
                    new LoginDto(telegramUser.UserName, "")
                    {
                        Email = telegramUser.Email,
                        Password = "",
                        HPassword = telegramUser.HPassword,
                    }
                );

                return (result.succes, result.session, result.user);
            }


            return (false, "", null);
        }

        public async Task<User?> LogInAsync(string sessionId)
        {
            try
            {
                var sessionKey = await _mediator.Send(new GetAndTouchQueryCoutchBase { Key = GetSessionKey(sessionId), updateTimeLife = TimeSpan.FromHours(1) });
                
                if(sessionKey is null)
                {
                    return null;
                }
                
                var userKey = sessionKey.ContentAs<string>();

                //берем пользователя
                var userKeyData = await _mediator.Send(new GetQueryCoutchBase { Key = userKey });
                if(userKeyData is null)
                {
                    return null;
                }

                var json = userKeyData?.ContentAs<string>();
                var userSession_cash = JsonConvert.DeserializeObject<LoginDtoCautchbase>(json);
                
                if (userSession_cash is not null)
                {
                    return userSession_cash.User;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task LogoutAsync(string sessionId)
        {
            var sessionKey = await _couchBaseAdapter.GetAsync(GetSessionKey(sessionId));
            var userKey = sessionKey?.ContentAs<string>();

            if (userKey is null)
            {
                return;
            }

            //берем пользователя
            var userKeyData = await _couchBaseAdapter.GetAndLockAsync(userKey, TimeSpan.FromSeconds(30));

            var json = userKeyData?.ContentAs<string>();
            var userSession_cash = JsonConvert.DeserializeObject<LoginDtoCautchbase>(json);

            userSession_cash.Sessions.Remove(sessionId);

            json = JsonConvert.SerializeObject(userSession_cash, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            var upsertOptions = new UpsertOptions();
            upsertOptions.Expiry(TimeSpan.FromDays(1));

            await _couchBaseAdapter.UnlockAsync(userKey, userKeyData.Cas);
            await _couchBaseAdapter.UpsertAsync(userKey, json, upsertOptions);

            if (userSession_cash.Sessions.Count <= 0)
            {
                await _couchBaseAdapter.RemoveAsync(userKey);
            }

            await _couchBaseAdapter.RemoveAsync($"Session_{sessionId}");
        }
    }
}
