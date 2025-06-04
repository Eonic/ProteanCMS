EXEC sp_rename 'tblOptOutAddresses.nOptOutId', 'nOptOutKey', 'COLUMN';
EXEC sp_rename 'tblOptOutAddresses.userid', 'nCartContactId', 'COLUMN';
EXEC sp_rename 'tblOptOutAddresses.optout_date', 'dOptOut', 'COLUMN';
EXEC sp_rename 'tblOptOutAddresses.status', 'nStatus', 'COLUMN'