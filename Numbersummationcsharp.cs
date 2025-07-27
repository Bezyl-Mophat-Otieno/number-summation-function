using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Sillivannah.Function;

public class Numbersummationcsharp
{
    private readonly ILogger<Numbersummationcsharp> _logger;

    public Numbersummationcsharp(ILogger<Numbersummationcsharp> logger)
    {
        _logger = logger;
    }

    [Function("number_summation_csharp")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        var firstNumber = req.Query["firstNumber"];
        var secondNumber = req.Query["secondNumber"];
        if (string.IsNullOrEmpty(firstNumber) || string.IsNullOrEmpty(secondNumber))
        {
            var requestBody = await req.ReadFromJsonAsync<dynamic>();
            firstNumber = requestBody?.firstNumber;
            secondNumber = requestBody?.secondNumber;

        }
        if (string.IsNullOrEmpty(firstNumber) || string.IsNullOrEmpty(secondNumber))
        {
            return new BadRequestObjectResult("Please pass both firstNumber and secondNumber in the query string or in the request body");
        }

        if (!int.TryParse(firstNumber, out var firstNum) || !int.TryParse(secondNumber, out var secondNum))
        {
            return new BadRequestObjectResult("Both firstNumber and secondNumber should be valid integers.");
        } 

        var sum = firstNum + secondNum;
        _logger.LogInformation("Summation result: {Sum}", sum);

        
        return new OkObjectResult(new { result = sum });

    }
}