﻿CREATE TABLE [dbo].[UserLoggingLog] (
    [UserLoggingLogId] BIGINT         IDENTITY (1, 1) NOT NULL,
    [UserId]           BIGINT         NOT NULL,
    [UserName]         NVARCHAR (50)  NOT NULL,
    [LoggingIpAddress] NVARCHAR (500) NOT NULL,
    [LoggingBrowser]   NVARCHAR (MAX) NOT NULL,
    [IsLogginSuccess]  BIT            NOT NULL,
    [CreatedOn]        DATETIME       NOT NULL,
    [CreatedBy]        BIGINT         NOT NULL,
    [ModifiedOn]       DATETIME       NOT NULL,
    [ModifiedBy]       BIGINT         NOT NULL,
    CONSTRAINT [PK_dbo.UserLoggingLog] PRIMARY KEY CLUSTERED ([UserLoggingLogId] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId])
);
