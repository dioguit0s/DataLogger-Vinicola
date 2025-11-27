USE vinicola_db;
GO

CREATE OR ALTER PROCEDURE sp_users_insert
    @nome VARCHAR(100),
    @email VARCHAR(255),
    @password_hash VARCHAR(255),
    @profile_pic VARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO users (nome, email, password_hash, profile_pic)
    VALUES (@nome, @email, @password_hash, @profile_pic);
    
--    SELECT SCOPE_IDENTITY() AS new_id;
END;
GO

CREATE OR ALTER PROCEDURE sp_users_update
    @id INT,
    @nome VARCHAR(100),
    @email VARCHAR(255),
    @password_hash VARCHAR(255),
    @profile_pic VARCHAR(MAX) = NULL
AS
BEGIN
    UPDATE users
    SET 
        nome = @nome,
        email = @email,
        password_hash = @password_hash,
        profile_pic = @profile_pic
    WHERE id = @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_users_select
    @id INT = NULL
AS
BEGIN
    IF @id IS NULL
    BEGIN
        SELECT id, nome, email, profile_pic -- Nota: O hash da senha geralmente não é retornado em um select geral
        FROM users;
    END
    ELSE
    BEGIN
        SELECT id, nome, email, profile_pic
        FROM users
        WHERE id = @id;
    END
END;
GO


CREATE OR ALTER PROCEDURE sp_winery_insert
    @user_id INT,
    @name VARCHAR(100),
    @address VARCHAR(500),
    @cnpj VARCHAR(18),
    @email VARCHAR(255),
    @telephone VARCHAR(20),
    @description VARCHAR(500) = NULL,
    @logo_pic VARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO winery (user_id, name, description, address, cnpj, email, telephone, logo_pic)
    VALUES (@user_id, @name, @description, @address, @cnpj, @email, @telephone, @logo_pic);

--    SELECT SCOPE_IDENTITY() AS new_id;
END;
GO

CREATE OR ALTER PROCEDURE sp_winery_update
    @id INT,
    @user_id INT,
    @name VARCHAR(100),
    @address VARCHAR(500),
    @cnpj VARCHAR(18),
    @email VARCHAR(255),
    @telephone VARCHAR(20),
    @description VARCHAR(500) = NULL,
    @logo_pic VARCHAR(MAX) = NULL
AS
BEGIN
    UPDATE winery
    SET
        user_id = @user_id,
        name = @name,
        description = @description,
        address = @address,
        cnpj = @cnpj,
        email = @email,
        telephone = @telephone,
        logo_pic = @logo_pic
    WHERE id = @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_winery_select
    @id INT = NULL
AS
BEGIN
    IF @id IS NULL
    BEGIN
        SELECT * FROM winery;
    END
    ELSE
    BEGIN
        SELECT * FROM winery WHERE id = @id;
    END
END;
GO


CREATE OR ALTER PROCEDURE sp_dataLogger_insert
	@id int,
    @winery_id INT,
    @user_id INT,
    @device_id VARCHAR(100),
    @temp_min DECIMAL(10,2),
    @temp_max DECIMAL(10,2),
    @lum_min DECIMAL(10,2),
    @lum_max DECIMAL(10,2),
    @humid_min DECIMAL(10,2),
    @humid_max DECIMAL(10,2)
AS
BEGIN
    INSERT INTO dataLogger (
        winery_id, 
        user_id, 
        device_id, 
        temp_min, 
        temp_max, 
        lum_min, 
        lum_max, 
        humid_min, 
        humid_max
    )
    VALUES (
        @winery_id, 
        @user_id, 
        @device_id, 
        @temp_min, 
        @temp_max, 
        @lum_min, 
        @lum_max, 
        @humid_min, 
        @humid_max
    );
END;
GO

CREATE OR ALTER PROCEDURE sp_dataLogger_update
    @id INT,
    @winery_id INT,
    @user_id INT,
    @device_id VARCHAR(100), -- Novo parâmetro
    @temp_min DECIMAL(10,2),
    @temp_max DECIMAL(10,2),
    @lum_min DECIMAL(10,2),
    @lum_max DECIMAL(10,2),
    @humid_min DECIMAL(10,2),
    @humid_max DECIMAL(10,2)
AS
BEGIN
    UPDATE dataLogger
    SET 
        winery_id = @winery_id,
        user_id = @user_id,
        device_id = @device_id, 
        temp_min = @temp_min,
        temp_max = @temp_max,
        lum_min = @lum_min,
        lum_max = @lum_max,
        humid_min = @humid_min,
        humid_max = @humid_max
    WHERE id = @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_dataLogger_select
    @id INT = NULL
AS
BEGIN
    IF @id IS NULL
    BEGIN
        SELECT * FROM dataLogger;
    END
    ELSE
    BEGIN
        SELECT * FROM dataLogger WHERE id = @id;
    END
END;
GO


CREATE OR ALTER PROCEDURE sp_errorLog_insert
    @datalogger_id INT,
    @log_time DATETIME2,
    @temp DECIMAL(10,2),
    @lum DECIMAL(10,2),
    @humid DECIMAL(10,2),
    @error_temp BIT = 0,
    @error_humid BIT = 0,
    @error_lum BIT = 0
AS
BEGIN
    INSERT INTO errorLog (datalogger_id, log_time, temp, lum, humid, error_temp, error_humid, error_lum)
    VALUES (@datalogger_id, @log_time, @temp, @lum, @humid, @error_temp, @error_humid, @error_lum);
 
--    SELECT SCOPE_IDENTITY() AS new_id;
END;
GO

CREATE OR ALTER PROCEDURE sp_errorLog_update
    @id BIGINT,
    @datalogger_id INT,
    @log_time DATETIME2,
    @temp DECIMAL(10,2),
    @lum DECIMAL(10,2),
    @humid DECIMAL(10,2),
    @error_temp BIT,
    @error_humid BIT,
    @error_lum BIT
AS
BEGIN
    UPDATE errorLog
    SET
        datalogger_id = @datalogger_id,
        log_time = @log_time,
        temp = @temp,
        lum = @lum,
        humid = @humid,
        error_temp = @error_temp,
        error_humid = @error_humid,
        error_lum = @error_lum
    WHERE id = @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_errorLog_select
    @id BIGINT = NULL
AS
BEGIN
    IF @id IS NULL
    BEGIN
        SELECT * FROM errorLog ORDER BY log_time DESC; -- Logs geralmente são vistos do mais recente
    END
    ELSE
    BEGIN
        SELECT * FROM errorLog WHERE id = @id;
    END
END;
GO