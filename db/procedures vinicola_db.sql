USE vinicola_db;
GO

-- 1. CORRE플O: sp_users_insert
-- Alterado @profile_pic de VARCHAR(MAX) para VARBINARY(MAX)
CREATE OR ALTER PROCEDURE sp_users_insert
    @nome VARCHAR(100),
    @email VARCHAR(255),
    @password_hash VARCHAR(255),
    @profile_pic VARBINARY(MAX) = NULL 
AS
BEGIN
    INSERT INTO users (nome, email, password_hash, profile_pic)
    VALUES (@nome, @email, @password_hash, @profile_pic);
END;
GO

-- 2. CORRE플O: sp_users_update
-- Alterado @profile_pic de VARCHAR(MAX) para VARBINARY(MAX)
CREATE OR ALTER PROCEDURE sp_users_update
    @id INT,
    @nome VARCHAR(100),
    @email VARCHAR(255),
    @password_hash VARCHAR(255),
    @profile_pic VARBINARY(MAX) = NULL
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

-- 3. CORRE플O PREVENTIVA: sp_winery_insert
-- Alterado @logo_pic de VARCHAR(MAX) para VARBINARY(MAX)
CREATE OR ALTER PROCEDURE sp_winery_insert
    @user_id INT,
    @name VARCHAR(100),
    @address VARCHAR(500),
    @cnpj VARCHAR(18),
    @email VARCHAR(255),
    @telephone VARCHAR(20),
    @description VARCHAR(500) = NULL,
    @logo_pic VARBINARY(MAX) = NULL
AS
BEGIN
    INSERT INTO winery (user_id, name, description, address, cnpj, email, telephone, logo_pic)
    VALUES (@user_id, @name, @description, @address, @cnpj, @email, @telephone, @logo_pic);
END;
GO

-- 4. CORRE플O PREVENTIVA: sp_winery_update
-- Alterado @logo_pic de VARCHAR(MAX) para VARBINARY(MAX)
CREATE OR ALTER PROCEDURE sp_winery_update
    @id INT,
    @user_id INT,
    @name VARCHAR(100),
    @address VARCHAR(500),
    @cnpj VARCHAR(18),
    @email VARCHAR(255),
    @telephone VARCHAR(20),
    @description VARCHAR(500) = NULL,
    @logo_pic VARBINARY(MAX) = NULL
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