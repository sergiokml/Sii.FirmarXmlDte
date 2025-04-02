using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Azure;
using Sii.FirmarXmlDte.Helper;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DigitalCertLoader>();
builder.Services.AddSingleton<XmlDteSign>();

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnection"]!);
});

WebApplication app = builder.Build();
app.UseHttpsRedirection();

app.MapPost(
    "/api/dte/firmar",
    async (DteBatchRequest input, DigitalCertLoader certLoader, XmlDteSign xmlSigner) =>
    {
        XmlDocument rootDoc = new();
        XmlElement root = rootDoc.CreateElement("FIRMADOS");
        rootDoc.AppendChild(root);

        XmlWriterSettings settings = new()
        {
            Indent = true,
            Encoding = Encoding.GetEncoding("ISO-8859-1"),
            OmitXmlDeclaration = false,
        };

        try
        {
            X509Certificate2 cert = await certLoader.LoadCertificateAsync();

            foreach (string dte in input.Dtes)
            {
                XmlDocument firmado = await xmlSigner.SignXmlAsync(dte, input.Caf, cert!);
                XmlNode imported = rootDoc.ImportNode(firmado.DocumentElement!, true);
                root.AppendChild(imported);
            }

            using MemoryStream ms = new();
            using XmlWriter writer = XmlWriter.Create(ms, settings);
            rootDoc.Save(writer);
            writer.Flush();
            string xmlContent = Encoding.GetEncoding("ISO-8859-1").GetString(ms.ToArray());

            return Results.Text(xmlContent, "application/xml; charset=iso-8859-1");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
);

await app.RunAsync();

public record DteBatchRequest(List<string> Dtes, string Caf);
