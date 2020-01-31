﻿CREATE TABLE [dbo].[UserGender] (
    [UserGenderId]   SMALLINT       IDENTITY (10, 1) NOT NULL,
    [UserGenderName] NVARCHAR (500) NOT NULL,
    CONSTRAINT [PK_dbo.UserGender] PRIMARY KEY CLUSTERED ([UserGenderId] ASC)
);
