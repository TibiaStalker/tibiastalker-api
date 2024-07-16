namespace TibiaStalker.Application.Exceptions;

public class EntityAlreadyExistsException : TibiaStalkerException
{
    public EntityAlreadyExistsException(string entityName) : base(BuildErrorMessage(entityName))
    {
    }

    private static string BuildErrorMessage(string propertyName)
    {
        return $"Entity ({propertyName}) already exists.";
    }
}