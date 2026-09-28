// Protocol adapted from VLSub by Guillaume Le Maout (GPL-2.0-or-later).
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace SubClick.Core;

public static class XmlRpc
{
    public static XElement Value(object? value) => value switch
    {
        IDictionary<string, object> map => new("value", new XElement("struct", map.Select(p =>
            new XElement("member", new XElement("name", p.Key), Value(p.Value))))),
        object[] array => new("value", new XElement("array", new XElement("data", array.Select(Value)))),
        int integer => new("value", new XElement("int", integer)),
        _ => new("value", new XElement("string", Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""))
    };

    public static string Request(string method, params object[] args) => new XDocument(new XElement("methodCall",
        new XElement("methodName", method), new XElement("params", args.Select(a => new XElement("param", Value(a))))))
        .ToString(SaveOptions.DisableFormatting);

    public static object ParseResponse(string xml)
    {
        try
        {
            using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = SubtitleFiles.MaxBytes
            });
            var root = XDocument.Load(reader).Root;
            if (root?.Name != "methodResponse") throw new SubClickException("InvalidResponse");
            if (root.Element("fault") is not null) throw new SubClickException("ServiceError");
            return Read(root.Element("params")?.Element("param")?.Element("value") ?? throw new SubClickException("InvalidResponse"));
        }
        catch (XmlException ex) { throw new SubClickException("InvalidResponse", ex); }
    }

    private static object Read(XElement value)
    {
        var child = value.Elements().FirstOrDefault();
        if (child is null) return value.Value;
        return child.Name.LocalName switch
        {
            "struct" => child.Elements("member").ToDictionary(m => m.Element("name")?.Value ?? throw new SubClickException("InvalidResponse"),
                m => Read(m.Element("value") ?? throw new SubClickException("InvalidResponse"))),
            "array" => (child.Element("data") ?? throw new SubClickException("InvalidResponse")).Elements("value").Select(Read).ToArray(),
            "boolean" => child.Value == "1",
            _ => child.Value
        };
    }

    public static object? Get(object? map, string key) => map is Dictionary<string, object> d && d.TryGetValue(key, out var v) ? v : null;
    public static string Text(object? map, string key) => Convert.ToString(Get(map, key), CultureInfo.InvariantCulture) ?? "";
    public static IEnumerable<object> Items(object? array) => array is object[] items ? items : [];
}
