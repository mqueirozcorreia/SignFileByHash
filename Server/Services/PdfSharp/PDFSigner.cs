using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

namespace Server.Services.PdfSharp
{
    public class PDFSigner
    {
        public PdfDocument document;

        public PDFSigner(string fileName, string UserName)
        {
            // Open an existing document. Providing an unrequired password is ignored.
            var document = PdfReader.Open(fileName, "some text");
        }

        public void Sign(string filenameDest)
        {
            PdfSecuritySettings securitySettings = document.SecuritySettings;
            
            // Setting one of the passwords automatically sets the security level to 
            // PdfDocumentSecurityLevel.Encrypted128Bit.
            securitySettings.UserPassword  = "user";
            securitySettings.OwnerPassword = "owner";
            
            // Don't use 40 bit encryption unless needed for compatibility
            //securitySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted40Bit;
            
            // Restrict some rights.
            securitySettings.PermitAccessibilityExtractContent = false;
            securitySettings.PermitAnnotations = false;
            securitySettings.PermitAssembleDocument = false;
            securitySettings.PermitExtractContent = false;
            securitySettings.PermitFormsFill = true;
            securitySettings.PermitFullQualityPrint = false;
            securitySettings.PermitModifyDocument = true;
            securitySettings.PermitPrint = false;
            
            // Save the document...
            document.Save(filenameDest);
            // ...and start a viewer.
        }
    }
}