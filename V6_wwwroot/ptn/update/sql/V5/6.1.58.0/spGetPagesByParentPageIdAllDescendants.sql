CREATE PROCEDURE [dbo].[spGetPagesByParentPageIdAllDescendants]    
    @PageId INT = NULL,              
    @whereSql VARCHAR(MAX) = '',            
    @FilterTarget NVARCHAR(10) = ''            
AS                        
BEGIN                  
    SET NOCOUNT ON;
    
    DECLARE @effectivePageId INT = ISNULL(@PageId, 1);
    
    -- Step 1: Get ALL descendant pages with their hierarchy
    ;WITH PageHierarchy AS (
        -- Anchor: Direct children of the parent page
        SELECT 
            cs.nStructKey,
            cs.nStructParId,
            cs.cStructName,
            CAST(cs.cStructDescription AS NVARCHAR(MAX)) AS cStructDescription,
            cs.nStructOrder,
            0 AS Level,
            CAST(RIGHT('00000' + CAST(cs.nStructOrder AS VARCHAR), 5) AS VARCHAR(MAX)) AS SortPath
        FROM tblContentStructure cs WITH(NOLOCK)
        INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = cs.nAuditId AND ca.nStatus = 1
        WHERE cs.nStructParId = @effectivePageId
        
        UNION ALL
        
        -- Recursive: All descendants
        SELECT 
            cs.nStructKey,
            cs.nStructParId,
            cs.cStructName,
            CAST(cs.cStructDescription AS NVARCHAR(MAX)) AS cStructDescription,
            cs.nStructOrder,
            ph.Level + 1,
            CAST(ph.SortPath + '/' + RIGHT('00000' + CAST(cs.nStructOrder AS VARCHAR), 5) AS VARCHAR(MAX))
        FROM tblContentStructure cs WITH(NOLOCK)
        INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = cs.nAuditId AND ca.nStatus = 1
        INNER JOIN PageHierarchy ph ON cs.nStructParId = ph.nStructKey
        WHERE ph.Level < 20
    )
    SELECT *
    INTO #AllPages
    FROM PageHierarchy
    OPTION (MAXRECURSION 100);
    
    -- Step 2: Create parent-child mapping (page includes itself + all descendants)
    SELECT DISTINCT
        parent.nStructKey AS ParentKey,
        child.nStructKey AS ChildKey
    INTO #ParentChildMap
    FROM #AllPages parent
    INNER JOIN #AllPages child ON child.SortPath LIKE parent.SortPath + '%'
    
    UNION
    
    SELECT 
        nStructKey AS ParentKey,
        nStructKey AS ChildKey
    FROM #AllPages;
    
    -- Step 3: Get direct content count for each page
    SELECT 
        cl.nStructId,
        COUNT(DISTINCT c.nContentKey) AS DirectContentCount
    INTO #DirectContentCounts
    FROM tblContentLocation cl WITH(NOLOCK)
    INNER JOIN tblContent c WITH(NOLOCK) ON c.nContentKey = cl.nContentId 
        AND c.cContentSchemaName = @FilterTarget
    INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = c.nAuditId AND ca.nStatus = 1
    INNER JOIN tblAudit cla WITH(NOLOCK) ON cla.nAuditKey = cl.nAuditId AND cla.nStatus = 1
    WHERE cl.nStructId IN (SELECT nStructKey FROM #AllPages)
    GROUP BY cl.nStructId;
    
    -- Step 4: Calculate total content count (page + all descendants)
    SELECT 
        pcm.ParentKey AS nStructKey,
        SUM(ISNULL(dcc.DirectContentCount, 0)) AS TotalContentCount
    INTO #TotalContentCounts
    FROM #ParentChildMap pcm
    LEFT JOIN #DirectContentCounts dcc ON dcc.nStructId = pcm.ChildKey
    GROUP BY pcm.ParentKey;
    
    -- Step 5: Build dynamic SQL for final select with whereSql support
    DECLARE @sqlQuery NVARCHAR(MAX);
    DECLARE @whereSqlClause NVARCHAR(MAX) = '';
    
    IF (@whereSql <> '')
    BEGIN
        SET @whereSqlClause = ' AND ' + @whereSql;
    END
    
    SET @sqlQuery = '
    SELECT 
        ap.nStructKey,
        ap.nStructParId,
        CASE 
            WHEN ISNULL(CONVERT(XML, ap.cStructDescription).value(''(/DisplayName/node())[1]'', ''nvarchar(max)''), '''') = '''' 
            THEN ap.cStructName 
            ELSE CONVERT(XML, ap.cStructDescription).value(''(/DisplayName/node())[1]'', ''nvarchar(max)'') 
        END AS cStructName,
        ap.Level,
        ISNULL(tcc.TotalContentCount, 0) AS ContentCount
    FROM #AllPages ap
    LEFT JOIN #TotalContentCounts tcc ON tcc.nStructKey = ap.nStructKey
    WHERE ISNULL(tcc.TotalContentCount, 0) > 0 ' + @whereSqlClause + '
    ORDER BY ap.SortPath';
    
    EXEC (@sqlQuery);
    
    -- Cleanup
    DROP TABLE #AllPages;
    DROP TABLE #ParentChildMap;
    DROP TABLE #DirectContentCounts;
    DROP TABLE #TotalContentCounts;
    
END
