CREATE TABLE Product (
	ProductId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[Name] NVARCHAR(128) NOT NULL,
	Price decimal(9,2) NOT NULL,
	LastUpdated DATETIME2 NOT NULL DEFAULT(GETDATE())
);

CREATE TABLE ProductPriceHistory (
	ProductPriceHistoryId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	ProductId INT NOT NULL,
	[Timestamp] DATETIME2 NOT NULL,
	OldPrice decimal(9,2) NULL,
	NewPrice decimal(9,2) NOT NULL,
	DiscountPercentage decimal(9,2) NULL
);

CREATE TABLE ProductDiscount (
	ProductDiscountId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	ProductId INT NOT NULL,
	DiscountPercentage decimal NOT NULL
);