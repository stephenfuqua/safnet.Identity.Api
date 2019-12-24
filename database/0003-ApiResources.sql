INSERT INTO [dbo].[ApiResources] (
    [Enabled]
    ,[Name]
    ,[DisplayName]
	,[NonEditable]
	,[Created]
)
VALUES (
    1
    ,'admin'
    ,'Admin API'
	,0
	,GETDATE()
)

INSERT INTO [dbo].[ApiScopes] (
    [Name]
    ,[DisplayName]
    ,[Required]
    ,[Emphasize]
    ,[ShowInDiscoveryDocument]
    ,[ApiResourceId]
)
SELECT
    [Name],
    [DisplayName],
    0,
    0,
    1,
    [Id]
FROM [dbo].[ApiResources]
WHERE [Id] = @@identity