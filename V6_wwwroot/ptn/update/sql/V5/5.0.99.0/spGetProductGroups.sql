CREATE PROCEDURE sp_GetProductGroups

	@ProductId as int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @Results AS VARCHAR(2000)
	SET @Results = ''

	SELECT @Results = cast(nCatId as nvarchar(255)) +','+ @Results FROM
	tblCartCatProductRelations where nContentId = @ProductId

	SELECT @Results

END

