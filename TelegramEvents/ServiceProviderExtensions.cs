using Authorization;
using Authorization.Bearer.Authentication;
using Authorization.Bearer.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

using MediatR;
using Couchbase.Extensions.DependencyInjection;
using DataBase.Coutchbase;
using CQRS;
using Couchbase.KeyValue;
using DataBase.Entities.Entities_DBContext;
using CQRS.Query;
using DataBase.Contexts.DBContext;
using Workers;
using CQRS.Query.Events;
using CQRS.Query.TelegramBotInChat;
using CQRS.Query.CustomerCompany;
using CQRS.Command.UserInfo;
using Microsoft.AspNetCore.Authorization;
using MyLoggerNamespace;
using TelegramEvents.Fasad;
using Utils.Managers;

namespace MailParserMicroService
{
    public static partial class ServiceProviderExtensions
    {
        public static void InitSettings(this IServiceCollection services,IConfiguration configurationManager,IWebHostEnvironment env)
        {
            services.InitCors();
            services.AddSwagger();
            
            services.AddCustomDbContext(configurationManager, env);
            services.AddTransientServices(configurationManager);
            services.AddScopedServices(configurationManager);
            services.AddSingeltonServices(configurationManager);
            services.AddHttpContextAccessor();

            // Регистрация MediatR
            services.AddMediatR(typeof(Mediator));
            services.InitMediatorCommands(configurationManager);
            services.InitTelegramBotCommands(configurationManager);


            services.AddConfigureAuthentication();
            services.AddConfigureAuthorization(configurationManager);

            services.AddHostedService(configurationManager);


        }

        /// <summary>
        ///     Method to initialization CORS policy
        /// </summary>
        private static IServiceCollection InitCors(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://192.168.0.2:3000", "http://95.188.89.10:3000", "http://192.168.18.65:3000", "https://192.168.0.2:3000", "https://95.188.89.10:3000") // Укажите домен фронтенда
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
                    //.SetIsOriginAllowed(_ => true);
                    // policy.AllowAnyMethod()
                    //     .AllowAnyHeader()
                    //     .AllowCredentials()
                    //     .SetIsOriginAllowed(_ => true)
                    //     .WithOrigins("http://localhost:4200/", "http://localhost:8080/");
                })
            );

            return serviceCollection;
        }

        /// <summary>
        ///     Method to add DbContext
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        private static IServiceCollection AddCustomDbContext(
            this IServiceCollection serviceCollection,
            IConfiguration configuration,
            IWebHostEnvironment env
        )
        {
            var connectionJson = configuration.GetConnectionString("AuthConnection");

            // serviceCollection.AddDbContext<AuthDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b=> b.MigrationsAssembly("AuthDAL")));
            //Context lifetime MUST BE ServiceLifetime.Scoped, for usage in singletons, use IDbContextFactory<AppDbContext> instead
            //Options must be ServiceLifetime.Singleton in order to be consumed in a DbContextFactory which is a singleton
            serviceCollection.AddDbContext<DBContext>(options =>
            {
                options
                    .UseNpgsql(connectionJson); // Используем провайдер PostgreSQL

                if (env.IsDevelopment()) options.EnableSensitiveDataLogging().EnableDetailedErrors();
            }, ServiceLifetime.Scoped, ServiceLifetime.Singleton);

            serviceCollection.AddScoped<IDBContext>(provider => provider.GetService<DBContext>()!);

            //ADBConnection.LogLevel level = (ADBConnection.LogLevel)Enum.Parse(typeof(ADBConnection.LogLevel), "OnlyExceptions");
            //ADBConnection connection = (ADBConnection)ADBConnection.Create("Postgress", connectionJson, "MsSqlConnection.dll", "MsSqlConnection.MsDBConnection", level, 15);

            return serviceCollection;
        }

        public static IServiceCollection AddSwagger(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API",
                    Version = "v1",
                    Description = "An API of ASP.NET Core",
                });

                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Scheme = "Bearer",
                        Name = "Authorization"
                    });
                
                //c.OperationFilter<AuthorizeCheckOperationFilter>();

                //c.EnableAnnotations();
            });
        
            return serviceCollection;
        }

        private static IServiceCollection AddSingeltonServices(this IServiceCollection serviceCollection, IConfiguration Configuration)
        {
            // Получите доступ к конфигурации из appsettings.json
            serviceCollection.AddCouchbase(Configuration.GetSection("Couchbase")); // (1)
            serviceCollection.AddSingleton<ICouchBaseAdapter, CouchBaseAdapter>();
            serviceCollection.AddSingleton<ITelegramBotWorkerManager, TelegramBotWorkerManager>();

            return serviceCollection;
        }

        private static IServiceCollection AddTransientServices(this IServiceCollection services, IConfiguration Configuration)
        {
            // Регистрация обработчиков команд и запросов
           
            return services;
        }

        private static IServiceCollection AddScopedServices(this IServiceCollection serviceCollection, IConfiguration Configuration)
        {
            //Extentions Repository
            serviceCollection.AddScoped<IUserFacade, UserFacade>();
            //serviceCollection.AddScoped<IUserRepository, UserRepository>();
            //serviceCollection.AddScoped<ICompanyRepository, CompanyRepository>();
            //serviceCollection.AddScoped<ITelegramUserRepository, TelegramUserRepository>();

            return serviceCollection;
        }

        private static IServiceCollection AddHostedService(this IServiceCollection serviceCollection, IConfiguration Configuration)
        {
            serviceCollection.AddSingleton<IHostedService, TelegramBotBackgroundService>();
            return serviceCollection;
        }

        public static IServiceCollection AddConfigureAuthentication(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAuthentication(options =>
                {
                    options.DefaultScheme = AuthenticationSchemes.Default; // Установите схему по умолчанию
                    options.DefaultChallengeScheme = AuthenticationSchemes.Default; // Установите схему для вызова аутентификации
                })
                .AddScheme<DefaultAuthenticationSchemeOptions, DefaultAuthenticationHandler>(
                    AuthenticationSchemes.Default, null)
                .AddScheme<AccessTokenAuthenticationSchemeOptions, AccessTokenAuthenticationHandler>(
                    AuthenticationSchemes.AccessToken, null)
                .AddScheme<JsonWebTokenAuthenticationSchemeOptions, JsonWebTokenAuthenticationHandler>(
                    AuthenticationSchemes.JsonWebToken, null)
                .AddScheme<SessionCookieAuthenticationSchemeOptions, SessionCookieAuthenticationHandler>(
                    AuthenticationSchemes.SessionCookie, null)
                .AddCookie("Cookies", options =>
                {
                    // Настройки аутентификации с использованием кук
                    options.Cookie.Name = "SessionUID"; // Имя куки
                    options.Cookie.HttpOnly = true; // Защита от XSS атак
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Требуется HTTPS
                    options.Cookie.SameSite = SameSiteMode.Strict; // Запретить куки отправляться с запросами с других доменов
                    options.LoginPath = "/"; // Путь к странице входа
                });

            return serviceCollection;
        }

        public static IServiceCollection AddConfigureAuthorization(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(AdminRoleRequirement),
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(AuthenticationSchemes.SessionCookie);
                        policy.Requirements.Add(new AdminRoleRequirement(new List<string>{"admin"}));
                    });
            });
            
            serviceCollection.AddSingleton<IAuthorizationHandler, AdminRoleAuthorizationHandler>();

            return serviceCollection;
        }

        public static IServiceCollection InitMediatorCommands(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            #region CoutchBase
            #region Command
            serviceCollection.AddScoped<IRequestHandler<ExistsCommandCoutchBase, bool>, ExistsCommandCoutchBaseHandler>();
            serviceCollection.AddScoped<IRequestHandler<InsertCommandCoutchBase, bool>, InsertCommandCoutchBaseHandler>();
            serviceCollection.AddScoped<IRequestHandler<UpserWithUnlockCommandCoutchBase, bool>, UpserWithUnlockCommandCoutchBaseHandler>();

            #endregion

            #region Queries
            serviceCollection.AddScoped<IRequestHandler<GetAndLockQueryCoutchBase, IGetResult>, GetAndLockQueryCoutchBaseHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetAndTouchQueryCoutchBase, IGetResult>, GetAndTouchQueryCoutchBaseHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetQueryCoutchBase,IGetResult?>, GetQueryCoutchBaseHandler>();
            #endregion

            #endregion

            #region Queries

            #region User
            serviceCollection.AddScoped<IRequestHandler<GetUserByIdQuery, User?>, GetUserQueryHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetUserByEmailORLoginQuery, User?>, GetUserByEmailORLoginQueryHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetUserByChatIdQuery, User?>, GetUserByChatIdQueryHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetUserByUserNameQuery, User?>, GetUserByUserNameQueryHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetUserAllQuery, List<User>>, GetUserAllQueryHandler>();
            
            #endregion

            #endregion
            serviceCollection.AddScoped<IRequestHandler<CheckExistXOrgUserCommand, (bool Success, XOrgUser? xOrgUser)>, CheckExistXOrgUserCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<UpdateEventsCommand, (bool Success, Event? newEvent)>, UpdateEventsCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<CheckExistEventsCommand, (bool Success, Event? newEvent)>, CheckExistEventsCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetAllTelegramBotsQuery, IEnumerable<TelegramBots>>, GetAllTelegramBotsQueryHandler>();
            serviceCollection.AddScoped<IRequestHandler<UpdateTelegramBotCommand, (bool Success, TelegramBots? TelegramBot)>, UpdateTelegramBotCommandHandler>();

            serviceCollection.AddScoped<IRequestHandler<DeleteTelegramBotInChatCommand, bool>, DeleteTelegramBotInChatCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<UpdateTelegramBotInChatCommand, (bool Success, TelegramBotInChats? newTelegramBotInChat)>, UpdateTelegramBotInChatCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<CheckExistOrgCommand, (bool Success, Org? customerCompany)>, CheckExistOrgCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetOrgByIdCommand, (bool Success, Org? customerCompany)>, GetOrgByIdCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<UpdateOrgCommand, (bool Success, Org? customerCompany)>, UpdateOrgCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<CheckExistRegistrationOnEventsCommand, (bool Success, XEventUser? registrationOnEvents)>, CheckExistRegistrationOnEventsCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<UpdateRegistrationOnEventsCommand, (bool Success, XEventUser? registrationOnEvents)>, UpdateRegistrationOnEventsCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<CheckExistWhoCreatedOrgCommand, (bool Success, List<Org> org, int totalRecords)>, CheckExistWhoCreatedOrgCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetAllTelegramBotByOrgIdQuery, IEnumerable<TelegramBots>>, GetAllTelegramBotByOrgIdQueryHandler>();
            serviceCollection.AddScoped<IRequestHandler<CheckExistTelegramBotByTokenQuery, TelegramBots?>, GetAllTelegramBotByOrgIdQueryHandler >();


            //weather
            serviceCollection.AddScoped<IRequestHandler<UpdateOpenWeatherMapCommand, (bool Success, OpenWeatherMap? OpenWeatherMapDB)>, UpdateOpenWeatherMapCommandHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetOpenWeatherMapQuery, (bool Success, OpenWeatherMap? OpenWeatherMapDB)>, GetOpenWeatherMapQueryHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetWeatherCityQuery, WeatherCity?>, GetWeatherCityQueryHandler>();
            serviceCollection.AddScoped<IRequestHandler<GetByIdWeatherCityQuery, WeatherCity?>, GetWeatherCityQueryHandler>();
            serviceCollection.AddScoped<IRequestHandler<UpdateWeatherSubscribersQuery,(bool Success, WeatherSubscribers? WeatherSubscribers)>, UpdateWeatherSubscribersQueryHandler>();

            return serviceCollection;
        }

        public static IServiceCollection InitTelegramBotCommands(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
           serviceCollection.AddScoped<IRequestHandler<CheckExistUserCommand, (bool Success, User? user)>, CheckExistUserCommandHandler>();
           serviceCollection.AddScoped<IRequestHandler<UpdateUserCommand, bool>, UpdateUserCommandHandler>();
           serviceCollection.AddScoped<IRequestHandler<StartTelegramBotCommand, (User? telegramUser, string password)>, StartTelegramBotCommandHandler>();
           serviceCollection.AddScoped<IRequestHandler<RegistrationCustomerCompanyTelegramBotCommand, (Org? customerCompany, bool created)>, RegistrationCustomerCompanyTelegramBotCommandHandler>();
           serviceCollection.AddScoped<IRequestHandler<ExitFromBotTelegramBotCommand, bool>, ExitFromBotTelegramBotCommandHandler>();

            return serviceCollection;
        }

    }
}