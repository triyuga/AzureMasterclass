
CREATE TABLE [Book]
(
    [Id]            int IDENTITY (1,1)  NOT NULL,
    [Name]          VARCHAR(200)   NOT NULL,
    [Description]   NVARCHAR(1000) NULL,
    [NumberOfPages] int            NOT NULL,
    CONSTRAINT [PK_Book] PRIMARY KEY CLUSTERED (Id ASC)
)

CREATE TABLE [Author]
(
    [Id]            int IDENTITY (1,1)  NOT NULL,
    [Name]          NVARCHAR(100)  NOT NULL,
    [Website]       NVARCHAR(100)  NULL,
    CONSTRAINT [PK_Author] PRIMARY KEY CLUSTERED (Id ASC)
)

GO