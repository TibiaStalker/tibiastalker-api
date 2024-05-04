using TibiaStalker.Application.TibiaData.Dtos;

namespace ChangeNameDetector.Validators;

public class NameDetectorValidator : INameDetectorValidator
{
    public bool IsCharacterChangedName(CharacterResult fetchedCharacter, string characterName)
    {
        return fetchedCharacter?.Name?.ToLower() != characterName;
    }

    public bool IsCharacterTraded(CharacterResult fetchedCharacter)
    {
        return fetchedCharacter.Traded;
    }

    public bool IsCharacterExist(CharacterResult fetchedCharacter)
    {
        return !string.IsNullOrWhiteSpace(fetchedCharacter.Name);
    }
}