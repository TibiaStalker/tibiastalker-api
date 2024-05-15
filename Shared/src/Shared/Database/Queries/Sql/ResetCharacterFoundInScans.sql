UPDATE characters
SET found_in_scan1 = FALSE, found_in_scan2 = FALSE
WHERE found_in_scan1 = TRUE OR found_in_scan2 = TRUE;
