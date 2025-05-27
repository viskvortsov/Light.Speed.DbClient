using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class TranslationRow : DatabaseObjectTableElement
{
    
    [Column(name: "language_id")]
    public Guid LanguageId { get; set; }
    
    [OwnerKey(relation: "id")]
    [Column(name: "source_id")]
    public TranslationKey SourceId { get; set; }
    
    [Column(name: "content_id")]
    public Guid ContentId { get; set; }
    
    [Column(name: "content")]
    public String Content { get; set; }

    public TranslationRow() : base(Models.ModelType.Row)
    {
    }
    public TranslationRow(ModelType modelType) : base(modelType)
    {
    }

    public class TranslationKey
    {
        public Type Type { get; }
        public int IntId { get; }
        public Guid GuidId { get; }
        public bool IsGuid { get; }
        public bool IsInt { get; }
        
        public TranslationKey(object id)
        {
            IsInt = false;
            IsGuid = false;
            if (id is Guid guidId)
            {
                GuidId = guidId;
                IsGuid = true;
            }
            if (id.GetType().IsEnum || id is int)
            {
                IntId = (int) id;
                IsInt = true;
            }
        }
        
    }
    
}