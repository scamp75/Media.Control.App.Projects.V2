using Microsoft.AspNetCore.Mvc;
using Media.Control.App.Api.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Text;

namespace Media.Control.App.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private MediaDbService mediaService = null;
        private readonly IHubContext<LogHub> _hubContext;

        public LogController(IHubContext<LogHub> hubContext)
        {
            _hubContext = hubContext;
            mediaService = new MediaDbService("log");
        }

        // POST: api/Log (로그 입력)
        [HttpPost]
        public IActionResult CreateLog([FromBody] LogData log)
        {

            var jsonData = JsonConvert.SerializeObject(log);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // SignalR을 통해 클라이언트에 실시간으로 로그 정보 전송
            _hubContext.Clients.All.SendAsync("loghub", jsonData);

            return mediaService.InsertLogDate(log) == true ? Ok() : NoContent();
        }

        //https://localhost:5050/api/Log/count
        [HttpGet("count")]
        public IActionResult GetLogCount()
        {
            try
            {
                int count = 2222;
                return Ok(count); // 정상적으로 개수를 반환
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLogCount: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("Read")]
        public IActionResult GetLogRead([FromQuery] bool result)
        {
            try
            {
                return Ok(result); // 정상적으로 개수를 반환
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLogCount: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public IEnumerable<LogData> GetLog([FromQuery] string channel, [FromQuery] string createDate)
        {
            return mediaService.GetLogDate(channel, createDate);
        }

        // DELETE: api/Log/{id} (로그 삭제)
        [HttpDelete("{crateDate}")]
        public IActionResult DeleteLog(DateTime crateDate)
        {
            
            return mediaService.DeleteLogDate(crateDate) == true ? Ok(): NoContent();
        }
    }

    // [Route("api/[controller]")]
    [Route("api/MediaInfo")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private MediaDbService mediaService = null;
        private readonly IHubContext<MediaHub> _hubContext;

        public MediaController(IHubContext<MediaHub> hubContext)
        {
            mediaService = new MediaDbService("media");
            _hubContext = hubContext;
        }
        // POST: api/MediaInfo (미디어 입력)
        [HttpPost]
        public IActionResult CreateMediaInfo([FromBody] MediaDataInfo media)
        {
            var jsonData = JsonConvert.SerializeObject(media);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // SignalR을 통해 클라이언트에 실시간으로 로그 정보 전송
            _hubContext.Clients.All.SendAsync("mediahub","Insert", jsonData);
            return mediaService.InsertMeidaInfo(media) == true ? Ok() : NoContent();   
        }

        [HttpGet("{id}")]
        public MediaDataInfo GetMediaInfo(string id)
        {
            return mediaService.GetMediaDataInfo(id);
        }


        // GET: api/MediaInfo/{id} (특정 미디어 조회)
        [HttpGet]
        public IEnumerable<MediaDataInfo> GetMediaInfo(string createDate, bool iscreateDate, string channel, string title)
        {
            return mediaService.GetMediaDate(createDate, iscreateDate, channel, title);   
        }

        // DELETE: api/MediaInfo/{id} (미디어 삭제)
        [HttpDelete("{id}")]
        public IActionResult DeleteMediaInfo(string id)
        {
            return mediaService.DeleteMeidaDate(id) == true ? Ok() : NoContent();  
        }

        [HttpPut("{id}")]
        public IActionResult UpDateMediaState(string id , string state)
        {
            var updateData = new { id = id, state = state };
            var jsonData = JsonConvert.SerializeObject(updateData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            _hubContext.Clients.All.SendAsync("mediahub", "UpDate", jsonData);
            return mediaService.UpDateMediaState(id, state) == true ? Ok() : NoContent();
        }

        [HttpPut("Inpoint/{id}")]
        public IActionResult UpDateInPoint(string id, int inPoint,  string inTimecode, int frame , string duration)
        {
            var updateData = new { id = id, inPoint = inPoint, inTimecode = inTimecode , frame = frame, duration = duration};
            var jsonData = JsonConvert.SerializeObject(updateData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            _hubContext.Clients.All.SendAsync("mediahub", "InTimecode", jsonData);
            return mediaService.UpDateInPoint(id, inPoint, inTimecode, frame, duration) == true ? Ok() : NoContent();
        }

        [HttpPut("outpoint/{id}")]
        public IActionResult UpDateOutPoint(string id, int outPoint, string outTimecode, int frame, string duration)
        {
            var updateData = new { id = id, outPoint = outPoint, outTimecode = outTimecode, frame = frame, duration = duration };
            var jsonData = JsonConvert.SerializeObject(updateData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            _hubContext.Clients.All.SendAsync("mediahub", "OutTimecode", jsonData);
            return mediaService.UpDateOutPoint(id, outPoint, outTimecode, frame, duration) == true ? Ok() : NoContent();
        }

        [HttpPut("duration/{id}")]
        public IActionResult UpDateDuration(string id, int frame, string duration)
        {
            return mediaService.UpDateDuration(id, frame, duration) == true ? Ok() : NoContent();
        }


    }

}
