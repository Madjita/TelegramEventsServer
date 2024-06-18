using System.Globalization;
using Authorization;
using CQRS.Query;
using DataBase.Entities.QrCodeEntities;
using MailParserMicroService.RequestDto;
using MailParserMicroService.ResponseDto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyLoggerNamespace;
using MyLoggerNamespace.Enums;
using MyLoggerNamespace.Helpers;

namespace TelegramEvents.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.Default)]
public class QrCodeController : ControllerBase
{
    private readonly IMyLogger _logger;
    private readonly IMediator _mediator;

    public QrCodeController(IMediator mediator)
    {
        _logger = MyLoggerNamespace.Logger.InitLogger(this.GetType().Name);
        _mediator = mediator;
    }
    
    [Route("Save")]
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SaveQrData([FromBody] SaveQrDataRequest request)
    {
        _logger.WriteLine(MessageType.Debug,$"[SaveQrData] data = [{request.QrData}]");

        if (!string.IsNullOrEmpty(request.QrData))
        {
            try
            {
                var checkDataDictionary = request.QrData.ConvertParamsWithNonUniqueKeysToDictionary(insensitiveKey: false, insensitiveValue: false, splitParams:'&');

                if (checkDataDictionary.ContainsKey("t") && checkDataDictionary.ContainsKey("fn") && checkDataDictionary.ContainsKey("fp") && checkDataDictionary.ContainsKey("i"))
                {
                    var response = await _mediator.Send(new UpdateCheckDataCommand {newCheckData = new CheckData()
                    {
                        Fn =  checkDataDictionary["fn"],
                        Fp = checkDataDictionary["fp"],
                        I = checkDataDictionary["i"],
                        N = checkDataDictionary["n"],
                        S = checkDataDictionary["s"],
                        T = checkDataDictionary["t"]
                    }});

                    if (response.Success)
                    {
                        return Ok("We have received the information and are processing it.");
                    }
                }
                
                return Ok("We have received the information and are processing it.");
            }
            catch (Exception e)
            {
                _logger.WriteLine(e,$"[SaveQrData] Have exception data = [{request.QrData}]");
                return BadRequest("Have exception");
            }
        }
        
        return Ok("We have received the information and are processing it.");
    }

    [Route("GetCheckProcessed")]
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> GetCheckProcessed ([FromBody] SaveQrDataRequest request)
    {
        _logger.WriteLine(MessageType.Debug,$"[GetCheckProcessed] data = [{request.QrData}]");

        if (!string.IsNullOrEmpty(request.QrData))
        {
            try
            {
                var checkDataDictionary = request.QrData.ConvertParamsWithNonUniqueKeysToDictionary(insensitiveKey: false, insensitiveValue: false, splitParams:'&');

                if (checkDataDictionary.ContainsKey("t") && checkDataDictionary.ContainsKey("fn") && checkDataDictionary.ContainsKey("fp") && checkDataDictionary.ContainsKey("i"))
                {
                    var response = await _mediator.Send(new GetCheckProcessed_CheckParsedItemCommand {CheckData = new CheckData()
                    {
                        Fn =  checkDataDictionary["fn"],
                        Fp = checkDataDictionary["fp"],
                        I = checkDataDictionary["i"],
                        N = checkDataDictionary["n"],
                        S = checkDataDictionary["s"],
                        T = checkDataDictionary["t"]
                    }});

                    if (response.Success)
                    {
                        var first = response.ListCheckParsedItems.FirstOrDefault();

                        CheckProcessedResponse checkProcessedResponse = new CheckProcessedResponse();
                        
                        for (int i = 0; i < response.ListCheckParsedItems.Count; i++)
                        {
                            var item = response.ListCheckParsedItems[i];
                            
                            if (i == 0)
                            {
                                
                                string dateString = item.CheckData!.T;
                                DateTime parsedDateTime;

                                // Попробовать разобрать сначала формат "yyyyMMddTHHmmss"
                                if (DateTime.TryParseExact(dateString, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
                                {
                                    checkProcessedResponse.DateTime = parsedDateTime;
                                }
                                else
                                {
                                    // Если не удалось разобрать с форматом "yyyyMMddTHHmmss", попробовать с форматом "yyyyMMddTHHmm"
                                    if (DateTime.TryParseExact(dateString, "yyyyMMddTHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
                                    {
                                        checkProcessedResponse.DateTime = parsedDateTime;
                                    }
                                    else
                                    {
                                        // Обработка ошибки парсинга, если формат строки не соответствует ожидаемым
                                    }
                                }
                                
                                checkProcessedResponse.DateTime = parsedDateTime;
                                checkProcessedResponse.Name = item.CheckCompany!.Name;
                                checkProcessedResponse.Inn = item.CheckCompany.Inn;
                                checkProcessedResponse.RetailPlace = item.CheckCompany.RetailPlace;
                                checkProcessedResponse.RetailPlaceAddress = item.CheckCompany.RetailPlaceAddress;
                                checkProcessedResponse.SellerAddress = item.CheckCompany.SellerAddress;
                                checkProcessedResponse.CheckRawData = item.CheckData.GetRawStr();
                            }

                            item.CheckCompany = null;
                            item.CheckData = null;

                            var cehckItem = new CheckProcessedItemResponse
                            {
                                Name = item.Name,
                                Nds = item.Nds,
                                Sum = item.Sum,
                                Price = item.Price,
                                NdsSum = item.NdsSum,
                                Quantity = item.Quantity,
                                Itemsquantitymeasure = item.Itemsquantitymeasure,
                                Paymenttype = item.Paymenttype,
                                Producttype = item.Producttype
                            };
                            
                            checkProcessedResponse.CheckProcessedItems.Add(cehckItem);
                        }
                        
                        return Ok(checkProcessedResponse);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.WriteLine(e,$"[GetCheckProcessed] Have exception data = [{request.QrData}]");
                return BadRequest("Have exception");
            }
        }

        return BadRequest("Check not found");
    }
}
