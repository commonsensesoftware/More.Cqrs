CREATE TABLE [Tokens].[OrderLineItem]
(
 OrderId UNIQUEIDENTIFIER NOT NULL
,TokenId VARCHAR(50) NOT NULL
,CONSTRAINT PK_Tokens_OrderLineItem PRIMARY KEY( OrderId, TokenId )
,CONSTRAINT FK_Tokens_OrderLineItem_Order FOREIGN KEY ( OrderId )
	REFERENCES [Tokens].[Order]( OrderId ) ON DELETE CASCADE
,CONSTRAINT FK_Tokens_OrderLineItem_Token FOREIGN KEY ( TokenId )
	REFERENCES [Tokens].[Token]( TokenId )
);