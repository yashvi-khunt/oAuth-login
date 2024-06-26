﻿using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OAuthLogin.BLL.Repositories;
using OAuthLogin.DAL.Models;
using OAuthLogin.DAL.ViewModels;


namespace OAuthLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CrawlerController : ControllerBase
    {
        private readonly ICrawlerService _crawlerService;
        private readonly UserManager<ApplicationUser> _userManager;
        public CrawlerController(ICrawlerService crawlerService, UserManager<ApplicationUser> userManager)
        {
            _crawlerService = crawlerService;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("CreateJob/{jobId}")]
        public IActionResult CreateJob(int jobId)
        {
            RecurringJob.AddOrUpdate<ICrawlerService>($"Job{jobId}", x => x.TriggerJob(jobId), GenerateCronExpression() );
            RecurringJob.TriggerJob($"Job{jobId}");
            return Ok(new Response("Data loading scheduled successfully!", true));
        }

        [HttpPost("TriggerJob/{jobId}")]
        public void TriggerJob(int jobId)
        {
            RecurringJob.TriggerJob($"Job{jobId}");
        }


        [HttpDelete]
        [Route("RemoveJob/{jobId}")]
        public IActionResult RemoveJob(int jobId)
        {
            RecurringJob.RemoveIfExists($"Job{jobId}");
            return Ok(new Response("Job stopped successfully!", true));
        }

        [HttpPost]
        [Route("AddJob")]
        public IActionResult AddJob(VMAddCrawlingJob vMAddCrawlingJob)
        {
            var user = _userManager.GetUserId(User);
            if (user == "")
            {
                return StatusCode(500, new Response("User not found.", false));
            }
            var response = _crawlerService.AddCrawlingJob(vMAddCrawlingJob, user);
            if (response.IsCompletedSuccessfully)
            {
                IActionResult getDataResult = CreateJob(response.Result.Id);
                return Ok(new Response("Data Added Successfully!", true));
            }
            else return StatusCode(500, new Response("Something went wrong.", false));
        }

        [HttpGet("GetCrawlingJobs")]
        public async Task<ActionResult<List<VMGetCrawlingJobs>>> GetAllCrawlingJobs([FromQuery] VMGetCrawlingJobsInput getCrawlingJobsInput)
        {
            //var user = await _userManager.GetUserAsync(User);

            //if (User.IsInRole("User"))
            //{
            //    getLoginHistoryInputModel.UserIds = user.Id;
            //}


            var crawlingJobs = await _crawlerService.GetAllCrawlingJobs(getCrawlingJobsInput);

            //var mappedLoginHistories = _mapper.Map<VMGetLoginHistories>(loginHistories);

            //if (mappedLoginHistories == null)
            //{
            //    return NotFound(new Response(MESSAGE.DATA_NOT_FOUND, false));
            //}

            return Ok(new Response<VMGetCrawlingJobs>(crawlingJobs, true, "Data loaded successfully!"));
        }

        [HttpGet("GetResponseForJobId/{JobId}")]
        public async Task<IActionResult> GetJobsForId(int JobId)
        {
            var response = await _crawlerService.GetResponseForJobId(JobId);
            if (response != null)
            {
                return Ok(new Response<List<VMJobResponseForJobId>>(response, true, "Data loaded successfully!"));
            }
            else { return StatusCode(500, new Response("Something went wrong!", false)); }
        }

        [HttpGet("GetFormByJobId/{JobId}")]
        public async Task<IActionResult> GetFormByJobId(int JobId)
        {
            try
            {
                var response = await _crawlerService.GetFormByJobId(JobId);
                if (response != null)
                {
                    return Ok(new Response<VMAddCrawlingJob>(response, true, "Data loaded successfully!"));
                }
                else { return StatusCode(500, new Response("Something went wrong!", false)); }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response(ex.Message, false));
            }
        }


        [HttpPut]
        [Route("EditJob/{jobId}")]
        public async Task<IActionResult> EditJob(int jobId, VMAddCrawlingJob vMAddCrawlingJob)
        {
            var user = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(user))
            {
                return StatusCode(500, new Response("User not found.", false));
            }

            try
            {
                var response = await _crawlerService.EditCrawlingJob(jobId, vMAddCrawlingJob, user);
                if (response != null)
                {
                    // Optionally re-schedule the job if needed
                    RecurringJob.AddOrUpdate<ICrawlerService>($"Job{jobId}", x => x.TriggerJob(jobId), GenerateCronExpression());
                    RecurringJob.Trigger($"Job{jobId}");
                    return Ok(new Response("Job edited and rescheduled successfully!", true));
                }
                else
                {
                    return StatusCode(500, new Response("Something went wrong.", false));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response(ex.Message, false));
            }
        }

        private string GenerateCronExpression()
        {
            DateTime now = DateTime.UtcNow;

            // Extract the hour and minute
            int hour = now.Hour ;
            int minute = now.Minute ;

            // Construct the cron expression for daily at the current time
            string cronExpression = $"{minute} {hour} * * *";
            return cronExpression;
        }
    }
}
