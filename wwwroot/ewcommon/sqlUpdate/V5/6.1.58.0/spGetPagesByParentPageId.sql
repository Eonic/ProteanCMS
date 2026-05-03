CREATE PROCEDURE [dbo].[spGetPagesByParentPageId]    
    @PageId INT = NULL,              
    @whereSql VARCHAR(MAX) = '',            
    @FilterTarget NVARCHAR(10) = ''            
AS                        
BEGIN                  
    SET NOCOUNT ON;
    
    DECLARE @effectivePageId INT = ISNULL(@PageId, 1)
    
    -- Materialize the page hierarchy first (much faster than dynamic SQL recursion)
    ;WITH PageHierarchy AS (
        SELECT 
            nStructKey, 
            nStructKey AS RootKey,
            nStructParId, 
            0 AS Level
        FROM tblContentStructure cs WITH(NOLOCK)
        INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = cs.nAuditId AND ca.nStatus = 1
        WHERE cs.nStructParId = @effectivePageId
        
        UNION ALL
        
        SELECT 
            cs.nStructKey,
            ph.RootKey,  -- Carry forward the root
            cs.nStructParId,
            ph.Level + 1
        FROM tblContentStructure cs WITH(NOLOCK)
        INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = cs.nAuditId AND ca.nStatus = 1
        INNER JOIN PageHierarchy ph ON cs.nStructParId = ph.nStructKey
        WHERE ph.Level < 20  -- Prevent runaway recursion
    )
    SELECT 
        RootKey,
        nStructKey
    INTO #PageHierarchy
    FROM PageHierarchy
    OPTION (MAXRECURSION 100);
    
    -- Pre-calculate content counts per page (avoids correlated subquery)
    SELECT 
        cl.nStructId,
        COUNT(DISTINCT c.nContentKey) AS ContentCount
    INTO #ContentCounts
    FROM tblContentLocation cl WITH(NOLOCK)
    INNER JOIN tblContent c WITH(NOLOCK) ON c.nContentKey = cl.nContentId 
        AND c.cContentSchemaName = @FilterTarget
    INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = c.nAuditId AND ca.nStatus = 1
    INNER JOIN tblAudit cla WITH(NOLOCK) ON cla.nAuditKey = cl.nAuditId AND cla.nStatus = 1
    WHERE cl.nStructId IN (SELECT nStructKey FROM #PageHierarchy)
    GROUP BY cl.nStructId;
    
    -- Aggregate content counts per root page
    SELECT 
        ph.RootKey,
        SUM(cc.ContentCount) AS TotalContentCount
    INTO #RootContentCounts
    FROM #PageHierarchy ph
    INNER JOIN #ContentCounts cc ON cc.nStructId = ph.nStructKey
    GROUP BY ph.RootKey
    HAVING SUM(cc.ContentCount) > 0;
    
    -- Build dynamic SQL for final select
    DECLARE @sqlQuery NVARCHAR(MAX);
    DECLARE @whereSqlClause NVARCHAR(MAX) = '';
    
    IF (@whereSql <> '')
    BEGIN
        SET @whereSqlClause = ' AND ' + @whereSql;
    END
    
    SET @sqlQuery = '
    SELECT 
        cs.nStructKey,
        CASE 
            WHEN ISNULL(CONVERT(XML, cs.cStructDescription).value(''(/DisplayName/node())[1]'', ''nvarchar(max)''), '''') = '''' 
            THEN cs.cStructName 
            ELSE CONVERT(XML, cs.cStructDescription).value(''(/DisplayName/node())[1]'', ''nvarchar(max)'') 
        END AS cStructName,
        rcc.TotalContentCount AS ContentCount
    FROM #RootContentCounts rcc
    INNER JOIN tblContentStructure cs WITH(NOLOCK) ON cs.nStructKey = rcc.RootKey
    INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = cs.nAuditId AND ca.nStatus = 1
    WHERE 1=1 ' + @whereSqlClause + '
    ORDER BY cs.nStructOrder';
    
    EXEC (@sqlQuery);
    
    -- Cleanup
    DROP TABLE #PageHierarchy;
    DROP TABLE #ContentCounts;
    DROP TABLE #RootContentCounts;
    
END
