
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Aws libraries
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace ComprehendTutorial.Controllers
{
    [Route("api/[controller]")]
    
    public class CommentsController : ControllerBase
    {
        //POST api/comments
        
        // Sentiment Analysis on per text basis 
        [HttpPost]
        public async Task<JsonResult> Post([FromBody] string commentText)
        {
            try
            {
                using (var comprehendcClient = new AmazonComprehendClient(Amazon.RegionEndpoint.APSouth1))
                {
                    var sentimentResults = await comprehendcClient.DetectSentimentAsync(
                        new DetectSentimentRequest()
                        {
                            Text = commentText,
                            LanguageCode = LanguageCode.En
                        });

                    if (sentimentResults.Sentiment.Value == "NEGATIVE")
                    {
                        using (var snsClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.APSouth1))
                        {
                            var response = await snsClient.PublishAsync(new PublishRequest()
                            {
                                Subject = "Negative Comment",
                                Message = $"Someone just posted a negative comment on your post. Check it out - " +
                                $"{Environment.NewLine}<b>{commentText}</b>",
                                TargetArn = "arn:aws:sns:ap-south-1:429636622452:CommentNotifier"


                            });
                        }
                    }
                }
            }
            catch (Exception ex)
                { return new JsonResult(ex.Message); }
            return new JsonResult("PostSuccessful!!");
        }
    }
}
