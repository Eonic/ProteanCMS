/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/

ALTER TABLE tblCartItem ALTER COLUMN cItemURL nvarchar(800) NOT NULL;
ALTER TABLE tblCartItem ALTER COLUMN cItemName nvarchar(800) NOT NULL;
