namespace TibiaStalker.Application.Exceptions;

public class TibiaDataApiEmptyResponseException : TibiaStalkerException
{
    public TibiaDataApiEmptyResponseException() : base("Tibia Data Api response with empty object and status code 200.")
    {
    }
}