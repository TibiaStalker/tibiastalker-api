WITH characters_in_scan2 AS (
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
   OR (cc.logout_character_id = cs2.character_id AND cc.login_character_id = cs1.character_id);