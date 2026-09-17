using System.Text.Json;
using System.Security.Cryptography;
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
foreach (var file in args) {
  using var doc=JsonDocument.Parse(File.ReadAllText(file));
  var root=doc.RootElement;
  var relations=root.GetProperty("relations").EnumerateArray().Select(r => new {
    FromId=r.GetProperty("fromId").GetString(), ToId=r.GetProperty("toId").GetString(), RelationType=r.GetProperty("relationType").GetString()
  }).ToArray();
  var anchor=new {items=root.GetProperty("items"), relations};
  var bytes=JsonSerializer.SerializeToUtf8Bytes(anchor, options);
  Console.WriteLine(JsonSerializer.Serialize(new {file, fingerprint=Convert.ToHexString(SHA256.HashData(bytes))[..16].ToLowerInvariant(),items=root.GetProperty("items").GetArrayLength(),relations=relations.Length}));
}
