using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ApprovalWorkflow;

public static class SmsVerify
{
    [FunctionName("SmsVerify")]
    public static async Task<bool> Run(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        string phoneNumber = context.GetInput<string>();
        if (string.IsNullOrEmpty(phoneNumber))
        {
            throw new ArgumentException("Phone number is required");
        }

        log.LogInformation($"Starting SmsChallenge for: {phoneNumber}");
        int challengeCode = await context.CallActivityAsync<int>("SmsChallenge", phoneNumber);

        using (var timeoutCts = new CancellationTokenSource())
        {
            var expiration = context.CurrentUtcDateTime.AddSeconds(5*60);
            var timeoutTask = context.CreateTimer(expiration, timeoutCts.Token);

            var authorized = false;
            for (int retryCount = 0; retryCount <= 3; retryCount++)
            {
                var challengeResponseTask =  context.WaitForExternalEvent<int>("SmsChallengeResponse");
                var winner = await Task.WhenAny(challengeResponseTask, timeoutTask);
                if (winner == challengeResponseTask)
                {
                    if (challengeResponseTask.Result == challengeCode)
                    {
                        log.LogInformation($"Authorized! User responded correctly to SmsChallenge for: {phoneNumber}");                    
                        authorized = true;
                        break;
                    }
                    log.LogInformation($"NOT authorized. User did not respond with correct code to SmsChallenge for: {phoneNumber}");                    
                }
                else
                {
                    log.LogInformation($"NOT authorized. User did not respond to SmsChallenge for: {phoneNumber}");
                    break;
                }
            }

            if (!timeoutTask.IsCompleted)
            {
                timeoutCts.Cancel();
            }

            return authorized;
        }
    }
}