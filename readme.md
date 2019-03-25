# Executar o Server:
cd Server
dotnet run Server

# Navegar:
Abra um browser e vá no Swagger:
https://localhost:5001/swagger/index.html

Você poderá ver a assinatura do arquivo "1.txt" ou "2.txt" navegando nestes respectivos links:
https://localhost:5001/api/Files/Sign/1.txt
https://localhost:5001/api/Files/Sign/2.txt


# Licenciamento
Para ter informação de licenciamento do iTextSharp [veja no site](https://itextpdf.com/en/how-buy)

Há versões de comunidade como 
https://stackoverflow.com/a/2655113/3424212
https://www.nuget.org/packages/iTextSharp-LGPL/

# Inspirações:
[Codigo Exemplo Stackoverflow](https://stackoverflow.com/a/7475985/3424212)

[PDF Signature - Embed separatly signed hash](https://stackoverflow.com/a/43672139/3424212)

[E-signing PDF documents with iTextSharp](https://www.codeproject.com/Articles/14488/E-signing-PDF-documents-with-iTextSharp)

[ebook iTextSharp](https://itextpdf.com/sites/default/files/2018-12/digitalsignatures20130304.pdf)

[MakeCert para gerar certificado no Windows](https://docs.microsoft.com/en-us/windows/desktop/SecCrypto/makecert)

[Para gerar certificado no Linux](https://andrewlock.net/creating-and-trusting-a-self-signed-certificate-on-linux-for-use-in-kestrel-and-asp-net-core/)

[How to add digital signature in pdf using iTextSharp](http://c-sharpcode.com/thread/how-to-add-digital-signature-in-pdf-using-itextsharp/)

[How would I validate digital signature for PDFs in linux?](https://superuser.com/a/1330627/620199)