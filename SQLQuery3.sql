-- ================================
-- 1. PRODUCTS TABLE
-- ================================
CREATE TABLE Products (
    ProductID      INT IDENTITY(1,1) PRIMARY KEY,
    ProductName    NVARCHAR(150) NOT NULL,
    Category       NVARCHAR(100) NULL,
    Quantity       INT NOT NULL DEFAULT 0,
    ReorderLevel   INT NOT NULL DEFAULT 10,
    UnitPrice      DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    CreatedAt      DATETIME NOT NULL DEFAULT GETDATE()
);


-- ================================
-- 2. STAFF TABLE
-- ================================

CREATE TABLE Staff (
    StaffID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50),
    Password NVARCHAR(100),
    FullName NVARCHAR(100)
);


-- ================================
-- 3. GRN (Goods Receipt Note)
-- ================================
CREATE TABLE GRN (
    GRN_ID       INT IDENTITY(1,1) PRIMARY KEY,
    ProductID    INT NOT NULL,
    Quantity     INT NOT NULL,
    StaffID      INT NOT NULL,
    DateReceived DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_GRN_Product FOREIGN KEY (ProductID)
        REFERENCES Products(ProductID),

    CONSTRAINT FK_GRN_Staff FOREIGN KEY (StaffID)
        REFERENCES Staff(StaffID)
);


-- ================================
-- 4. GIN (Goods Issue Note)
-- ================================
CREATE TABLE GIN (
    GIN_ID       INT IDENTITY(1,1) PRIMARY KEY,
    ProductID    INT NOT NULL,
    Quantity     INT NOT NULL,
    StaffID      INT NOT NULL,
    DateIssued   DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_GIN_Product FOREIGN KEY (ProductID)
        REFERENCES Products(ProductID),

    CONSTRAINT FK_GIN_Staff FOREIGN KEY (StaffID)
        REFERENCES Staff(StaffID)
);

GO
CREATE TRIGGER trg_UpdateStock_GRN
ON GRN
AFTER INSERT
AS
BEGIN
    UPDATE P
    SET P.Quantity = P.Quantity + I.Quantity
    FROM Products P
    JOIN inserted I ON P.ProductID = I.ProductID;
END;

GO
CREATE TRIGGER trg_UpdateStock_GIN
ON GIN
AFTER INSERT
AS
BEGIN
    UPDATE P
    SET P.Quantity = P.Quantity - I.Quantity
    FROM Products P
    JOIN inserted I ON P.ProductID = I.ProductID;
END;


GO
INSERT INTO Products (ProductName, Category, Quantity, ReorderLevel, UnitPrice)
VALUES
('Acer Aspire 5 Laptop', 'Laptop', 20, 5, 15000.00),
('HP Pavilion 14', 'Laptop', 18, 5, 16500.00),
('Logitech Wireless Mouse M331', 'Accessories', 50, 10, 350.00),
('Razer DeathAdder V2', 'Accessories', 40, 8, 1250.00),
('Kingston 8GB DDR4 RAM', 'Components', 40, 10, 550.00),
('Corsair 16GB DDR4 RAM', 'Components', 35, 10, 890.00),
('Samsung 500GB SSD', 'Storage', 30, 5, 1200.00),
('Seagate 1TB HDD', 'Storage', 32, 8, 950.00),
('Dell 24-inch Monitor', 'Monitor', 15, 3, 3200.00),
('Asus VG248 Gaming Monitor', 'Monitor', 12, 3, 4500.00),
('Corsair 650W PSU', 'Components', 25, 5, 1800.00),
('Intel Core i5-10400F', 'Processor', 18, 5, 4200.00),
('Intel Core i7-11700', 'Processor', 14, 3, 7800.00),
('TP-Link Archer C6 Router', 'Networking', 28, 5, 890.00),
('MSI B560M PRO Motherboard', 'Components', 20, 5, 2500.00);

INSERT INTO Staff (Username, Password, FullName)
VALUES
('staff_Bao', '523K0033',  N'Tr?n Qu?c B?o'),
('staff_Duy', '523K0036',  N'Nguy?n Khánh Duy'),
('staff_Hau', '523K0038',  N'Nguy?n Trung H?u'),
('staff_Hien', '523K0039', N'Hu?nh Lê Huy Hi?n'),
('staff_Huy', '523K0041',  N'Châu Gia Huy'),
('staff_Nhan1', '523K0046', N'Nguy?n Minh Nhân'),
('staff_Nhan2', '523K0047', N'Tr?n Th? Th? Nhân'),
('staff_Nhat', '523K0048', N'Tr??ng Quang Nh?t'),
('staff_Quan1', '523K0049', N'Lê Minh Quân'),
('staff_Quan2', '523K0050', N'Nguy?n Minh Quân'),
('staff_Truong', '523K0054', N'Bùi Qu?c Tr??ng'),
('staff_Aung', '523K0073', N'Thiha Aung'),
('staff_Sandi', '523K0075', N'Thin Lei Sandi'),
('staff_Akhom', '523K0076', N'Mounthady Souk Akhom'),
('staff_Kyaw', '523K0078', N'Pai Hein Kyaw'),
('staff_Kien', '523V0009', N'Hu?nh Trung Kiên'),
('staff_Luan', '523V0010', N'Võ Minh Luân'),
('staff_Dat1', '523V0015', N'V? Thành ??t'),
('staff_Tan', '523V0017', N'??ng ?oàn Minh Tân'),
('staff_Dat2', '722I0004', N'Nguy?n Thành ??t');


INSERT INTO GRN (ProductID, Quantity, StaffID)
VALUES
(1, 5, 1),
(2, 4, 2),
(3, 12, 3),
(4, 9, 4),
(5, 10, 5),
(6, 8, 6),
(7, 15, 7),
(8, 14, 8),
(9, 6, 9),
(10, 7, 10),
(11, 11, 11),
(12, 5, 12),
(13, 9, 13),
(14, 13, 14),
(15, 10, 1);

INSERT INTO GIN (ProductID, Quantity, StaffID)
VALUES
(1, 2, 1),
(2, 1, 2),
(3, 5, 3),
(4, 3, 4),
(5, 2, 5),
(6, 4, 6),
(7, 3, 7),
(8, 2, 8),
(9, 2, 9),
(10, 3, 10),
(11, 2, 11),
(12, 1, 12),
(13, 3, 13),
(14, 4, 14),
(15, 2, 1);
