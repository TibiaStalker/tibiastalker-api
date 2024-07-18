using TibiaStalker.Application.TibiaData.Dtos;

namespace ChangeNameDetectorSubscriber.Validators;

public interface INameDetectorValidator
{
    public bool IsCharacterChangedName(CharacterResult fetchedCharacter, string characterName);
    public bool IsCharacterTraded(CharacterResult fetchedCharacter);
    public bool IsCharacterExist(CharacterResult fetchedCharacter);
}