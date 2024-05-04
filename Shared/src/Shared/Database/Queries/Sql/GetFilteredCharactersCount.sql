SELECT COUNT(*) AS TotalCount
FROM characters c
WHERE c.name LIKE '%' || @SearchText || '%'
OFFSET ((@Page - 1) * @PageSize) ROWS
    LIMIT @PageSize;