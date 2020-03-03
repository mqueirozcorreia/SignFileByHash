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
    public class PdfSharpController : ControllerBase
    {
        public PdfSharpController(IFileProvider fileProvider)
        {
            this.FileProvider = fileProvider;

        }

        private IFileProvider FileProvider { get; set; }

        [HttpPost("Sign/{fileName}")]
        public ActionResult<string> Sign(string fileName)
        {
            FilesController.CheckFileNameArgument(fileName);

            var file = FileProvider.GetFileInfo($"{FileService.FILES_DIRECTORY}/{fileName}");
            Console.WriteLine($"Encontrou o arquivo {file.PhysicalPath}");

            var signer = new Server.Services.PdfSharp.PDFSigner(file.PhysicalPath,"Mateus");
            var destinyPDFSigned = $"{file.PhysicalPath}.signed.pdf";
            signer.Sign(destinyPDFSigned);
            Console.WriteLine($"Gerou a assinatura {destinyPDFSigned.Length}");

            return file.Name;
        }
    }
}
