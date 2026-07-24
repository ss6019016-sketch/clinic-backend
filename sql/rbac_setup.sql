IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Modules')
BEGIN
    CREATE TABLE Modules (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RolePermissions')
BEGIN
    CREATE TABLE RolePermissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleName NVARCHAR(100) NOT NULL,
        ModuleId INT NOT NULL,
        CanView BIT NOT NULL DEFAULT 0,
        CanCreate BIT NOT NULL DEFAULT 0,
        CanEdit BIT NOT NULL DEFAULT 0,
        CanDelete BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_RolePermissions_Modules FOREIGN KEY (ModuleId) REFERENCES Modules(Id)
    );
END

DECLARE @moduleNames TABLE (Name NVARCHAR(100));
INSERT INTO @moduleNames (Name) VALUES
    ('Patients'),
    ('Doctors'),
    ('Appointments'),
    ('Prescriptions'),
    ('Billing'),
    ('Staff'),
    ('Reports'),
    ('AuditLog'),
    ('Settings');

MERGE INTO Modules AS target
USING @moduleNames AS source
ON target.Name = source.Name
WHEN NOT MATCHED THEN
    INSERT (Name) VALUES (source.Name);

DECLARE @adminRole NVARCHAR(100) = 'Admin';
DECLARE @receptionistRole NVARCHAR(100) = 'Receptionist';

DECLARE @moduleCount INT = (SELECT COUNT(*) FROM Modules);

DELETE FROM RolePermissions;

INSERT INTO RolePermissions (RoleName, ModuleId, CanView, CanCreate, CanEdit, CanDelete)
SELECT @adminRole, Id, 1, 1, 1, 1 FROM Modules;

INSERT INTO RolePermissions (RoleName, ModuleId, CanView, CanCreate, CanEdit, CanDelete)
SELECT @receptionistRole, Id, 1, 0, 0, 0 FROM Modules;

UPDATE RolePermissions
SET CanCreate = 1,
    CanEdit = 1
WHERE RoleName = @receptionistRole
  AND ModuleId IN (
      SELECT Id FROM Modules WHERE Name IN ('Patients', 'Appointments', 'Billing', 'Prescriptions')
  );

UPDATE RolePermissions
SET CanView = 1,
    CanCreate = 0,
    CanEdit = 0,
    CanDelete = 0
WHERE RoleName = @receptionistRole
  AND ModuleId IN (
      SELECT Id FROM Modules WHERE Name IN ('Doctors', 'Staff', 'AuditLog', 'Settings')
  );

UPDATE RolePermissions
SET CanDelete = 0
WHERE RoleName = @receptionistRole;
