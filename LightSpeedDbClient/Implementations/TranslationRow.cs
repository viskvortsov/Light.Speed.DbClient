using LightSpeedDbClient.Attributes;

namespace LightSpeedDbClient.Implementations;

public class TranslationRow : DatabaseObjectTableElement
{
    
    [Column(name: "language_id")]
    public Guid LanguageId { get; set; }
    
    [OwnerKey(relation: "id")]
    [Column(name: "source_id")]
    public Guid SourceId { get; set; }
    
    [Column(name: "content_id")]
    public Guid ContentId { get; set; }
    
    [Column(name: "content")]
    public String Content { get; set; }
    
}