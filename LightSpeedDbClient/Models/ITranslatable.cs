namespace LightSpeedDbClient.Models;

public interface ITranslatable
{
    
    string GetTranslation(Guid languageId);
    
}