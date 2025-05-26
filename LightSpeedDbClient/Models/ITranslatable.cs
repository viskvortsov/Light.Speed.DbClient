namespace LightSpeedDbClient.Models;

public interface ITranslatable
{

    void AddTranslation(Guid languageId, string text);
    string GetTranslation(Guid languageId);
    
}