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
    public class ITextSharpController : ControllerBase
    {
        public ITextSharpController(IFileProvider fileProvider)
        {
            this.FileProvider = fileProvider;
        }

        private IFileProvider FileProvider { get; set; }

        // Sign api/Sign/1.txt
        [HttpPost("SignTextFile/{fileName}")]
        public ActionResult<string> SignTextFile(string fileName)
        {
            FilesController.CheckFileNameArgument(fileName);

            var file = FileProvider.GetFileInfo($"{FileService.FILES_DIRECTORY}/{fileName}");
            Console.WriteLine($"Encontrou o arquivo {file.PhysicalPath}");

            var destinyPDF = Server.Services.iTextSharp.PDFCreator.CreateFromText(file.PhysicalPath);
            Console.WriteLine($"Gerou o PDF {destinyPDF}");

            using (var signer = new Server.Services.iTextSharp.PDFSigner(destinyPDF,"Mateus"))
            {
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

        [HttpPost("SignPDF/{fileName}")]
        public ActionResult<string> SignPDF(string fileName)
        {
            FilesController.CheckFileNameArgument(fileName);

            var file = FileProvider.GetFileInfo($"{FileService.FILES_DIRECTORY}/{fileName}");
            Console.WriteLine($"Encontrou o arquivo {file.PhysicalPath}");

            var signer = new Server.Services.iTextSharp.PDFSigner(file.PhysicalPath,"Mateus");
            var hash = signer.GenerateHash();
            Console.WriteLine($"Gerou o Hash do arquivo {hash}");

            var signBytes = signer.SignHash(hash, "");
            Console.WriteLine($"Gerou a assinatura {signBytes.Length}");

            var destinyPDFSigned = $"{file.PhysicalPath}.signed.pdf";
            signer.SignPDFToNewFile(new byte[] {}, destinyPDFSigned);
            Console.WriteLine($"Gerou o arquivo assinado em disco {destinyPDFSigned}");

            return file.Name;
        }

        // Sign api/RemovePDFSignatures/1.pdf
        [HttpPost("RemovePDFSignatures/{fileName}")]
        public ActionResult<string> RemovePDFSignatures(string fileName)
        {
            FilesController.CheckFileNameArgument(fileName);

            var file = FileProvider.GetFileInfo($"{FileService.FILES_DIRECTORY}/{fileName}");
            Console.WriteLine($"Encontrou o arquivo {file.PhysicalPath}");

            Server.Services.iTextSharp.PDFSigner.RemoveSignatures(file.PhysicalPath, $"{file.PhysicalPath}.unsigned.pdf");
            Console.WriteLine($"Limpou as assinaturas");

            return file.Name;
        }
        
        // IsValid api/IsValid/1.pdf
        [HttpPost("IsValid/{fileName}")]
        public ActionResult<string> IsValid(string fileName)
        {
            FilesController.CheckFileNameArgument(fileName);

            var file = FileProvider.GetFileInfo($"{FileService.FILES_DIRECTORY}/{fileName}");
            Console.WriteLine($"Encontrou o arquivo {file.PhysicalPath}");

            using (var signer = new Server.Services.iTextSharp.PDFSigner(file.PhysicalPath,"Mateus"))
            {
                return signer.IsValid(file.PhysicalPath).ToString();
            }
        }
    }
}
