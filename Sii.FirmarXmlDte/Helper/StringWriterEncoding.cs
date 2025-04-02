using System.Text;

namespace Sii.FirmarXmlDte.Helper;

public class StringWriterEncoding : StringWriter
{
    private readonly Encoding _encoding;

    public StringWriterEncoding(Encoding encoding)
    {
        _encoding = encoding;
    }

    public override Encoding Encoding => _encoding;
}
