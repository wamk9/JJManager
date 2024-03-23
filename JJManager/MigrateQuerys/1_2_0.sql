-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

CREATE TABLE dbo.jj_products (
    id INT IDENTITY (1, 1) NOT NULL,
    product_name VARCHAR (50) NOT NULL,
    analog_inputs_qtd TINYINT NOT NULL DEFAULT 0, 
    digital_inputs_qtd TINYINT NOT NULL DEFAULT 0, 
    CONSTRAINT PK_JJ_PRODUCT PRIMARY KEY CLUSTERED (id ASC)
);


CREATE TABLE dbo.digital_inputs
(
    id              INT      NOT NULL,
    name            VARCHAR (15) NULL,
    type            VARCHAR (10) NOT NULL,
    cmd			    TEXT         NULL,
    id_profile      INT          NOT NULL,
    CONSTRAINT PK_DIGITAL_INPUT PRIMARY KEY CLUSTERED (id ASC, id_profile ASC),
    CONSTRAINT FK_PROFILE_DIGITAL_INPUT FOREIGN KEY (id_profile) REFERENCES dbo.profiles (id) ON DELETE CASCADE ON UPDATE CASCADE
)

GO

INSERT INTO dbo.jj_products (product_name, analog_inputs_qtd, digital_inputs_qtd) 
	VALUES 
	('Mixer de Áudio JJM-01', 5, 0),
	('ButtonBox JJB-01', 2, 0),
	('Streamdeck JJSD-01', 0, 12),
	('Streamdeck JJSD-02', 0, 100),
	('ButtonBox JJB-01 V2', 0, 0),
    ('Painel de Leds JJQ-01', 0, 0);

GO

CREATE TABLE dbo.user_products (
	id INT IDENTITY (1, 1) NOT NULL,
    id_product INT NOT NULL,
    auto_connect BIT NOT NULL DEFAULT 0,
    conn_id NCHAR(10) NOT NULL,
    id_profile INT NOT NULL, 
	CONSTRAINT FK_JJ_PRODUCT_USER_PRODUCT FOREIGN KEY (id_product) REFERENCES dbo.jj_products (id) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT FK_PROFILE_USER_PRODUCT FOREIGN KEY (id_profile) REFERENCES dbo.profiles (id),
    CONSTRAINT PK_USER_PRODUCT PRIMARY KEY CLUSTERED (id ASC)
);
GO

ALTER TABLE dbo.[profiles] DROP CONSTRAINT FK_PRODUCT_PROFILE;
GO

UPDATE dbo.profiles SET dbo.profiles.id_product = (SELECT id FROM dbo.jj_products WHERE product_name = 'Mixer de Áudio JJM-01') WHERE dbo.profiles.id_product IN (SELECT dbo.products.id FROM dbo.products WHERE dbo.products.name = 'Mixer de Áudio JJM-01');
GO

UPDATE dbo.profiles SET dbo.profiles.id_product = (SELECT id FROM dbo.jj_products WHERE product_name = 'ButtonBox JJB-01') WHERE dbo.profiles.id_product IN (SELECT dbo.products.id FROM dbo.products WHERE dbo.products.name = 'ButtonBox JJB-01');
GO

ALTER TABLE dbo.profiles ADD CONSTRAINT FK_JJ_PRODUCT_PROFILE FOREIGN KEY (id_product) REFERENCES dbo.jj_products (id) ON DELETE CASCADE ON UPDATE CASCADE;
GO

DROP TABLE dbo.products;
GO

UPDATE dbo.configs SET software_version = '1.2.0';
GO

SET ANSI_WARNINGS on
GO