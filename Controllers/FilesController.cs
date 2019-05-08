using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LinccubeApi.Model;
using Microsoft.AspNetCore.Mvc;
using MimeTypes.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LinccubeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {

        // GET api/files
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/files/filename
        [HttpGet("{name}", Name = "Get")]
        public async Task<ActionResult<FileModel>> Get(string name)
        {
            FileModel savedfile = new FileModel();
            //Json 
            var pathTemp = Path.GetTempPath();
            var jsonfileName = name + ".json";
            var jsonfullPath = Path.Combine(pathTemp, jsonfileName);

            Task TaskGetFile = new Task(() =>
            {
                using (StreamReader reader = System.IO.File.OpenText(jsonfullPath))
                {
                    JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    var fileName = name + MimeTypeMap.GetExtension((string)o["MimeType"]);
                    var fullPath = Path.Combine(pathTemp, fileName);
                    savedfile.FileName = name;
                    savedfile.MimeType = (string)o["MimeType"];
                }
            });

            TaskGetFile.Start();

            try
            {
                // we wait for complete all tasks
                await Task.WhenAll(TaskGetFile);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }

            return Ok(savedfile);
        }


        // POST: api/Files
        [HttpPost]
        public async Task<ActionResult<FileModel>> Post([FromBody] FileModel item)
        {
            var pathTemp = Path.GetTempPath();
            var fileName = item.FileName + MimeTypeMap.GetExtension(item.MimeType);
            var fullPath = Path.Combine(pathTemp, fileName);

            // Create the file.

            Task TaskSaveFile1 = new Task(async () =>
            {
                using (FileStream fs = System.IO.File.Create(fullPath))
                {
                    await fs.WriteAsync(item.Content, 0, item.Content.Length);
                }
            }
            );

            // Create json file.
            var jsonfileName = item.FileName + ".json";
            var jsonfullPath = Path.Combine(pathTemp, jsonfileName);

            JObject newJsonObject = new JObject(
                new JProperty("fileName", item.FileName),
                new JProperty("Content", item.Content),
                new JProperty("MimeType", item.MimeType));

            JObject newJsonObject1 = (JObject)JToken.FromObject(item);

            Task TaskSaveFile2 = new Task(async () =>
            {
                using (StreamWriter file = System.IO.File.CreateText(jsonfullPath))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    await newJsonObject1.WriteToAsync(writer);
                }
            }
            );

            TaskSaveFile1.Start();
            TaskSaveFile2.Start();

            try
            {
                // we wait for complete all tasks
                await Task.WhenAll(TaskSaveFile1, TaskSaveFile2);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }

            return Ok();
        }

    }
}
