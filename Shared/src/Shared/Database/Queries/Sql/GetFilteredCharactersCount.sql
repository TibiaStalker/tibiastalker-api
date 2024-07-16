SELECT COUNT(*) AS TotalCount
FROM characters c
WHERE c.name LIKE '%' || @SearchText || '%';