
UPDATE t
SET 
    cDirSalt = s.SaltString,
    cDirPassword = b.Base64Hash
FROM dbo.tblDirectory t

CROSS APPLY (
    SELECT CAST(NEWID() AS NVARCHAR(36)) AS SaltString
) s

CROSS APPLY (
    SELECT HASHBYTES(
        'SHA2_512',
        (UPPER(s.SaltString) + LOWER(LTRIM(RTRIM(t.cDirPassword))))
            COLLATE Latin1_General_100_CI_AS_SC_UTF8
    ) AS HashBytes
) h

CROSS APPLY (
    SELECT CAST('' AS XML).value(
        'xs:base64Binary(sql:column("h.HashBytes"))',
        'NVARCHAR(128)'
    ) AS Base64Hash
) b

WHERE t.cDirPassword IS NOT NULL;
