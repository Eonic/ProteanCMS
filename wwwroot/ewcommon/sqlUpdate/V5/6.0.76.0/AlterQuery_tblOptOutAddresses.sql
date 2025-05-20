-- 1. Drop the existing primary key constraint on EmailAddress
ALTER TABLE [dbo].[tblOptOutAddresses]
DROP CONSTRAINT [PK_tblOptOutAddresses];

-- 2. Add new column as identity primary key
ALTER TABLE [dbo].[tblOptOutAddresses]
ADD [nOptOutId] INT IDENTITY(1,1) NOT NULL;

-- 3. Add the new columns
ALTER TABLE [dbo].[tblOptOutAddresses]
ADD 
    [userid] VARCHAR(200) NULL,
    [optout_reason] VARCHAR(500) NULL,
    [status] BIT NULL,
    [optout_date] DATETIME NULL;

-- 4. Add new primary key constraint on nOptOutId
ALTER TABLE [dbo].[tblOptOutAddresses]
ADD CONSTRAINT [PK_tblOptOutAddresses] PRIMARY KEY CLUSTERED ([nOptOutId] ASC);