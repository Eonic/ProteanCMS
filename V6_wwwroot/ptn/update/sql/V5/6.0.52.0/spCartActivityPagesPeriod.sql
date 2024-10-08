
ALTER PROCEDURE [dbo].[spCartActivityPagesPeriod] 
	@Group nvarchar(50)='Week',
	@nYear int,
	@nMonth int,
	@nWeek int,
	@cCurrencySymbol nvarchar(50) = '',
	@nOrderStatus nvarchar(50) = '6,9,17',
	@cOrderType nvarchar(50) = 'ORDER'
AS
BEGIN
	
	SELECT 
		tblContentStructure.cStructName, 
		SUM(vw_CartOverViewPages.nQuantity) AS Quantity, 
		SUM(vw_CartOverViewPages.nLinePrice) AS Price, 
		CASE 
			WHEN @Group = 'Month' THEN vw_CartOverViewPages.Month 
			WHEN @Group = 'Week' THEN vw_CartOverViewPages.Week 	
			WHEN @Group = 'Day' THEN vw_CartOverViewPages.Day 
		END 
		AS GroupBy,
		@Group as GroupType
			
	FROM
		vw_CartOverViewPages LEFT OUTER JOIN tblContentStructure ON vw_CartOverViewPages.nStructId = tblContentStructure.nStructKey
	WHERE 
		vw_CartOverViewPages.Year = @nYear AND  
		vw_CartOverViewPages.Month = CASE WHEN @nMonth= 0 THEN vw_CartOverViewPages.Month ELSE @nMonth END AND 
		vw_CartOverViewPages.Week = CASE WHEN @nWeek= 0 THEN vw_CartOverViewPages.Week ELSE @nWeek END 
		AND 		
				nCartStatus IN((SELECT convert(int, value) FROM string_split(@nOrderStatus, ',')))
			
		AND 
			cCartSchemaName = @cOrderType
		AND 
			(
				cCurrency = CASE WHEN @cCurrencySymbol = '' THEN '' ELSE  @cCurrencySymbol END
					OR 
				cCurrency = CASE WHEN @cCurrencySymbol = '' THEN NULL ELSE  @cCurrencySymbol END
			)

	GROUP BY 
		tblContentStructure.cStructName, 
		vw_CartOverViewPages.Year, 
		CASE 
			WHEN @Group = 'Month' THEN vw_CartOverViewPages.Month 
			WHEN @Group = 'Week' THEN vw_CartOverViewPages.Week 	
			WHEN @Group = 'Day' THEN vw_CartOverViewPages.Day 
		END
	ORDER BY
		CASE 
			WHEN @Group = 'Month' THEN vw_CartOverViewPages.Month 
			WHEN @Group = 'Week' THEN vw_CartOverViewPages.Week 	
			WHEN @Group = 'Day' THEN vw_CartOverViewPages.Day 
		END
		,tblContentStructure.cStructName
		
END
