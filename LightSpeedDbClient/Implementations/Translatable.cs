using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class Translatable : ITranslatable
{
    
    private readonly Dictionary<Guid, string> _translations = new ();

    public string GetTranslation(Guid languageId)
    {
        _translations.TryGetValue(languageId, out string translation);
        if (translation == null)
            translation = "";
        return translation;
    }
    
}