[![](https://img.shields.io/badge/License-GPLv3-blue.svg?style=for-the-badge)](LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet?style=for-the-badge)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/w/sergiokml/Sii.FirmarXmlDte?style=for-the-badge)](https://github.com/sergiokml/Sii.FirmarXmlDte)
[![GitHub contributors](https://img.shields.io/github/contributors/sergiokml/Sii.FirmarXmlDte?style=for-the-badge)](https://github.com/sergiokml/Sii.FirmarXmlDte/graphs/contributors/)
[![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/sergiokml/Sii.FirmarXmlDte?style=for-the-badge)](https://github.com/sergiokml/Sii.FirmarXmlDte)

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

Once the app is running, you can sign DTEs via:

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

### 📢 Have a question? Found a Bug?

Feel free to **file a new issue** with a respective title and description on the [Sii.FirmarXmlDte/issues](https://github.com/sergiokml/FirmarXmlDte/issues) repository.

---

### 💖 Community and Contributions

If this tool was useful, consider contributing with ideas or improving it further.


<p align="center">
    <a href="https://www.paypal.com/donate/?hosted_button_id=PTKX9BNY96SNJ" target="_blank">
        <img width="12%" src="https://img.shields.io/badge/PayPal-00457C?style=for-the-badge&logo=paypal&logoColor=white" alt="Azure Function">
    </a>
</p>

---

### 📘 License

This repository is released under the [GNU General Public License v3.0](LICENSE.txt).


