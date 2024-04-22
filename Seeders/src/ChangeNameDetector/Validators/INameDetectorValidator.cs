using TibiaStalker.Application.TibiaData.Dtos;
using TibiaStalker.Domain.Entities;

namespace ChangeNameDetector.Validators;

public interface INameDetectorValidator
{
    public bool IsCharacterChangedName(CharacterResult fechedCharacter, Character character);
    public bool IsCharacterTraded(CharacterResult fechedCharacter);
    public bool IsCharacterExist(CharacterResult fechedCharacter);
}