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
            Console.WriteLine($"Encontrou o arquivo {file.PhysicalPath}");

            var destinyPDF = Server.Services.iTextSharp.PDFCreator.CreateFromText(file.PhysicalPath);
            Console.WriteLine($"Gerou o PDF {destinyPDF}");

            var signer = new Server.Services.iTextSharp.PDFSigner(destinyPDF,"Mateus");
            var hash = signer.GenerateHash();
            Console.WriteLine($"Gerou o Hash do arquivo {hash}");

            var signBytes = signer.SignHash(hash, "");
            Console.WriteLine($"Gerou a assinatura {signBytes.Length}");

            var destinyPDFSigned = $"{destinyPDF}.signed.pdf";
            signer.SignPDFToNewFile(signBytes, destinyPDFSigned);
            Console.WriteLine($"Gerou o arquivo assinado em disco {destinyPDFSigned}");

            return file.Name;
        }

        // Sign api/SignPDF/1.txt
        [HttpPost("SignPDFWithITextSharp/{fileName}")]
        public ActionResult<string> SignPDFWithITextSharp(string fileName)
        {
            CheckFileNameArgument(fileName);

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

        // Sign api/SignPDF/1.txt
        [HttpPost("SignPDFWithPdfSharp/{fileName}")]
        public ActionResult<string> SignPDFWithPdfSharp(string fileName)
        {
            CheckFileNameArgument(fileName);

            var file = FileProvider.GetFileInfo($"{FileService.FILES_DIRECTORY}/{fileName}");
            Console.WriteLine($"Encontrou o arquivo {file.PhysicalPath}");

            var signer = new Server.Services.PdfSharp.PDFSigner(file.PhysicalPath,"Mateus");
            var destinyPDFSigned = $"{file.PhysicalPath}.signed.pdf";
            signer.Sign(destinyPDFSigned);
            Console.WriteLine($"Gerou a assinatura {destinyPDFSigned.Length}");

            return file.Name;
        }
        // Sign api/SignPDF/1.txt
        [HttpPost("RemovePDFSignatures/{fileName}")]
        public ActionResult<string> RemovePDFSignatures(string fileName)
        {
            CheckFileNameArgument(fileName);

            var file = FileProvider.GetFileInfo($"{FileService.FILES_DIRECTORY}/{fileName}");
            Console.WriteLine($"Encontrou o arquivo {file.PhysicalPath}");

            //var signer = new PDFSigner(file.PhysicalPath,"Mateus");
            Server.Services.iTextSharp.PDFSigner.RemoveSignatures(file.PhysicalPath, $"{file.PhysicalPath}.unsigned.pdf");
            Console.WriteLine($"Limpou as assinaturas");

            return file.Name;
        }
    }
}
