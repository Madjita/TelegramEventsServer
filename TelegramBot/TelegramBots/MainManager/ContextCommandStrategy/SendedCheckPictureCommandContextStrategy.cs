using CQRS;
using CQRS.Query.TelegramBotInChat;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Facade;
using TelegramBot.MessageContext;
using Utils.Managers;
using User = DataBase.Entities.Entities_DBContext.User;


namespace TelegramBot.TelegramBots.MainManager.ContextCommandStrategy;

public class SendedCheckPictureCommandContextStrategy : BaseCommandContextStrategy
{
    protected long? _chanelPostId = -1002112876204;
    
    public SendedCheckPictureCommandContextStrategy(
        Message message,
        ITelegramBotClient botClient, 
        IMediator mediator,
        TelegramBotMessageContext currentContextMessage,
        IServiceScopeFactory scopeFactory,
        ITelegramBotFacade telegramBotFacade
    ) :
        base(message, botClient, mediator, currentContextMessage, scopeFactory, telegramBotFacade)
    {

    }

    public override async Task<CommandContextStrategyResponseDto> Execute(User telegramUser, string messageText,
        CancellationToken cancellationToken)
    {
        var responseDto = new CommandContextStrategyResponseDto();
        
        // Текст, который вы хотите закодировать в QR-код
        string textToEncode = _currentContextMessage.LastMessage;

        // Сохранить фотку которую нам прислали:
        // Получаем информацию о фото
        var photo = _message.Photo[^1]; // берем последний элемент массива, который обычно является самым крупным размером
        var fileId = photo.FileId;
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory; // или Directory.GetCurrentDirectory()
        var dir = Path.Combine(baseDirectory, "downloaded_photos");

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Путь, куда сохранить файл
        var savePath = $"{dir}/{textToEncode}.jpg";

        // Получаем информацию о файле и скачиваем его
        using (var destinationStream = new MemoryStream())
        {
            await _botClient.GetInfoAndDownloadFileAsync(fileId, destinationStream);

            // Копируем файл в указанный путь
            using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                destinationStream.Seek(0, SeekOrigin.Begin);
                await destinationStream.CopyToAsync(fileStream);
            }

            Console.WriteLine($"Фотография сохранена: {savePath}");
        }

        var chanel = await _mediator.Send(new GetTelegramBotInChatCommand
        {
            TelegramBotId = (long)_telegramBotFacade.CurrentTelegramBotId
        });
                            
        if (chanel is not null )
        {
            _chanelPostId = chanel.TelegramChatId;
            using (var imageStream = new FileStream(savePath, FileMode.Open))
            {
                MemoryStream stream = new();
                imageStream.CopyTo(stream);
                stream.Position = 0;
                var imageInput = new InputFileStream(stream, $"{textToEncode}.jpg");
                _inlineKeyboard = new(
                    new[] {
                        new[] { InlineKeyboardButton.WithCallbackData("Фейк",KeyboardCommand.FaildCheckPictureFromMenager.ToString()) },
                        new[] { InlineKeyboardButton.WithCallbackData("Подтверить", KeyboardCommand.ApproveCheckPictureFromMenager.ToString()) }
                    }
                );
                
                responseDto.SentMessage = await _botClient.SendPhotoAsync(
                    chatId: _chanelPostId,
                    photo: imageInput,
                    replyMarkup: _inlineKeyboard,
                    caption: $"Дата: {DateTime.Now.ToString("dd:MM:yyyy HH:mm:ss")}\n" +
                                                 $"Телеграм пользователь: {telegramUser.TelegramChatId} {telegramUser.UserName.Replace("_", "\\_")}\n" +
                                                 $"Чек на регистрацию: {_currentContextMessage.UserInfo.FIO}\n" +
                                                 $"Телефон: {_currentContextMessage.UserInfo.Phone}",
                    parseMode: ParseMode.Markdown
                    );
            }
                                
            // Создание QR-кода
            var qrCodePath = Path.Combine(baseDirectory, "downloaded_photos", $"{textToEncode}_QrCode.svg");
            TelegramBot.SaveQrCodeToFile("https://avatars.mds.yandex.net/i?id=560e59bc7761ffdc6fcf0193cf42877d9ab7c05b-12393450-images-thumbs&n=13", qrCodePath);
                        
            try
            {
                using (var imageStream = new FileStream(qrCodePath, FileMode.Open))
                {
                    MemoryStream stream = new();
                    imageStream.CopyTo(stream);
                    stream.Position = 0;
                    //var imageInput = new InputFileStream(stream, "QRCode");
                    var imageInput = new InputFileStream(stream, "qr_code.svg");
                    responseDto.SentMessage = await _botClient.SendPhotoAsync(
                                            chatId: telegramUser.TelegramChatId,
                                            photo: imageInput,
                                            caption: "Ваш QR код для входа:"
                        ); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        else
        {
            responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                chatId: telegramUser.TelegramChatId,
                text: "Телеграм канал не подключен.",
                replyMarkup: _inlineKeyboard,
                cancellationToken: cancellationToken);
        }
                                    
                                   
        return responseDto;
    }
}