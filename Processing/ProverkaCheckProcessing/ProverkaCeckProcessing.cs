using System;
using CommonTypes.RabbitDto;
using CQRS.Query;
using DataBase.Entities.QrCodeEntities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyLoggerNamespace;
using MyLoggerNamespace.Enums;
using MyLoggerNamespace.Helpers;
using Newtonsoft.Json;
using Processing.ProverkaCheckApi;
using Processing.ProverkaCheckApi.Request;
using RabbitMQ;
using Workers.Producers;

namespace Processing.ProverkaCheckProcessing;

public class ProverkaCeckProcessing : IMainProcessing
{
    private string ClassName => nameof(ProverkaCeckProcessing);
    private readonly IMyLogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IMediator _mediator;
    
    public ProverkaCeckProcessing(IServiceScopeFactory serviceScopeFactory)
    {
        _logger = new MyLoggerNamespace.Logger(ClassName);
        _serviceScopeFactory = serviceScopeFactory;

    }
    
    public async Task MainProcessAsync(IBaseRabbitDto data)
    { 
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        // Получаем все реализации IProducer
        var producers = scope.ServiceProvider.GetRequiredService<IEnumerable<IProducer>>();
        // Извлекаем конкретную реализацию
        var proverkaCheckProducer = producers.OfType<ProverkaCheckProducer>().FirstOrDefault() as IProducer;


        var taskData = data as CheckDataRabbitDto;

        if (taskData is null || taskData.Processed == CheckProcessed.Success)
        {
            _logger.WriteLine(MessageType.Info, $"[{ClassName}] Task data not correct for this Processing: Processed=[{taskData?.Processed}] taskData=[{JsonConvert.SerializeObject(taskData)}].");
            return;
        }


        var test = CheckData.FromString(taskData.ToJson());

        //Взять объект из базы:
        var dataFromDB = await mediator.Send(new GetCheckDataByDataQuery(test));

        if (!dataFromDB.Success)
        {
            _logger.WriteLine(MessageType.Info, $"[{ClassName}] Not found in Database data=[{JsonConvert.SerializeObject(taskData)}].");
            return;
        }

        
        var checkRawItem = dataFromDB.CheckData;
        
        var client = new ProverkaCheckApiClient(_logger);
        var request = new GetCheckRequest(checkRawItem.GetRawStr());
        var response = await client.SendPostAsync(request); 
         
        _logger.WriteLine(MessageType.Info, $"[{ClassName}] response=[{JsonConvert.SerializeObject(response)}].");

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

            if (taskData.RepeatCount <= 3)
            {
                var delayTime = TimeSpan.FromMinutes(2).TotalMilliseconds + taskData.RepeatDelayMs;
                taskData.RepeatDelayMs = delayTime.ToInt();
                taskData.RepeatCount++;
                proverkaCheckProducer!.PublishWithDelay(JsonConvert.SerializeObject(taskData), delayTime.ToInt());
            }
            else
            {
                _logger.WriteLine(MessageType.Info, $"[{ClassName}] response=[{JsonConvert.SerializeObject(response)}].");
            }

        }
                        
        var updateRawCheck = await mediator.Send(new UpdateCheckDataCommand()
        {
            newCheckData = checkRawItem,
            Update = true
        });
    }
    
}