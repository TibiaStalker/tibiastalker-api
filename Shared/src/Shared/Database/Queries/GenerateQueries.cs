﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool (GenerateQueries).
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//
//     To regenerate this file, please click 'Run Template' at GenerateQueries.tt file.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shared.Database.Queries.Sql
{
    public static class GenerateQueries
    {
        public const string CreateCharactersIfNotExists = @"INSERT INTO characters (name, world_id)
SELECT DISTINCT ca.character_name, ca.world_id
FROM character_actions ca
LEFT JOIN characters c ON ca.character_name = c.name
WHERE c.name IS NULL;";

        /// <summary>
        /// Required parameters: 
        ///    @Available
        /// </summary>
        public const string GetActiveWorlds = @"SELECT name, url, is_available
FROM worlds
WHERE (@Available IS NULL OR is_available = @Available)
ORDER BY name";

        /// <summary>
        /// Required parameters: 
        ///    @Page
        ///    @PageSize
        ///    @SearchText
        /// </summary>
        public const string GetFilteredCharacterNames = @"SELECT c.name
FROM characters c
WHERE c.name LIKE '%' || @SearchText || '%'
ORDER BY c.name
OFFSET ((@Page - 1) * @PageSize) ROWS
    LIMIT @PageSize;";

        /// <summary>
        /// Required parameters: 
        ///    @SearchText
        /// </summary>
        public const string GetFilteredCharactersCount = @"SELECT COUNT(*) AS TotalCount
FROM characters c
WHERE c.name LIKE '%' || @SearchText || '%';";

        /// <summary>
        /// Required parameters: 
        ///    @Page
        ///    @PageSize
        ///    @SearchText
        /// </summary>
        public const string GetFilteredCharactersStartsAtSearchText = @"SELECT c.name
FROM characters c
WHERE c.name >= @SearchText
ORDER BY c.name
OFFSET ((@Page - 1) * @PageSize) ROWS
    LIMIT @PageSize;";

        /// <summary>
        /// Required parameters: 
        ///    @CharacterName
        /// </summary>
        public const string GetOtherPossibleCharacters = @"WITH character_id_CTE AS (SELECT character_id FROM characters WHERE name = @CharacterName)

SELECT DISTINCT c.name AS other_character_name, number_of_matches, create_date, last_match_date FROM character_correlations cc 
JOIN characters c ON c.character_id = cc.login_character_id OR c.character_id = cc.logout_character_id 
WHERE (logout_character_id = (SELECT character_id FROM character_id_CTE) OR login_character_id = (SELECT character_id FROM character_id_CTE)) 
AND c.character_id <> (SELECT character_id FROM character_id_CTE)
ORDER BY number_of_matches DESC LIMIT 10";

        /// <summary>
        /// Required parameters: 
        ///    @p0
        /// </summary>
        public const string GetSameCharacterCorrelations = @"WITH cte AS (SELECT * FROM character_correlations cc WHERE cc.logout_character_id = @p0 OR cc.login_character_id = @p0)
SELECT DISTINCT ON (cc1.logout_character_id, cc1.login_character_id)
    json_build_object(
            'FirstCombinedCorrelation', json_build_object(
            'CorrelationId', cc1.correlation_id,
            'LogoutCharacterId', cc1.logout_character_id,
            'LoginCharacterId', cc1.login_character_id,
            'NumberOfMatches', cc1.number_of_matches,
            'CreateDate', cc1.create_date,
            'LastMatchDate', cc1.last_match_date
        ),
            'SecondCombinedCorrelation', json_build_object(
                    'CorrelationId', cc2.correlation_id,
                    'LogoutCharacterId', cc2.logout_character_id,
                    'LoginCharacterId', cc2.login_character_id,
                    'NumberOfMatches', cc2.number_of_matches,
                    'CreateDate', cc2.create_date,
                    'LastMatchDate', cc2.last_match_date
                )
        ) AS combined_json
FROM (SELECT * FROM cte) AS cc1
         INNER JOIN (SELECT * FROM cte) AS cc2 ON cc1.logout_character_id = cc2.logout_character_id
    AND cc1.login_character_id = cc2.login_character_id
    AND cc1.correlation_id <> cc2.correlation_id;";

        /// <summary>
        /// Required parameters: 
        ///    @p0
        /// </summary>
        public const string GetSameCharacterCorrelationsCrossed = @"WITH cte AS (SELECT * FROM character_correlations cc WHERE cc.logout_character_id = @p0 OR cc.login_character_id = @p0)
SELECT DISTINCT ON (cc1.correlation_id)
    json_build_object(
            'FirstCombinedCorrelation', json_build_object(
            'CorrelationId', cc1.correlation_id,
            'LogoutCharacterId', cc1.logout_character_id,
            'LoginCharacterId', cc1.login_character_id,
            'NumberOfMatches', cc1.number_of_matches,
            'CreateDate', cc1.create_date,
            'LastMatchDate', cc1.last_match_date
        ),
            'SecondCombinedCorrelation', json_build_object(
                    'CorrelationId', cc2.correlation_id,
                    'LogoutCharacterId', cc2.logout_character_id,
                    'LoginCharacterId', cc2.login_character_id,
                    'NumberOfMatches', cc2.number_of_matches,
                    'CreateDate', cc2.create_date,
                    'LastMatchDate', cc2.last_match_date
                )
        ) AS combined_json
FROM (SELECT * FROM cte) AS cc1
         INNER JOIN (SELECT * FROM cte) AS cc2 ON cc1.logout_character_id = cc2.login_character_id
    AND cc1.login_character_id = cc2.logout_character_id
    AND cc1.correlation_id <> cc2.correlation_id
WHERE cc1.logout_character_id < cc1.login_character_id;";

        public const string RemoveImpossibleCorrelations = @"WITH characters_in_scan2 AS (
    SELECT character_id
    FROM characters
    WHERE found_in_scan2 = TRUE
),
     characters_in_scan2_not_in_scan1 AS (
         SELECT character_id
         FROM characters
         WHERE found_in_scan2 = TRUE AND found_in_scan1 = FALSE
     )
DELETE FROM character_correlations cc
    USING characters_in_scan2 cs2, characters_in_scan2_not_in_scan1 cs1
WHERE (cc.login_character_id = cs2.character_id AND cc.logout_character_id = cs1.character_id)
   OR (cc.logout_character_id = cs2.character_id AND cc.login_character_id = cs1.character_id);";

    }

}

