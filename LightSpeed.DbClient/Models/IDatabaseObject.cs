namespace LightSpeed.DbClient.Models;

public interface IDatabaseObject: IDatabaseElement
{
    IKey Key();
    IDatabaseObjectTable Table(string name);
    
    void BeforeSave();
    void BeforeDelete();
    void BeforeGetReference();
    void BeforeGetObject();

}