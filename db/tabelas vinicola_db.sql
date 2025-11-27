CREATE DATABASE vinicola_db;
GO

USE vinicola_db;
GO

CREATE TABLE users
    (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    
    email VARCHAR(255) NOT NULL UNIQUE, -- único no sistema
    
    password_hash VARCHAR(255) NOT NULL, 
    profile_pic VARCHAR(MAX) 
    );
GO

CREATE TABLE winery
    (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    user_id INT NOT NULL, 
    
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    address VARCHAR(500) NOT NULL,
    
    cnpj VARCHAR(18) NOT NULL UNIQUE, 
    
    email VARCHAR(255) NOT NULL UNIQUE, -- único no sistema
    
    telephone VARCHAR(20) NOT NULL, 
    logo_pic VARCHAR(MAX),

    CONSTRAINT FK_winery_user 
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE NO ACTION -- impede que um usuário seja deletado se ele tiver uma vinícola
    );
GO

CREATE TABLE dataLogger
    (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    
    winery_id INT NOT NULL, 
    user_id INT NOT NULL, 
    device_id VARCHAR(100) NOT NULL UNIQUE,
    temp_min DECIMAL(10,2) NOT NULL,
    temp_max DECIMAL(10,2) NOT NULL,
    lum_min DECIMAL(10,2) NOT NULL,
    lum_max DECIMAL(10,2) NOT NULL,
    humid_min DECIMAL(10,2) NOT NULL,
    humid_max DECIMAL(10,2) NOT NULL,   

    CONSTRAINT FK_dataLogger_winery 
        FOREIGN KEY (winery_id) REFERENCES winery(id)
        ON DELETE CASCADE, -- se a vinícola for deletada, os loggers vão junto
        
    CONSTRAINT FK_dataLogger_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE NO ACTION, -- não deixa deletar o usuário se ele configurou um logger

--    CONSTRAINT CHK_temp_range CHECK (temp_min < temp_max),
--    CONSTRAINT CHK_lum_range CHECK (lum_min < lum_max),
--    CONSTRAINT CHK_humid_range CHECK (humid_min < humid_max)
    );
GO

CREATE TABLE errorLog
    (
    id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    datalogger_id INT NOT NULL, 
    
    log_time DATETIME2 NOT NULL, 
    
    temp DECIMAL(10,2) NOT NULL,
    lum DECIMAL(10,2) NOT NULL,
    humid DECIMAL(10,2) NOT NULL,
    
    error_temp BIT DEFAULT 0 NOT NULL,
    error_humid BIT DEFAULT 0 NOT NULL,
    error_lum BIT DEFAULT 0 NOT NULL,

    CONSTRAINT FK_errorLog_dataLogger 
        FOREIGN KEY (datalogger_id) REFERENCES dataLogger(id)
        ON DELETE CASCADE -- se o logger for deletado, seus logs de erro vão junto
    );
GO