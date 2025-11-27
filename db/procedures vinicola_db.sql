USE vinicola_db;
GO

-- 1. ATUALIZAÇÃO: sp_users_select
-- Adicionamos password_hash no retorno (pois o C# exige isso no MontaUsuario)
-- Adicionamos ORDER BY nome quando for listagem geral
CREATE OR ALTER PROCEDURE sp_users_select
    @id INT = NULL
AS
BEGIN
    IF @id IS NULL
    BEGIN
        SELECT id, nome, email, password_hash, profile_pic 
        FROM users 
        ORDER BY nome;
    END
    ELSE
    BEGIN
        SELECT id, nome, email, password_hash, profile_pic
        FROM users
        WHERE id = @id;
    END
END;
GO

-- 2. NOVO: sp_users_delete
CREATE OR ALTER PROCEDURE sp_users_delete
    @id INT
AS
BEGIN
    DELETE FROM users WHERE id = @id;
END;
GO

-- 3. NOVO: sp_users_login
CREATE OR ALTER PROCEDURE sp_users_login
    @email VARCHAR(255),
    @password_hash VARCHAR(255)
AS
BEGIN
    SELECT Id, Nome, Email, password_hash, profile_pic 
    FROM users 
    WHERE Email = @email AND password_hash = @password_hash;
END;
GO

-- 4. NOVO: sp_winery_delete
CREATE OR ALTER PROCEDURE sp_winery_delete
    @id INT
AS
BEGIN
    DELETE FROM winery WHERE id = @id;
END;
GO

-- 5. NOVO: sp_winery_select_by_user
-- Necessário para listar as vinícolas de um usuário específico
CREATE OR ALTER PROCEDURE sp_winery_select_by_user
    @user_id INT
AS
BEGIN
    SELECT * FROM winery 
    WHERE user_id = @user_id 
    ORDER BY name;
END;
GO

-- 6. NOVO: sp_dataLogger_delete
CREATE OR ALTER PROCEDURE sp_dataLogger_delete
    @id INT
AS
BEGIN
    DELETE FROM dataLogger WHERE id = @id;
END;
GO

-- 7. NOVO: sp_dataLogger_select_by_user
-- Necessário para listar os dataloggers de um usuário
CREATE OR ALTER PROCEDURE sp_dataLogger_select_by_user
    @user_id INT
AS
BEGIN
    SELECT * FROM dataLogger 
    WHERE user_id = @user_id 
    ORDER BY id;
END;
GO