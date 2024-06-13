using CQRS;
using CQRS.Query;
using CQRS.Query.CustomerCompany;
using DataBase.Entities.Entities_DBContext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyLoggerNamespace;
using Newtonsoft.Json.Bson;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.TelegramBots.MainManager;
using TelegramBot.TelegramBots.Party;
using Utils;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TelegramBot.Facade
{
    public enum TelegramUserState
    {
        None = 0,
        Start,
        StartRegistrationOrganization,


        StartRegistration,
        StartRegistrationFriend,
        WriteFriendAccount,
        StartRegistrationYourSelf,
        WritedFIOandPhoneYourSelf,
        WritedFIOandPhoneFriendNo,
        EditRegistrationFIOandPhone,
        SendedCheckPicture,
        Registrated,
        InternalError,

        RegistrationCustomerCompany,
        RegistrationParty,
        RegistrationTelegramBot,
        
        SelectCompany,
        
        WriteAdministratorNikNameForAddInOrg, // ждем когда пользователь впишет ник нейм администратора
        WriteEvent, // ждем когда пользователь впишет событие для создания


        //weatherTelegramBot
        weather_Registration,
        SubscribeTheWeather
    }

    public enum KeyboardCommand
    {
        PageBotTypePrevPage,
        PageBotTypeNextPage,
        
        PageCompanyPrevPage,
        PageCompanyNextPage,
        
        SelectTelegramBot,
        PageTelegramBotInOrgPrevPage,
        PageTelegramBotInOrgNextPage,
        
        SelectAdministratorInOrg,
        PageAdministratorInOrgPrevPage,
        PageAdministratorInOrgNextPage,
        
        CanselOperation,
        ExitFromBot,
        ExitFromBotYes,
        ExitFromBotNo,
        StartRegistrationCompany, // хотим зарегестрировать компанию
        SelectCompany, //выбрали конкретную уже зарегестрированную компанию
        SelectBotType, //выбрали конкретный тип создаваемого бота.
        SelectAlreadyRegistrationCompany, // выбрали отобразить зарегестрированные компанию относительно текущего пользователя

        StartRegistrationDancer,
        StartRegistrationFriend,
        StartRegistrationFriendYes,
        StartRegistrationFriendNo,
        StartRegistrationYourSelf,
        StartRegistrationNoDancer,
        EditRegistrationFIOandPhone,
        BreakRegistration,
        ApproveRegistrationFIOandPhone,


        //
        StartRegistrationBot,
        StartCreationBot,
        EndRegistrationBot,
        AddAdministratorInOrg, // Добавить администратора на конкретную организацию.
        SettingsAdministratorsInOrg, // Кнопка настроек администраторов в организации
        
        SettingsEvents, // Кнопка настроек событий
        StartCreateEvent, // Добавить событие к конкретному телеграм боту


        //
        ApproveCheckPictureFromMenager,
        FaildCheckPictureFromMenager
    }

    public enum TelegramUserErrorCode
    {
        None = 0,
        MediatorNotFound,
        TelegramUserNotCreated,
        TelegramUserNotExit,
        CustomerCompanyNotCreated,
        TelegramBotNotFound
    }

    public class ResponceTelegramFacade
    {
        public bool Success { get; set; }
        public TelegramUserState State { get; set; }
        public TelegramUserErrorCode ErrorCode { get; set; }
        public string Message { get; set; }
        public InlineKeyboardMarkup? InlineKeyboard { get; set; }

        public ResponceTelegramFacade(TelegramUserErrorCode errorCode)
        {
            State = TelegramUserState.InternalError;
            Success = false;
            ErrorCode = errorCode;
        }

        public ResponceTelegramFacade(TelegramUserState state, bool success)
        {
            State = state;
            Success = success;
            ErrorCode = TelegramUserErrorCode.None;
        }
    }
    
    public interface ITelegramBotFacade
    {
        Task<ResponceTelegramFacade> Start(TelegramBot typeTelegramBot, User requestTelegramUser);

        Task<ResponceTelegramFacade> RegistrationCustomerCompany(User requestTelegramUser, string nameCustomerCompany);

        Task<ResponceTelegramFacade> Exit(TelegramBot typeTelegramBot, User requestTelegramUser);
        Task<ResponceTelegramFacade> ActivateUser(TelegramBot typeTelegramBot, User requestTelegramUser);

        //Test
        Task<List<List<InlineKeyboardButton>>> PageCompanyButtons(IMediator mediator, User? responceUser, int currentPage = 0, int companiesPerPage = 4);
        Task<List<List<InlineKeyboardButton>>> PageBotTypeButtons(IMediator mediator, User? responceUser,int orgId, int currentPage = 0, int companiesPerPage = 4);
        Task<List<List<InlineKeyboardButton>>> PageTelegramBotInOrgButtons(IMediator mediator, User? responceUser, int orgId, int currentPage = 0, int companiesPerPage = 4);
        
        /// <summary>
        /// Получить список кнопок Администраторов в текущей организации
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="responceUser"></param>
        /// <param name="orgId"></param>
        /// <param name="currentPage"></param>
        /// <param name="companiesPerPage"></param>
        /// <returns></returns>
        Task<List<List<InlineKeyboardButton>>> PageAdminUsersInOrgButtons(IMediator mediator, User? responceUser, int orgId, int currentPage = 0, int companiesPerPage = 4);

        //Start buttons
        InlineKeyboardMarkup StartManagerButtons(IMediator mediator, User responceUser, int currentPage = 0, int companiesPerPage = 4);
        Task<User?> FindUser(IMediator mediator, User requestTelegramUser, bool isDeleted = false);
    }

    public class TelegramBotFacade : ITelegramBotFacade
    {
        private readonly IMyLogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public TelegramBotFacade (IMyLogger logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }
        
        public async Task<User?> FindUser(IMediator mediator, User requestTelegramUser, bool isDeleted = false)
        {
            //Ищем пользователя.
            var cqrsUser = new GetUserByChatIdQuery { TelegramChatId = requestTelegramUser.TelegramChatId, IsDeleted = isDeleted };
            var responceUser = await mediator.Send(cqrsUser);

            return responceUser;
        }

        public async Task<ResponceTelegramFacade> Exit(TelegramBot typeTelegramBot, User requestTelegramUser)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetService<IMediator>();
            if (mediator is null)
                return new ResponceTelegramFacade(TelegramUserErrorCode.MediatorNotFound);

            //Ищем пользователя.
            var responceUser = await FindUser(mediator, requestTelegramUser);

            if(responceUser is null)
            {
                return new ResponceTelegramFacade(TelegramUserErrorCode.TelegramUserNotCreated)
                {
                    Message = "Пользователь деактивирован.\n" +
                              "Хотите вернуться в чат? ",
                    InlineKeyboard = new(
                        new[] {
                           new[] { InlineKeyboardButton.WithCallbackData("Нет", Facade.KeyboardCommand.ExitFromBotNo.ToString()) },
                           new[] { InlineKeyboardButton.WithCallbackData("Да", Facade.KeyboardCommand.ExitFromBotYes.ToString()) }
                        }
                   )
                };
            }

            var responseExitFromTelegramBot = await mediator.Send(new ExitFromBotTelegramBotCommand()
            {
                telegramUser = responceUser
            });

            if(responseExitFromTelegramBot)
            {
                return new ResponceTelegramFacade(TelegramUserErrorCode.None)
                {
                    Message = "Пользователь успешно деактивирован"
                };
            }
            else
            {
                return new ResponceTelegramFacade(TelegramUserErrorCode.TelegramUserNotExit)
                {
                    Message = "Пользователь не деактивирован"
                };
            }
        }

        public async Task<ResponceTelegramFacade> RegistrationCustomerCompany(User requestTelegramUser,string nameCustomerCompany)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetService<IMediator>();
            if (mediator is null)
                return new ResponceTelegramFacade(TelegramUserErrorCode.MediatorNotFound);

            //Ищем пользователя.
            var responceUser = await FindUser(mediator, requestTelegramUser);

            var cqrsITem = new UpdateOrgCommand { newCustomerCompany = new Org
            { 
                IsActive = true,
                Name = nameCustomerCompany,
                WhoRegisterdUsserId = responceUser!.Id
            } };

            var responce = await mediator.Send(cqrsITem);

            if (!responce.Success)
            {
                return new ResponceTelegramFacade(TelegramUserErrorCode.CustomerCompanyNotCreated);
            }

            return new ResponceTelegramFacade(TelegramUserState.RegistrationCustomerCompany, true)
            {
                Message = "Компания успешно созданна"
            };
        }

        public async Task<List<List<InlineKeyboardButton>>> PageCompanyButtons(IMediator mediator, User? responceUser, int currentPage = 0, int companiesPerPage = 4)
        {
            var checkOrganizationCommand = new CheckExistWhoCreatedOrgCommand()
            {
                WhoRegisterdUsserId = responceUser!.Id,
                Skip = currentPage * companiesPerPage,
                Take = companiesPerPage
            };

            var responsecheckOrganizationCommand = await mediator.Send(checkOrganizationCommand);

            List<List<InlineKeyboardButton>> buttons = new();
            buttons.Add(new List<InlineKeyboardButton>());
            buttons.Add(new List<InlineKeyboardButton>());
            
            // Заполняем кнопки для выбора организаций на текущей странице
            void FillButtonsForPage(int page)
            {
                for (int i = 0; i < responsecheckOrganizationCommand.customerCompany.Count; i++)
                {
                    buttons.Add(new List<InlineKeyboardButton>());
                    var company = responsecheckOrganizationCommand.customerCompany[i];
                    var button = InlineKeyboardButton.WithCallbackData(company.Name, $"{Facade.KeyboardCommand.SelectCompany} {company.Id}");
                    buttons.LastOrDefault().Add(button);
                }
            }

            FillButtonsForPage(currentPage);

            // Добавляем кнопки "назад" и "вперед", если необходимо
            if (responsecheckOrganizationCommand.totalRecords > companiesPerPage)
            {
                if (currentPage > 0)
                {
                    buttons[1].Insert(0, InlineKeyboardButton.WithCallbackData("⬅️", $"{Facade.KeyboardCommand.PageCompanyPrevPage} {currentPage - 1}"));
                }

                if (currentPage <= (responsecheckOrganizationCommand.totalRecords / companiesPerPage)-1)
                {
                    buttons.Add(new List<InlineKeyboardButton>());
                    buttons.LastOrDefault().Add(InlineKeyboardButton.WithCallbackData("➡️", $"{Facade.KeyboardCommand.PageCompanyNextPage} {currentPage + 1}"));
                }
            }
            
            buttons.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Завершить", KeyboardCommand.CanselOperation.ToString())
            });

            return buttons;
        }

        public async Task<List<List<InlineKeyboardButton>>> PageBotTypeButtons(IMediator mediator, User? responceUser, int orgId, int currentPage = 0, int companiesPerPage = 4)
        {
             var command = new GetSliceTelegramBotTypeByIsServicesQuery()
            {
                IsSystem = false,
                Skip = currentPage * companiesPerPage,
                Take = companiesPerPage
            };

            var responseCommand = await mediator.Send(command);

            List<List<InlineKeyboardButton>> buttons = new();
            buttons.Add(new List<InlineKeyboardButton>());
            buttons.Add(new List<InlineKeyboardButton>());
            //buttons[0].Add(InlineKeyboardButton.WithCallbackData("Выйти из чата", KeyboardCommand.ExitFromBot.ToString()));

            // Заполняем кнопки для выбора организаций на текущей странице
            void FillButtonsForPage(int page)
            {

                for (int i = 0; i < responseCommand.TelegramBotTypes.Count; i++)
                {
                    buttons.Add(new List<InlineKeyboardButton>());
                    var item = responseCommand.TelegramBotTypes[i];
                    var button = InlineKeyboardButton.WithCallbackData(item.TelegramBotTypeName, $"{Facade.KeyboardCommand.StartCreationBot} {orgId} {item.Id}");
                    buttons.LastOrDefault().Add(button);
                }
            }

            FillButtonsForPage(currentPage);

            // Добавляем кнопки "назад" и "вперед", если необходимо
            if (responseCommand.TotalRecords > companiesPerPage)
            {
                if (currentPage > 0)
                {
                    buttons[1].Insert(0, InlineKeyboardButton.WithCallbackData("⬅️", $"{Facade.KeyboardCommand.PageBotTypePrevPage} {currentPage - 1}"));
                }

                if (currentPage <= (responseCommand.TotalRecords / companiesPerPage)-1)
                {
                    buttons.Add(new List<InlineKeyboardButton>());
                    buttons.LastOrDefault().Add(InlineKeyboardButton.WithCallbackData("➡️", $"{Facade.KeyboardCommand.PageBotTypeNextPage} {currentPage + 1}"));
                }
            }
            
            buttons.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Завершить", KeyboardCommand.CanselOperation.ToString())
            });

            return buttons;
        }

        public async Task<ResponceTelegramFacade> Start(TelegramBot typeTelegramBot, User requestTelegramUser)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetService<IMediator>();

            if (mediator is null)
                return new ResponceTelegramFacade(TelegramUserErrorCode.MediatorNotFound);

            //Ищем пользователя.
            var responceUser = await FindUser(mediator, requestTelegramUser, true);

            var test = new StartTelegramBotCommand { telegramUser = requestTelegramUser };
            var responce = await mediator.Send(test);

            if (responce.telegramUser is null)
            {
                return new ResponceTelegramFacade(TelegramUserErrorCode.TelegramUserNotCreated);
            }

            return new ResponceTelegramFacade(TelegramUserState.StartRegistration, true)
            {
                Message = "НОВОГОДНЯЯ ВЕЧЕРИНКА!🎄🎉🎅" +
                          "\r\n\r\n\r\n" +
                          "Некоторые уже начали готовиться к празднованию Нового года и с нетерпением ждут анонсы всех вечеринок\U0001f973💃" +
                          "\r\n\r\n" +
                          "💃Вечеринка состоится 22.12 (пятница) с 21:00 до 24:00🎉" +
                          "\r\n\r\n" +
                          "У нас в программе:" +
                          "\r\n" +
                          "🎅 Тайный Санта\r\n" +
                          "💃 Много зажигательных танцев, общение \r\n" +
                          "🌟Поздравление преподавателя школы Анны \U0001f973 а также всех ноябрьских и декабрьских именинников\r\n" +
                          "🌟Розыгрыш абонементов и скидок на абонемент\r\n" +
                          "🌟Танцевальная анимация\r\n" +
                          "🌟Вкусные треки от DJ GOODVIN\r\n" +
                          "🌟Welcome drink и аппетитные фрукты\r\n" +
                          "🌟Фотограф\r\n\r\n" +
                          "Стоимость входа:\r\n" +
                          "600₽ - при оплате онлайн до 22.12 с репостом записи\r\n" +
                          "700₽ - при оплате онлайн до 22.12 без репоста\r\n" +
                          "800₽ - при оплате онлайн 22.12 или на входе\r\n" +
                          "Нетанцующему другу/подруге вход свободный🤗\r\n\r\n\r\n" +
                          "Дресс-код: праздничный💃\r\n\r\n" +
                          "Вечеринка для всех желающих, поэтому если ты хочешь повеселиться вместе с нами, бронируй свое место у @Nataschunya 🔥\U0001f973"
            };
        }

        public async Task<ResponceTelegramFacade> ActivateUser(TelegramBot typeTelegramBot, User requestTelegramUser)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetService<IMediator>();

            if (mediator is null)
                return new ResponceTelegramFacade(TelegramUserErrorCode.MediatorNotFound);

            //Ищем пользователя.
            var responceUser = await FindUser(mediator, requestTelegramUser, true);

            responceUser.IsDeleted = false;
            var responseUpdateUser = await mediator.Send(new UpdateUserCommand { newUser = responceUser });

            return await Start(typeTelegramBot, requestTelegramUser);
        }
        
        public InlineKeyboardMarkup StartManagerButtons(IMediator mediator, User responceUser, int currentPage = 0, int companiesPerPage = 4)
        {
            // Создаем клавиатуру с кнопками
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Зарегистрировать организацию", KeyboardCommand.StartRegistrationCompany.ToString()),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Выбрать из своих зарегистрированных организаций", KeyboardCommand.SelectAlreadyRegistrationCompany.ToString())
                }
            });
            
            return keyboard;
        }
        
        public async Task<List<List<InlineKeyboardButton>>> PageTelegramBotInOrgButtons(IMediator mediator, User? responceUser, int orgId, int currentPage = 0, int companiesPerPage = 4)
        {
            var command = new GetSliceTelegramBotByOrgIdQuery()
            {
                OrgId = orgId,
                Skip = currentPage * companiesPerPage,
                Take = companiesPerPage
            };

            var responseCommand = await mediator.Send(command);

            List<List<InlineKeyboardButton>> buttons = new();
            buttons.Add(new List<InlineKeyboardButton>());
            buttons.Add(new List<InlineKeyboardButton>());
            
            // Заполняем кнопки для выбора ботов на текущей странице
            void FillButtonsForPage(int page)
            {
                for (int i = 0; i < responseCommand.TelegramBots.Count; i++)
                {
                    buttons.Add(new List<InlineKeyboardButton>());
                    var item = responseCommand.TelegramBots[i];
                    var button = InlineKeyboardButton.WithCallbackData(item.TelegramBotName, $"{Facade.KeyboardCommand.SelectTelegramBot} {item.Id}");
                    buttons.LastOrDefault().Add(button);
                }
            }

            FillButtonsForPage(currentPage);

            // Добавляем кнопки "назад" и "вперед", если необходимо
            if (responseCommand.TotalRecords > companiesPerPage)
            {
                if (currentPage > 0)
                {
                    buttons[1].Insert(0, InlineKeyboardButton.WithCallbackData("⬅️", $"{Facade.KeyboardCommand.PageTelegramBotInOrgPrevPage} {orgId} {currentPage - 1}"));
                }

                if (currentPage <= (responseCommand.TotalRecords / companiesPerPage)-1)
                {
                    buttons.Add(new List<InlineKeyboardButton>());
                    buttons.LastOrDefault().Add(InlineKeyboardButton.WithCallbackData("➡️", $"{Facade.KeyboardCommand.PageTelegramBotInOrgNextPage} {orgId} {currentPage + 1}"));
                }
            }
            
            buttons.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Создать телеграм бота", $"{KeyboardCommand.StartRegistrationBot} {orgId}"),
                InlineKeyboardButton.WithCallbackData("Настроить администраторов", $"{KeyboardCommand.SettingsAdministratorsInOrg} {orgId}"),
            });
            
            buttons.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Завершить", KeyboardCommand.CanselOperation.ToString())
            });

            return buttons;
        }
        
        public async Task<List<List<InlineKeyboardButton>>> PageAdminUsersInOrgButtons(IMediator mediator, User? responceUser, int orgId, int currentPage = 0,
            int companiesPerPage = 4)
        {
            var command = new GetSliceUsersInOrganizationByOrganizationIdWithRoleIdQuery()
            {
                OrganizationId = orgId,
                RoleId = 3, // Должен быть id роли администратора
                Skip = currentPage * companiesPerPage,
                Take = companiesPerPage
            };

            var responseCommand = await mediator.Send(command);

            List<List<InlineKeyboardButton>> buttons = new();
            buttons.Add(new List<InlineKeyboardButton>());
            buttons.Add(new List<InlineKeyboardButton>());
            
            // Заполняем кнопки для выбора администратора на текущей странице
            void FillButtonsForPage(int page)
            {
                for (int i = 0; i < responseCommand.UsersInOrganization.Count; i++)
                {
                    buttons.Add(new List<InlineKeyboardButton>());
                    var item = responseCommand.UsersInOrganization[i];
                    var button = InlineKeyboardButton.WithCallbackData(item.User?.UserName, $"{Facade.KeyboardCommand.SelectAdministratorInOrg} {item.Id}");
                    buttons.LastOrDefault().Add(button);
                }
            }

            FillButtonsForPage(currentPage);

            // Добавляем кнопки "назад" и "вперед", если необходимо
            if (responseCommand.TotalRecords > companiesPerPage)
            {
                if (currentPage > 0)
                {
                    buttons[1].Insert(0, InlineKeyboardButton.WithCallbackData("⬅️", $"{Facade.KeyboardCommand.PageAdministratorInOrgPrevPage} {orgId} {currentPage - 1}"));
                }

                if (currentPage <= (responseCommand.TotalRecords / companiesPerPage)-1)
                {
                    buttons.Add(new List<InlineKeyboardButton>());
                    buttons.LastOrDefault().Add(InlineKeyboardButton.WithCallbackData("➡️", $"{Facade.KeyboardCommand.PageAdministratorInOrgNextPage} {orgId} {currentPage + 1}"));
                }
            }
            
            buttons.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Добавить администратора", $"{KeyboardCommand.AddAdministratorInOrg} {orgId}"),
            });
            
            buttons.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Завершить", KeyboardCommand.CanselOperation.ToString())
            });

            return buttons;
        }

    }
}
