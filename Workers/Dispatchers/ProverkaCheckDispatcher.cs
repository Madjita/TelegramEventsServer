using CommonTypes.RabbitDto;
using DataBase.Entities.QrCodeEntities;
using MailParserMicroService.RequestDto;
using MediatR;
using MyLoggerNamespace.Enums;
using Microsoft.Extensions.DependencyInjection;
using Processing;
using Processing.ProverkaCheckProcessing;
using RabbitMQ.ConfigModels;

namespace RabbitMQ.Dispatchers
{
    public class ProverkaCheckDispatcher
    {
        public static string  ClassLoggerName => nameof(ProverkaCheckDispatcher);

        private readonly IMainProcessing _processing;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        
        private readonly IConsumer _consumer;
        private readonly MyLoggerNamespace.Logger _logger;
        private readonly string _queue;
        private readonly string _routingKey;
        private readonly int _prefetchCount;

        public ProverkaCheckDispatcher()
        {
            _logger = new MyLoggerNamespace.Logger(nameof(ProverkaCheckDispatcher));
        }
        public ProverkaCheckDispatcher(IServiceScopeFactory serviceScopeFactory, IConsumer consumer, QueueSettings queueSettings, int prefetchCount): this()
        {
           
            _consumer = consumer;
            _queue = queueSettings.QueueName;
            _routingKey = queueSettings.ExchangeRoutingKey;
            _prefetchCount = prefetchCount;
            _serviceScopeFactory = serviceScopeFactory;
            
            _processing = new ProverkaCeckProcessing(_serviceScopeFactory);
        }

        public async Task Receive(string task)
        {
            try
            {
                CheckDataRabbitDto? checkDataRabbitDto;
                if (!string.IsNullOrEmpty(task) && (checkDataRabbitDto = CheckDataRabbitDto.FromString(task)) != null)
                {
                    try
                    {
                        // TODO: обработка задачи
                        await _processing.MainProcessAsync(checkDataRabbitDto).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.WriteLine(ex, $"[{ClassLoggerName}] Ошибка обработки задачи task=[{task}]");
                    }
                }
                else
                {
                    _logger.WriteLine(MessageType.Error, $"[{ClassLoggerName}] Задача пустая");
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex, $"[{ClassLoggerName}] Ошибка при получении задачи task=[{task}]");
            }
        }

        public async Task<bool> StartAsync()
        {
            var response = await Task.Run(async () =>
            {
                try
                {
                    if (_consumer is null)
                    {
                        _logger.WriteLine(MessageType.Error, "[{0}] консьюмер не задан", ClassLoggerName);
                        return false;
                    }
                    
                    _consumer.Start(_queue,_routingKey,Receive);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.WriteLine(MessageType.Error, "[{0}] Ошибка при старте консьюмера: {1}", ClassLoggerName, ex);
                }
                return false;
            });

            _logger.WriteLine(MessageType.Info, "[{0}] Консьюмер для очереди [{1}] Start=[{2}]", ClassLoggerName, _queue, response);
            return response;
        }

        public void Stop()
        {
            StopQueueConsumer();
        }

        private void StopQueueConsumer()
        {
            _consumer?.Stop();
            _logger.WriteLine(MessageType.Info, "[{0}] Консьюмер остановлен", ClassLoggerName);
        }
    }
}