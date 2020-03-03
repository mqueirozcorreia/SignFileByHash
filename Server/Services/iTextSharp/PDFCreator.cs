using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Server.Services.iTextSharp
{
    public class PDFCreator
    {
        public static string CreateFromText(string source)
        {
            string destiny = $"{source}.pdf";
            
            using (StreamReader rdr = new StreamReader(source))
            {
                Document doc = new Document();
    
                PdfWriter.GetInstance(doc, new  FileStream(destiny, FileMode.Create));

                doc.Open();
                doc.Add(new Paragraph(rdr.ReadToEnd()));
                doc.Close();
            }

            return destiny;
        }
    }
}