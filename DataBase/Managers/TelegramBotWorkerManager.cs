using DataBase.Entities.Entities_DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utils.Managers.TelegramBotWorkerManager;

namespace Utils.Managers
{
    public interface ITelegramBotWorkerManager
    {
        public void AddEvent(StartTelegramBotEventHandler func);
        public bool Start(string type, TelegramBots telegramBot);
    }

    public class TelegramBotWorkerManager : ITelegramBotWorkerManager
    {
        public delegate void StartTelegramBotEventHandler(TelegramBotEventArgs e);
        public event StartTelegramBotEventHandler? StartEvent;
        public bool Start(string type,TelegramBots telegramBot)
        {
            StartEvent?.Invoke(new TelegramBotEventArgs(type, telegramBot));
            return true;
        }

        public void AddEvent(StartTelegramBotEventHandler func)
        {
            StartEvent += func;
        }
    }

    public class TelegramBotEventArgs
    {
        public readonly string Type;
        public readonly TelegramBots TelegramBot;

        public TelegramBotEventArgs(string type, TelegramBots telegramBot)
        {
            Type = type;
            TelegramBot = telegramBot;
        }
    }
}
