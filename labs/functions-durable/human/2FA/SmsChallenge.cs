using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace _2FA;

    public static class SmsChallenge
    {
        [FunctionName("SmsChallenge")]
        public static int Run(
            [ActivityTrigger] string phoneNumber,
            ILogger log,
            [TwilioSms(AccountSidSetting = "TwilioAccountSid", AuthTokenSetting = "TwilioAuthToken", From = "%TwilioPhoneNumber%")]
                out CreateMessageOptions message)
        {
            // Get a random number generator with a random seed (not time-based)
            var rand = new Random(Guid.NewGuid().GetHashCode());
            int challengeCode = rand.Next(10000);

            log.LogInformation($"Sending verification code {challengeCode} to {phoneNumber}.");

            message = new CreateMessageOptions(new PhoneNumber(phoneNumber));
            message.Body = $"Your verification code is {challengeCode:0000}";

            return challengeCode;
        }
    }