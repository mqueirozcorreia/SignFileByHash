using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Server.Services;

namespace SignFile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        public FilesController(IFileProvider fileProvider)
        {
            this.FileProvider = fileProvider;
        }

        private IFileProvider FileProvider { get; set; }


        // GET api/values
        [HttpGet]
        public IEnumerable<IFileInfo> Get()
        {
            IDirectoryContents contents = FileProvider.GetDirectoryContents(FileService.FILES_DIRECTORY);

            var files =
                        contents
                        .OrderByDescending(f => f.LastModified)
                        .ToList();

            return files;
        }

        // GET api/values/5
        [HttpGet("{fileName}")]
        public ActionResult<string> Get(string fileName)
        {
            CheckFileNameArgument(fileName);

            var file = FileProvider.GetFileInfo($"{FileService.FILES_DIRECTORY}/{fileName}");

            return file.Name;
        }

        public static void CheckFileNameArgument(string fileName)
        {
            if (fileName.StartsWith(".") ||
                fileName.StartsWith("/") ||
                fileName.StartsWith("\\"))
                throw new ArgumentException("FileName inválido", nameof(fileName));
        }
    }
}
