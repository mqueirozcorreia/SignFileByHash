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

        private static void CheckFileNameArgument(string fileName)
        {
            if (fileName.StartsWith(".") ||
                fileName.StartsWith("/") ||
                fileName.StartsWith("\\"))
                throw new ArgumentException("FileName inválido", nameof(fileName));
        }


        // Sign api/Sign/1.txt
        [HttpPost("Sign/{fileName}")]
        public ActionResult<string> Sign(string fileName)
        {
            CheckFileNameArgument(fileName);

            var file = FileProvider.GetFileInfo($"{FileService.FILES_DIRECTORY}/{fileName}");
            Console.WriteLine($"Encontrou o arquivo {file.physicalPath}");

            var destinyPDF = PDFCreator.CreateFromText(file.PhysicalPath);
            Console.WriteLine($"Gerou o PDF {destinyPDF}");

            var signer = new PDFSigner(destinyPDF,"Mateus");
            var hash = signer.GenerateHash();
            Console.WriteLine($"Gerou o Hash do arquivo {hash}");

            var signBytes = signer.SignHash(hash, "");
            Console.WriteLine($"Gerou a assinatura {signBytes.Length}");

            var destinyPDFSigned = $"{destinyPDF}.signed.pdf";
            signer.SignPDFToNewFile(signBytes, destinyPDFSigned);
            Console.WriteLine($"Gerou o arquivo assinado em disco {destinyPDFSigned}");

            return file.Name;
        }
    }
}
