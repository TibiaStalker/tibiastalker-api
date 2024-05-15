DELETE FROM character_correlations cc
    USING "characters" c1, "characters" c2
WHERE cc.login_character_id = c1.character_id
  AND cc.logout_character_id = c2.character_id
  AND c1.found_in_scan2 = TRUE AND c2.found_in_scan2 = TRUE AND c2.found_in_scan1 = FALSE