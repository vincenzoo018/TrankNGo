-- Check if users exist in the database
USE TrackNGoDB;
GO

SELECT 
    Id,
    Username,
    Email,
    FullName,
    Role,
    Department,
    IsActive,
    CreatedAt,
    LEFT(PasswordHash, 20) + '...' AS PasswordHashPreview
FROM Users
ORDER BY Id;

-- Check total user count
SELECT COUNT(*) AS TotalUsers FROM Users;

-- Check specific user by username
SELECT * FROM Users WHERE Username = 'mayor';

-- Check specific user by email
SELECT * FROM Users WHERE Email = 'mayor@trackngo.mati.gov.ph';
