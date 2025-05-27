using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class Translatable(Guid id) : ITranslatable
{
    private readonly Dictionary<Guid, string> _translations = new ();

    public Translatable() : this(Guid.NewGuid())
    {
    }

    public void AddTranslation(Guid languageId, string text)
    {
        if (!_translations.TryAdd(languageId, text))
        {
            _translations.Remove(languageId);
            _translations.Add(languageId, text);
        }
    }

    public string GetTranslation(Guid languageId)
    {
        _translations.TryGetValue(languageId, out string? translation);
        return translation ?? "";
    }

    public Guid GetId()
    {
        return id;
    }

    public Dictionary<Guid, string> AllTranslations()
    {
        return _translations;
    }
    
}