-- Insert Customers
INSERT INTO [Customers] (
    [Id], [FirstName], [LastName], [BVN], [CreditScore], [DateOfBirth], 
    [Email], [PhoneNumber], 
    [Street], [City], [State], [ZipCode], [Country],
    [IsActive], [IsDeleted], [DeletedAt], [DeletedBy], [DateCreated], [DateUpdated]
)
VALUES
-- Customer 1: John Doe
('a1b2c3d4-1234-5678-9abc-000000000001', 'John', 'Doe', '20000000001', 745, '1985-03-20',
 'john.doe@email.com', '555-0101',
 '101 Main Street', 'Lagos', 'Lagos', '100001', 'Nigeria',
 1, 0, NULL, NULL, '2024-01-15 10:30:00', '2024-01-15 10:30:00'),

-- Customer 2: Jane Smith
('a1b2c3d4-1234-5678-9abc-000000000002', 'Jane', 'Smith', '20000000002', 740, '1988-07-12',
 'jane.smith@email.com', '555-0102',
 '102 Main Street', 'Lagos', 'Lagos', '100002', 'Nigeria',
 1, 0, NULL, NULL, '2024-01-15 10:30:00', '2024-01-15 10:30:00'),

-- Customer 3: Bob Williams
('a1b2c3d4-1234-5678-9abc-000000000003', 'Bob', 'Williams', '20000000003', 735, '1982-11-05',
 'bob.williams@email.com', '555-0103',
 '103 Main Street', 'Lagos', 'Lagos', '100003', 'Nigeria',
 1, 0, NULL, NULL, '2024-01-15 10:30:00', '2024-01-15 10:30:00'),

-- Customer 4: Sarah Brown
('a1b2c3d4-1234-5678-9abc-000000000004', 'Sarah', 'Brown', '20000000004', 730, '1990-01-25',
 'sarah.brown@email.com', '555-0104',
 '104 Main Street', 'Lagos', 'Lagos', '100004', 'Nigeria',
 1, 0, NULL, NULL, '2024-01-15 10:30:00', '2024-01-15 10:30:00'),

-- Customer 5: Mike Davis
('a1b2c3d4-1234-5678-9abc-000000000005', 'Mike', 'Davis', '20000000005', 725, '1987-09-15',
 'mike.davis@email.com', '555-0105',
 '105 Main Street', 'Lagos', 'Lagos', '100005', 'Nigeria',
 1, 0, NULL, NULL, '2024-01-15 10:30:00', '2024-01-15 10:30:00'),

-- Customer 6: Emily Wilson
('a1b2c3d4-1234-5678-9abc-000000000006', 'Emily', 'Wilson', '20000000006', 720, '1984-04-30',
 'emily.wilson@email.com', '555-0106',
 '106 Main Street', 'Lagos', 'Lagos', '100006', 'Nigeria',
 1, 0, NULL, NULL, '2024-01-15 10:30:00', '2024-01-15 10:30:00'),

-- Customer 7: David Taylor
('a1b2c3d4-1234-5678-9abc-000000000007', 'David', 'Taylor', '20000000007', 715, '1981-12-10',
 'david.taylor@email.com', '555-0107',
 '107 Main Street', 'Lagos', 'Lagos', '100007', 'Nigeria',
 1, 0, NULL, NULL, '2024-01-15 10:30:00', '2024-01-15 10:30:00'),

-- Customer 8: Lisa Anderson
('a1b2c3d4-1234-5678-9abc-000000000008', 'Lisa', 'Anderson', '20000000008', 710, '1989-06-18',
 'lisa.anderson@email.com', '555-0108',
 '108 Main Street', 'Lagos', 'Lagos', '100008', 'Nigeria',
 1, 0, NULL, NULL, '2024-01-15 10:30:00', '2024-01-15 10:30:00'),

-- Customer 9: Chris Thomas
('a1b2c3d4-1234-5678-9abc-000000000009', 'Chris', 'Thomas', '20000000009', 705, '1983-08-22',
 'chris.thomas@email.com', '555-0109',
 '109 Main Street', 'Lagos', 'Lagos', '100009', 'Nigeria',
 1, 0, NULL, NULL, '2024-01-15 10:30:00', '2024-01-15 10:30:00');

 -- Insert Accounts
INSERT INTO [Accounts] (
    [Id], [AccountNumber], [AccountType], [CustomerId], [DateOpened], 
    [DateClosed], [RowVersion], [AccountStatus], [IsDeleted], [DeletedAt], [DeletedBy], 
    [DateCreated], [DateUpdated], [CurrentBalance], [Currency], 
    [AvailableBalance], [AvailableBalanceCurrency]
)
VALUES
-- Account 1 for John Doe (Checking)
('c3d4e5f6-3456-7890-cde1-000000000001', '1000000002', 'Checking', 'a1b2c3d4-1234-5678-9abc-000000000001',
 '2024-01-25 14:15:00', NULL, NULL, 'Active', 0, NULL, NULL,
 '2024-01-25 14:15:00', '2024-01-25 14:15:00', 1000.00, 'NGN', 1000.00, 'NGN'),

-- Account 2 for Jane Smith (Savings)
('c3d4e5f6-3456-7890-cde1-000000000002', '1000000003', 'Savings', 'a1b2c3d4-1234-5678-9abc-000000000002',
 '2024-01-25 14:15:00', NULL, NULL, 'Active', 0, NULL, NULL,
 '2024-01-25 14:15:00', '2024-01-25 14:15:00', 1500.00, 'NGN', 1500.00, 'NGN'),

-- Account 3 for Bob Williams (Checking)
('c3d4e5f6-3456-7890-cde1-000000000003', '1000000004', 'Checking', 'a1b2c3d4-1234-5678-9abc-000000000003',
 '2024-01-25 14:15:00', NULL, NULL, 'Active', 0, NULL, NULL,
 '2024-01-25 14:15:00', '2024-01-25 14:15:00', 2000.00, 'NGN', 2000.00, 'NGN'),

-- Account 4 for Sarah Brown (Savings)
('c3d4e5f6-3456-7890-cde1-000000000004', '1000000005', 'Savings', 'a1b2c3d4-1234-5678-9abc-000000000004',
 '2024-01-25 14:15:00', NULL, NULL, 'Active', 0, NULL, NULL,
 '2024-01-25 14:15:00', '2024-01-25 14:15:00', 2500.00, 'NGN', 2500.00, 'NGN'),

-- Account 5 for Mike Davis (Checking)
('c3d4e5f6-3456-7890-cde1-000000000005', '1000000006', 'Checking', 'a1b2c3d4-1234-5678-9abc-000000000005',
 '2024-01-25 14:15:00', NULL, NULL, 'Active', 0, NULL, NULL,
 '2024-01-25 14:15:00', '2024-01-25 14:15:00', 3000.00, 'NGN', 3000.00, 'NGN'),

-- Account 6 for Emily Wilson (Savings)
('c3d4e5f6-3456-7890-cde1-000000000006', '1000000007', 'Savings', 'a1b2c3d4-1234-5678-9abc-000000000006',
 '2024-01-25 14:15:00', NULL, NULL, 'Active', 0, NULL, NULL,
 '2024-01-25 14:15:00', '2024-01-25 14:15:00', 3500.00, 'NGN', 3500.00, 'NGN'),

-- Account 7 for David Taylor (Checking)
('c3d4e5f6-3456-7890-cde1-000000000007', '1000000008', 'Checking', 'a1b2c3d4-1234-5678-9abc-000000000007',
 '2024-01-25 14:15:00', NULL, NULL, 'Active', 0, NULL, NULL,
 '2024-01-25 14:15:00', '2024-01-25 14:15:00', 4000.00, 'NGN', 4000.00, 'NGN'),

-- Account 8 for Lisa Anderson (Savings)
('c3d4e5f6-3456-7890-cde1-000000000008', '1000000009', 'Savings', 'a1b2c3d4-1234-5678-9abc-000000000008',
 '2024-01-25 14:15:00', NULL, NULL, 'Active', 0, NULL, NULL,
 '2024-01-25 14:15:00', '2024-01-25 14:15:00', 4500.00, 'NGN', 4500.00, 'NGN'),

-- Account 9 for Chris Thomas (Checking)
('c3d4e5f6-3456-7890-cde1-000000000009', '1000000010', 'Checking', 'a1b2c3d4-1234-5678-9abc-000000000009',
 '2024-01-25 14:15:00', NULL, NULL, 'Active', 0, NULL, NULL,
 '2024-01-25 14:15:00', '2024-01-25 14:15:00', 5000.00, 'NGN', 5000.00, 'NGN');

-- Verify Customers were inserted
SELECT 
    c.Id AS CustomerId,
    c.FirstName,
    c.LastName,
    c.BVN,
    c.CreditScore,
    c.Email AS Email,
    c.PhoneNumber AS Phone,
    c.Street AS Street,
    c.City AS City,
    c.IsActive,
    c.DateCreated
FROM [Customers] c
ORDER BY c.FirstName;

-- Verify Accounts were inserted
SELECT 
    a.Id AS AccountId,
    a.AccountNumber,
    a.AccountType,
    c.FirstName + ' ' + c.LastName AS CustomerName,
    a.CurrentBalance,
    a.Currency,
    a.DateOpened
FROM [Accounts] a
INNER JOIN [Customers] c ON a.CustomerId = c.Id
ORDER BY a.AccountNumber;

-- Count summary
SELECT 
    'Customers' AS TableName, 
    COUNT(*) AS RecordCount 
FROM [Customers]
UNION ALL
SELECT 
    'Accounts' AS TableName, 
    COUNT(*) AS RecordCount 
FROM [Accounts];