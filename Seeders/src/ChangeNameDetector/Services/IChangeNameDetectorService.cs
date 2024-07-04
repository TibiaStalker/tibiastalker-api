namespace ChangeNameDetector.Services;

public interface IChangeNameDetectorService
{
    Task<bool> Run();
}