using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Pkcs;

namespace Server.Services
{
    public class PDFSigner
    {
        private PdfReader reader;
        private MemoryStream ms;
        private PdfStamper stamper;
        private PdfSignatureAppearance appearance;
        private string userName;
        private const int RESERVED_SPACE_SIGNATURE = 2048;

        public PDFSigner(byte[] Content, string UserName) : 
            this(new PdfReader(Content), UserName)
        {
        }

        public PDFSigner(string fileName, string UserName): 
            this(new PdfReader(fileName), UserName)
        {
        }

        public PDFSigner(PdfReader Reader, string UserName)
        {
            reader = Reader;
            ms = new MemoryStream();
            stamper = PdfStamper.CreateSignature(reader, ms, '\0');
            appearance = stamper.SignatureAppearance;
            userName = UserName;
        }

        private Stream getCertificate()
        {
            return File.OpenRead("wwwroot/mateus.pfx");
        }

        public string GenerateHash()
        {
            string Reason = "Motivo";
            string Location = "Localização";
            string Contact = "Contato";

            string signatureFieldName = null;
            appearance.SetVisibleSignature(new Rectangle(500, 150, 400, 200), 1, signatureFieldName);
            appearance.SignDate = DateTime.Now;
            appearance.Reason = Reason;
            appearance.Location = Location;
            appearance.Contact = Contact;
            StringBuilder buf = new StringBuilder();
            buf.Append("Digitally signed by");
            buf.Append("\n");
            buf.Append(userName);
            buf.Append("\n");
            buf.Append("Date: " + appearance.SignDate);
            appearance.Layer2Text = buf.ToString();
            appearance.Acro6Layers = true;
            appearance.CertificationLevel = 0;
            PdfSignature dic = new PdfSignature(PdfName.ADOBE_PPKLITE, PdfName.ADBE_PKCS7_DETACHED)
            {
                Date = new PdfDate(appearance.SignDate),
                Name = userName
            };
            dic.Reason = appearance.Reason;
            dic.Location = appearance.Location;
            dic.Contact = appearance.Contact;

            appearance.CryptoDictionary = dic;

            Dictionary<PdfName, int> exclusionSizes = new Dictionary<PdfName, int>();
            exclusionSizes.Add(PdfName.CONTENTS, (RESERVED_SPACE_SIGNATURE * 2) + 2);
            appearance.PreClose(exclusionSizes);

            HashAlgorithm sha = new SHA256CryptoServiceProvider();
            Stream s = appearance.GetRangeStream();
            int read = 0;
            byte[] buff = new byte[0x2000];
            while ((read = s.Read(buff, 0, 0x2000)) > 0)
            {
                sha.TransformBlock(buff, 0, read, buff, 0);
            }
            sha.TransformFinalBlock(buff, 0, 0);

            StringBuilder hex = new StringBuilder(sha.Hash.Length * 2);
            foreach (byte b in sha.Hash)
                hex.AppendFormat("{0:x2}", b);

            return hex.ToString();
        }

        public byte[] SignHash(string hexhash, string password)
        {
            byte[] hash = StringToByteArray(hexhash);
            Pkcs12Store store = new Pkcs12Store(getCertificate(), password.ToCharArray());
            String alias = "";
            foreach (string al in store.Aliases)
                if (store.IsKeyEntry(al) && store.GetKey(al).Key.IsPrivate)
                {
                    alias = al;
                    break;
                }
            AsymmetricKeyEntry pk = store.GetKey(alias);
            X509CertificateEntry[] chain = store.GetCertificateChain(alias);
            List<Org.BouncyCastle.X509.X509Certificate> c = new List<Org.BouncyCastle.X509.X509Certificate>();
            foreach (X509CertificateEntry en in chain)
            {
                c.Add(en.Certificate);
            }
            PrivateKeySignature signature = new PrivateKeySignature(pk.Key, "SHA256");
            String hashAlgorithm = signature.GetHashAlgorithm();
            PdfPKCS7 sgn = new PdfPKCS7(null, c, hashAlgorithm, false);
            DateTime signingTime = DateTime.Now;
            byte[] sh = sgn.getAuthenticatedAttributeBytes(hash, null, null, CryptoStandard.CMS);
            byte[] extSignature = signature.Sign(sh);
            sgn.SetExternalDigest(extSignature, null, signature.GetEncryptionAlgorithm());
            return sgn.GetEncodedPKCS7(hash, null, null, null, CryptoStandard.CMS);
        }

        private static byte[] StringToByteArray(string hex) 
        {
            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
        }

        public byte[] SignPDFToMemory(byte[] pk)
        {
            byte[] paddedSig = new byte[RESERVED_SPACE_SIGNATURE];
            System.Array.Copy(pk, 0, paddedSig, 0, pk.Length);

            PdfDictionary dic2 = new PdfDictionary();
            dic2.Put(PdfName.CONTENTS, new PdfString(paddedSig).SetHexWriting(true));
            appearance.Close(dic2);

            return ms.ToArray();
        }

        public void SignPDFToNewFile(byte[] pk, string destinyPDFSigned)
        {
            var pdfInMemory = SignPDFToMemory(pk);

            File.WriteAllBytes(destinyPDFSigned, pdfInMemory);
        }
    }
}