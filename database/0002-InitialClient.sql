/* Sets up an initial client key and secret. If no variables injected 
* via DbUp then this should not do anything */

DECLARE @key varchar(100) = '$InitialClientKey$' 
DECLARE @secret varchar(100) = '$InitialClientSecret'

IF @key <> '' AND @secret <> ''
BEGIN
	INSERT INTO [Clients] (
		[Enabled],
		[ClientId],
		[ProtocolType],
		[RequireClientSecret],
		[ClientName],
		[RequireCOnsent],
		[AllowRememberConsent],
		[AlwaysIncludeUserClaimsInIdToken],
		[RequirePkce],
		[AllowPlainTextPkce],
		[AllowAccessTokensViaBrowser],
		[FrontChannelLogoutSessionRequired],
		[BackChannelLogoutSessionRequired],
		[AllowOfflineAccess],
		[IdentityTokenLifetime],
		[AccessTokenLifetime],
		[AuthorizationCodeLifetime],
		[ConsentLifetime],
		[AbsoluteRefreshTokenLifetime],
		[SlidingRefreshTokenLifetime],
		[RefreshTokenUsage],
		[UpdateAccessTokenClaimsOnRefresh],
		[RefreshTokenExpiration],
		[AccessTokenType],
		[EnableLocalLogin],
		[IncludeJwtId],
		[AlwaysSendClientClaims],
		[Created],
		[DeviceCodeLifetime],
		[NonEditable]
	)
	VALUES (
			[Enabled],
		[ClientId],
		[ProtocolType],
		[RequireClientSecret],
		[ClientName],
		[RequireCOnsent],
		[AllowRememberConsent],
		[AlwaysIncludeUserClaimsInIdToken],
		[RequirePkce],
		[AllowPlainTextPkce],
		[AllowAccessTokensViaBrowser],
		[FrontChannelLogoutSessionRequired],
		[BackChannelLogoutSessionRequired],
		[AllowOfflineAccess],
		[IdentityTokenLifetime],
		[AccessTokenLifetime],
		[AuthorizationCodeLifetime],
		[ConsentLifetime],
		[AbsoluteRefreshTokenLifetime],
		[SlidingRefreshTokenLifetime],
		[RefreshTokenUsage],
		[UpdateAccessTokenClaimsOnRefresh],
		[RefreshTokenExpiration],
		[AccessTokenType],
		[EnableLocalLogin],
		[IncludeJwtId],
		[AlwaysSendClientClaims],
		[Created],
		[DeviceCodeLifetime],
		[NonEditable]
	)
END
GO