using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using MyLoggerNamespace;
using MediatR;
using TelegramBot.Facade;
using Microsoft.Extensions.DependencyInjection;
using TelegramBot.MessageContext;
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using Utils.Managers;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using ZXing.Windows.Compatibility;
using File = System.IO.File;

namespace TelegramBot
{
    //https://telegrambots.github.io/book/2/send-msg/text-msg.html
    public abstract partial class TelegramBot: IAsyncDisposable
    {
        private static IMyLogger _logger;
        public static IMyLogger Logger => _logger;

        private readonly string _botToken = "testToken"; // Замените на ваш токен бота

        protected long chatId = 299839047;// Замените на ID чата, куда хотите отправить сообщение для отладки

        protected long? _chanelPostId = -1002112876204;

        protected TelegramBotClient _botClient;
        protected CancellationTokenSource _cts = new ();

        protected readonly string _name;

        public bool Enabled => _botEnable;

        private bool _botEnable = false;
        private bool _isRestartBot = false;
        private int _countRetry = 5;
        private bool _isDisposed = false;

        protected ConcurrentDictionary<long, TelegramBotMessageContext> _messageContexts = new ConcurrentDictionary<long, TelegramBotMessageContext>();

        //
        protected int? _id { get; private set; }
        public int TelegramBotId => _id ?? 0;
        //
        protected readonly IMediator _mediator;
        protected readonly ITelegramBotFacade _telegramBotFacade;
        protected readonly IServiceScopeFactory _scopeFactory;
        private readonly ITelegramBotWorkerManager _telegramBotWorkerManager;

        public TelegramBot(int? id, string name, string botToken, IMyLogger logger,IMediator mediator, IServiceScopeFactory scopeFactory)
        {
            _id = id;
            _name = name;
            _logger = logger;
            _botToken = botToken;

            _mediator = mediator;

            _scopeFactory = scopeFactory;

            _botClient = new TelegramBotClient(_botToken);

            //Создать фасад к телеграм Боту.
            _telegramBotFacade = new TelegramBotFacade(_logger, _scopeFactory);

            _telegramBotWorkerManager = _scopeFactory.CreateScope().ServiceProvider.GetService<ITelegramBotWorkerManager>()!;

            StartBot();
        }

        public async Task StartBotWithRetry()
        {
            _isRestartBot = true;

            _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, $"[BotApi] TelegramBot try restart bot: CountRetry={_countRetry}");

            while (_isRestartBot && _countRetry > 0)
            {
                if (_countRetry > 0)
                {
                    _countRetry--;
                }

                var isBotStop = await StopBot();
                if (isBotStop)
                {
                    _cts.Dispose();
                    _cts = new CancellationTokenSource();
                    bool isBotStarted = StartBot();
                    if (isBotStarted)
                    {
                        _countRetry = 5;
                        _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, $"[BotApi] TelegramBot restart successful");
                        _isRestartBot = false;
                    }
                    else
                    {
                        _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Error, $"[BotApi] TelegramBot restart fail");
                        _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, $"[BotApi] TelegramBot restart again CountRetry={_countRetry}");
                        _isRestartBot = false;
                    }
                }
                else
                {
                    _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Error, $"[BotApi] TelegramBot have error when he has tried restart CountRetry={_countRetry}");
                }
            }

            if (_countRetry <= 0)
            {
                _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Error, $"[BotApi] TelegramBot restart fail, could you restart server: CountRetry={_countRetry}");
            }
            _isRestartBot = false;
        }

        public async Task<bool> StopBot()
        {
            _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, "[BotApi] TelegramBot try send comand Cancel Token");

            try
            {
                _cts.Cancel();
                _botEnable = false;
                _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, "[BotApi] TelegramBot has been stoped");
            }
            catch (Exception ex)
            {
                _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Error, $"[BotApi] Telegram Bot have error when he has tried stopped: Message=[{ex.Message}] Error=[{ex}]");

                //Too Many Requests: retry after 45
                var list = ex.Message.Split(":");

                var errorCommand = list.FirstOrDefault();
                var countTimeWait = list.LastOrDefault()?.Split(" ").LastOrDefault();
                int secondsWait = 0;

                if (countTimeWait is not null)
                    secondsWait = int.Parse(countTimeWait);

                if (errorCommand is not null && errorCommand == "Too Many Requests")
                {
                    _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Error, $"[BotApi] Telegram Bot starting wait {secondsWait} seconds.");
                    await Task.Delay(secondsWait * 1000);
                    _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Error, $"[BotApi] Telegram Bot finishing wait {secondsWait} seconds. Retry StopBot.");
                }

                return false;
            }

            return true;
        }

        public bool StartBot()
        {
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            try
            {
                _botClient.StartReceiving(
                     updateHandler: HandleUpdateAsync,
                     pollingErrorHandler: HandlePollingErrorAsync,
                     receiverOptions: receiverOptions,
                     cancellationToken: _cts.Token
                );

                _botEnable = true;
            }
            catch (Exception ex)
            {
                _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, $"[BotApi] Error start bot: Message=[{ex.Message}] Error=[{ex}]");

                if (!_isRestartBot)
                    Task.Run(StartBotWithRetry);

                return false;
            }

            return true;

        }

        public async Task<Message> SendMessage(string messageText)
        {
            using CancellationTokenSource _ctsWrite = new();

            Message message = await _botClient.SendTextMessageAsync(
             chatId: new ChatId(chatId),
             text: messageText,
             cancellationToken: _ctsWrite.Token);

            return message;
        }

        public async void ReadData()
        {

            await Task.Factory.StartNew(async () =>
            {
                var me = await _botClient.GetMeAsync();

                Logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, $"Start listening for @{me.Username}");

                // Send cancellation request to stop bot
                //_cts.Cancel();
            });
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Error, $"[BotApi][HandlePollingErrorAsync] Error: [{ErrorMessage}]");

            if (!_isRestartBot)
                Task.Run(StartBotWithRetry);

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_isDisposed)
            {
                _logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, "TelegramBot start CloseAsync");
                await StopBot();
                _cts.Dispose();
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
        
        public static string DrawQrCode(string url)
        {
            var width = 250;
            var height = 250;
            var margin = 0;

            var qrCodeWriter = new BarcodeWriterSvg()
            {
                Format = BarcodeFormat.QR_CODE,
                Renderer = new SvgRenderer
                {
                    Foreground = SvgRenderer.Color.Black
                },
                Options = new EncodingOptions
                {
                    Height = height,
                    Width = width,
                    PureBarcode = true,
                    Margin = margin,
                }
            };

            var svg = qrCodeWriter.Write(url);
            return svg.Content.Replace("<!-- Created with ZXing.Net (http://zxingnet.codeplex.com/) -->","");
        }
        
        public static void SaveQrCodeToFile(string url, string filePath)
        {
            
            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            barcodeWriter.Options.Width = 200;
            barcodeWriter.Options.Height = 200;

            var barcodeBitmap = barcodeWriter.Write(url);
            barcodeBitmap.Save(filePath, ImageFormat.Png);
            
            //var svgContent = DrawQrCode(url);
            //File.WriteAllText(filePath, svgContent);
        }
    }
}