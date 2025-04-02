using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace Sii.FirmarXmlDte.Helper;

public class XmlDteSign
{
    public async Task<XmlDocument> SignXmlAsync(string dteXml, string cafXml, X509Certificate2 cert)
    {
        XmlDocument doc = new() { PreserveWhitespace = true };
        doc.LoadXml(dteXml);

        string id =
            doc.GetElementsByTagName("Documento")[0]?.Attributes?["ID"]?.Value
            ?? throw new Exception("Documento ID not found");

        XmlNamespaceManager ns = new(doc.NameTable);
        ns.AddNamespace("dte", "http://www.sii.cl/SiiDte");

        string re = doc.SelectSingleNode("//dte:RUTEmisor", ns)?.InnerText ?? "";
        string td = doc.SelectSingleNode("//dte:TipoDTE", ns)?.InnerText ?? "";
        string folio = doc.SelectSingleNode("//dte:Folio", ns)?.InnerText ?? "";
        string fecha = doc.SelectSingleNode("//dte:FchEmis", ns)?.InnerText ?? "";
        string rr = doc.SelectSingleNode("//dte:RUTRecep", ns)?.InnerText ?? "";
        string rsr = doc.SelectSingleNode("//dte:RznSocRecep", ns)?.InnerText ?? "";
        string mnt = doc.SelectSingleNode("//dte:MntTotal", ns)?.InnerText ?? "";
        string it1 = doc.SelectSingleNode("//dte:NmbItem", ns)?.InnerText ?? "";

        XmlDocument cafDoc = new();
        cafDoc.LoadXml(cafXml);

        XmlDocument dd = new();
        XmlElement root = dd.CreateElement("DD");
        dd.AppendChild(root);

        void Append(string tag, string value)
        {
            XmlElement el = dd.CreateElement(tag);
            el.InnerText = value;
            root.AppendChild(el);
        }

        Append("RE", re);
        Append("TD", td);
        Append("F", folio);
        Append("FE", fecha);
        Append("RR", rr);
        Append("RSR", rsr.Length > 40 ? rsr[..40] : rsr);
        Append("MNT", mnt);
        Append("IT1", it1.Length > 40 ? it1[..40] : it1);

        XmlNode cafOriginal = cafDoc.SelectSingleNode("//CAF")!;
        XmlElement cafClean = dd.CreateElement("CAF");
        cafClean.SetAttribute("version", "1.0");

        foreach (XmlNode child in cafOriginal.ChildNodes)
        {
            XmlNode imported = dd.ImportNode(child, true);
            cafClean.AppendChild(imported);
        }

        root.AppendChild(cafClean);
        Append("TSTED", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));

        string frmtValue = SignTedFrmt(dd, cafDoc);

        XmlElement ted = doc.CreateElement("TED", doc.DocumentElement!.NamespaceURI);
        ted.SetAttribute("version", "1.0");
        ted.AppendChild(doc.ImportNode(dd.DocumentElement!, true));

        XmlElement frmt = doc.CreateElement("FRMT", doc.DocumentElement.NamespaceURI);
        frmt.InnerText = frmtValue;
        frmt.SetAttribute("algoritmo", "SHA1withRSA");
        ted.AppendChild(frmt);

        XmlNode? documentoNode = doc.GetElementsByTagName("Documento")[0];
        documentoNode?.AppendChild(ted);

        XmlElement timestamp = doc.CreateElement("TmstFirma", doc.DocumentElement.NamespaceURI);
        timestamp.InnerText = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        documentoNode?.AppendChild(timestamp);

        XmlDocument signed = SignXmlById(doc, cert, id);

        XmlDocument formatted = new();
        using (MemoryStream ms = new())
        {
            XmlWriterSettings settings = new()
            {
                Indent = true,
                Encoding = Encoding.GetEncoding("ISO-8859-1"),
                OmitXmlDeclaration = false,
                Async = true,
            };
            using XmlWriter writer = XmlWriter.Create(ms, settings);
            signed.Save(writer);
            await writer.FlushAsync();
            formatted.LoadXml(Encoding.GetEncoding("ISO-8859-1").GetString(ms.ToArray()));
        }

        // Replace xmlns="" if it appears in DD or CAF
        string cleaned = formatted
            .OuterXml.Replace("<DD xmlns=\"\">", "<DD>")
            .Replace("<CAF version=\"1.0\" xmlns=\"\">", "<CAF version=\"1.0\">");

        XmlDocument final = new();
        final.LoadXml(cleaned);
        return final;
    }

    private static XmlDocument SignXmlById(
        XmlDocument xmlDoc,
        X509Certificate2 cert,
        string referenceId
    )
    {
        SignedXml signedXml = new(xmlDoc) { SigningKey = cert.GetRSAPrivateKey() };

        Reference reference = new("#" + referenceId);
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        signedXml.AddReference(reference);

        signedXml.SignedInfo!.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
        reference.DigestMethod = SignedXml.XmlDsigSHA1Url;

        KeyInfo keyInfo = new();
        keyInfo.AddClause(new RSAKeyValue(cert.GetRSAPrivateKey()!));
        keyInfo.AddClause(new KeyInfoX509Data(cert));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();
        XmlElement xmlDigitalSignature = signedXml.GetXml();

        xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        return xmlDoc;
    }

    private static string SignTedFrmt(XmlDocument dd, XmlDocument cafDoc)
    {
        string privateKeyPem =
            cafDoc.GetElementsByTagName("RSASK")[0]?.InnerText?.Trim()
            ?? throw new Exception("No se encontró la clave privada en el CAF.");

        string pemClean = privateKeyPem
            .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
            .Replace("-----END RSA PRIVATE KEY-----", "")
            .Replace("\r", "")
            .Replace("\n", "");

        byte[] privateKeyBytes = Convert.FromBase64String(pemClean);

        using RSA rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

        StringBuilder sbFlattened = new();
        using (StringWriter sw = new(new StringBuilder()))
        using (
            XmlWriter xw = XmlWriter.Create(
                sw,
                new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true }
            )
        )
        {
            dd.Save(xw);
            xw.Flush();
            sbFlattened.Append(sw.ToString());
        }

        byte[] ddBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(sbFlattened.ToString());
        byte[] signature = rsa.SignData(ddBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signature);
    }
}
