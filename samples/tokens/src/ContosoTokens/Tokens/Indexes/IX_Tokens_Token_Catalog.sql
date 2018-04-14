CREATE INDEX IX_Tokens_Token_Catalog
ON [Tokens].[Token] ( CatalogId, State )
INCLUDE( TokenId, Version, Code, Hash );