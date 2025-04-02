[![](https://img.shields.io/badge/License-GPLv3-blue.svg?style=for-the-badge)](LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet?style=for-the-badge)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/w/sergiokml/Sii.FirmarXmlDte?style=for-the-badge)](https://github.com/sergiokml/Sii.FirmarXmlDte)
[![GitHub contributors](https://img.shields.io/github/contributors/sergiokml/Sii.FirmarXmlDte?style=for-the-badge)](https://github.com/sergiokml/Sii.FirmarXmlDte/graphs/contributors/)
[![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/sergiokml/Sii.FirmarXmlDte?style=for-the-badge)](https://github.com/sergiokml/Sii.FirmarXmlDte)
![GitHub last commit](https://img.shields.io/github/last-commit/sergiokml/Sii.FirmarXmlDte?style=for-the-badge)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/sergiokml/Sii.FirmarXmlDte?style=for-the-badge)
![GitHub Discussions](https://img.shields.io/github/discussions/sergiokml/Sii.FirmarXmlDte?style=for-the-badge)


# Firma digitalmente DTE`s

This solution allows you to digitally sign DTE (Electronic Tax Documents) from Chile's [Servicio de Impuestos Internos (SII)](https://www.sii.cl/). It uses a CAF (Folio Assignment Code) to build the TED (Electronic Stamp) and sign it using a `.pfx` certificate retrieved from Azure Blob Storage.

It includes:

- Minimal API for secure access to signing services  
- XML Digital Signature using a .pfx certificate  
- Digital certificate retrieval from Azure Blob Storage  
- Accepts a JSON request with a list of DTEs and a single CAF  
- Outputs XML using ISO-8859-1 encoding as required by SII  
- Supports multiple DTEs in a single API call  

> This repository has no relationship with the government entity [SII](https://www.sii.cl/), only for educational purposes.

---

### 📦 Details

| Package Reference            | Version |
|-----------------------------|:-------:|
| Azure.Storage.Blobs         | 12.24.0 |
| Microsoft.Extensions.Azure  | 1.7.6   |

---

### 🚀 Usage

Once the app is running (default on `http://localhost:5200`), you can sign DTEs via:

```bash
curl -X POST http://localhost:5200/api/dte/firmar \
  -H "Content-Type: application/json" \
  -d '{
    "dtes": ["<DTE>...</DTE>", "<DTE>...</DTE>"],
    "caf": "<AUTORIZACION>...</AUTORIZACION>"
  }'
```

The response will be:

```xml
<?xml version="1.0" encoding="iso-8859-1"?>
<FIRMADOS>
  <DTE>    
    <Signature>...</Signature>
  </DTE>
  <DTE>    
    <Signature>...</Signature>
  </DTE>
</FIRMADOS>
```

### ⚙️ Configuration

Use `appsettings.json` or environment variables to configure the certificate source:

```json
{
  "StorageConnection": "UseDevelopmentStorage=true",
  "StorageConnection:ContainerName": "certificados",
  "StorageConnection:BlobName": "certificado1.pfx",
  "StorageConnection:CertPassword": "<your-cert-password>"
}
```

You may also define these as [Azure App Settings](https://learn.microsoft.com/en-us/azure/app-service/configure-common) if you're deploying the API to the cloud.

---

### 🔒 Security Disclaimer

> This project involves handling digital certificates and calling secure external services (SOAP). Make sure to store sensitive credentials such as certificate passwords securely using `secrets.json`, environment variables, or Azure App Settings. Never commit your certificates or passwords to the repository.

---

### 📢 Have a question? Found a Bug?

Feel free to **file a new issue** with a respective title and description on the [TokenAuth/issues](https://github.com/sergiokml/TokenAuth/issues) repository.

---

### ❤️ Community and Contributions

I think that **Knowledge Doesn’t Belong to Just Any One Person**, and I always intend to share my knowledge with other developers. A voluntary monetary contribution or your ideas/comments to improve these tools would be appreciated.

[![PayPal](https://img.shields.io/badge/PayPal-00457C?style=for-the-badge&logo=paypal&logoColor=white)](https://www.paypal.com/donate/?hosted_button_id=PTKX9BNY96SNJ)

---

### 📘 License

This repository is released under the [GNU General Public License v3.0](LICENSE.txt).

> ⚠️ This repository is for educational purposes and has no official affiliation with Chile's SII.

