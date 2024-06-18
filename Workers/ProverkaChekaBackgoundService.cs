using System.Globalization;
using CQRS.Query;
using DataBase.Entities.QrCodeEntities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLoggerNamespace;
using MyLoggerNamespace.Enums;
using MyLoggerNamespace.Helpers;
using Newtonsoft.Json;
using Processing.ProverkaCheckApi;
using Processing.ProverkaCheckApi.Request;

namespace Workers;

public class ProverkaChekaBackgoundService : IHostedService, IDisposable
{
    private readonly IMyLogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private ManualResetEvent eventStopBatchCheck = new ManualResetEvent(false);
    private Task _taskWorker;
    
    public ProverkaChekaBackgoundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = MyLoggerNamespace.Logger.InitLogger(nameof(ProverkaChekaBackgoundService));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.WriteLine(MessageType.Info, "[ProverkaChekaBackgoundService] is starting BackgoundService.");
        //1) Реализовать запрос чека.
        //2) Получить json чека.

        _taskWorker = Task.Run(async () =>
        {

            var mediator = _scopeFactory.CreateScope().ServiceProvider.GetService<IMediator>()!;
            do
            {
                _logger.WriteLine(MessageType.Info, "[ProverkaChekaBackgoundService] is working do while.");
                
                var checks = await mediator.Send(new GetCheckDataByProcessedQuery());

                if (checks.Success)
                {
                    foreach (var checkRawItem in checks.CheckDatas)
                    {
                        var client = new ProverkaCheckApiClient(_logger);
                        var request = new GetCheckRequest(checkRawItem.GetRawStr());
                        var response = await client.SendPostAsync(request);
                        
                        _logger.WriteLine(MessageType.Info, $"[ProverkaChekaBackgoundService] response=[{JsonConvert.SerializeObject(response)}].");

                        if (response is not null && response.Code == 1)
                        {
                            
                            //Сохраним компанию
                            var saveCompany = await mediator.Send(new UpdateCheckCompanyCommand()
                            {
                               newCheckCompany = new CheckCompany()
                               {
                                   Name = response.Data.Json.User,
                                   Inn = Convert.ToInt64(response.Data.Json.UserInn.Trim()),
                                   RetailPlace = response.Data.Json.RetailPlace,
                                   RetailPlaceAddress = response.Data.Json.RetailPlaceAddress,
                                   SellerAddress = response.Data.Json.SellerAddress
                               }
                            });
                            
                            foreach (var parsedCheckItem in response.Data.Json.CheckItems)
                            {
                                var checkParsed = await mediator.Send(new UpdateCheckParsedItemCommand()
                                {
                                    newCheckParsedItem = new CheckParsedItems()
                                    {
                                        CheckDataId = checkRawItem.Id,
                                        CheckCompanyId = saveCompany.CheckCompany!.Id,
                                        Name = parsedCheckItem.Name,
                                        Sum = parsedCheckItem.Sum,
                                        Price = parsedCheckItem.Price,
                                        Nds = parsedCheckItem.Nds,
                                        Quantity = parsedCheckItem.Quantity,
                                        Paymenttype = parsedCheckItem.PaymentType,
                                        Producttype = parsedCheckItem.ProductType,
                                        NdsSum = parsedCheckItem.NdsSum,
                                        Itemsquantitymeasure = parsedCheckItem.ItemsQuantityMeasure
                                    }
                                });
                            }
                            
                            checkRawItem.Processed = CheckProcessed.Success;
                        }
                        else
                        {
                            checkRawItem.Processed = CheckProcessed.IsError;
                        }
                        
                        var updateRawCheck = await mediator.Send(new UpdateCheckDataCommand()
                        {
                            newCheckData = checkRawItem,
                            Update = true
                        });
                    }
                }

                eventStopBatchCheck.WaitOne(1 * 60 * 1000); //минуту спать
                
            } while (!eventStopBatchCheck.WaitOne(0));
            
            _logger.WriteLine(MessageType.Info, "[ProverkaChekaBackgoundService] is stoping taskWorker.");
        });
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.WriteLine(MessageType.Info, "[ProverkaChekaBackgoundService] is stopping.");
        eventStopBatchCheck.Set();
        return;
    }

    public void Dispose()
    {
        // Освобождение ресурсов, если необходимо
    }
}