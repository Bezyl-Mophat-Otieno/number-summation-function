using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Sillivannah.Function
{
    public class Numbersummationcsharp
    {
        private readonly ILogger<Numbersummationcsharp> _logger;

        public Numbersummationcsharp(ILogger<Numbersummationcsharp> logger)
        {
            _logger = logger;
        }

        [Function("Numbersummationcsharp")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            var firstNumber = req.Query["firstNumber"];
            var secondNumber = req.Query["secondNumber"];

            // If not found in query, try to read the request body
            if (string.IsNullOrEmpty(firstNumber) || string.IsNullOrEmpty(secondNumber))
            {
                var requestBody = await req.ReadFromJsonAsync<NumberRequest>();
                firstNumber = requestBody?.FirstNumber.ToString();
                secondNumber = requestBody?.SecondNumber.ToString();
            }

            // Validation
            if (string.IsNullOrEmpty(firstNumber) || string.IsNullOrEmpty(secondNumber))
            {
                return new BadRequestObjectResult("Please pass both firstNumber and secondNumber in the query string or in the request body");
            }

            // Check if the numbers are valid integers
            if (!int.TryParse(firstNumber, out var firstNum) || !int.TryParse(secondNumber, out var secondNum))
            {
                return new BadRequestObjectResult("Both firstNumber and secondNumber should be valid integers.");
            }

            // Perform summation
            var sum = firstNum + secondNum;

            _logger.LogInformation("Summation result: {Sum}", sum);

            return new OkObjectResult(new { result = sum });
        }
    }
}

public class NumberRequest
{
    public int? FirstNumber { get; set; }
    public int? SecondNumber { get; set; }
}
