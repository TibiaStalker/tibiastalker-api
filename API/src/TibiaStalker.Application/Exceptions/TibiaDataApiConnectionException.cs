namespace TibiaStalker.Application.Exceptions;

public class TibiaDataApiConnectionException : TibiaStalkerException
{
    public TibiaDataApiConnectionException() : base("Error occurred while attempting to connect to TibiaData API. Please check https://tibiadata.com .")
    {
    }
}