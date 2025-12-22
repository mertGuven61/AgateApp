-- 1. TEMİZLİK (Hata almamak için önce tabloyu temizleyelim)
DELETE FROM Adverts;
DELETE FROM Campaigns;
DELETE FROM Clients;

-- ID'leri saklamak için değişkenler
DECLARE @Client1Id INT, @Client2Id INT, @Client3Id INT;
DECLARE @Camp1Id INT, @Camp2Id INT, @Camp3Id INT, @Camp4Id INT;

-- ==========================================
-- CLIENT 1: Global Tech Solutions
-- ==========================================
-- AddressLine2 alanına '' (boş metin) gönderiyoruz ki hata vermesin.
INSERT INTO Clients (CompanyName, AddressLine1, AddressLine2, City, PostalCode, ContactPersonName, ContactEmail, PhoneNumber)
VALUES ('Global Tech Solutions', '123 Innovation Blvd', 'Suite 100', 'San Francisco', '94016', 'Sarah Connor', 'sarah@globaltech.com', '555-0199');

SET @Client1Id = SCOPE_IDENTITY();

    -- Campaign 1.1
    INSERT INTO Campaigns (Title, PlannedStartDate, PlannedFinishDate, EstimatedCost, Budget, ActualCost, AmountPaid, Status, ClientId)
    VALUES ('Q3 Product Launch', '2024-07-01', '2024-09-30', 50000.00, 60000.00, 15000.00, 0.00, 'In Progress', @Client1Id);
    SET @Camp1Id = SCOPE_IDENTITY();

        -- Adverts
        INSERT INTO Adverts (Title, MediaChannel, ProductionStatus, ScheduledRunDateStart, ScheduledRunDateEnd, Cost, CampaignId)
        VALUES ('TechCrunch Banner', 'Web', 'Ready', '2024-07-10', '2024-07-20', 2500.00, @Camp1Id);
        
        INSERT INTO Adverts (Title, MediaChannel, ProductionStatus, ScheduledRunDateStart, ScheduledRunDateEnd, Cost, CampaignId)
        VALUES ('Youtube Pre-Roll', 'Video', 'Filming', '2024-08-01', '2024-08-15', 12000.00, @Camp1Id);

    -- Campaign 1.2
    INSERT INTO Campaigns (Title, PlannedStartDate, PlannedFinishDate, EstimatedCost, Budget, ActualCost, AmountPaid, Status, ClientId)
    VALUES ('Holiday Season Teaser', '2024-11-01', '2024-12-31', 30000.00, 30000.00, 0.00, 0.00, 'Planned', @Client1Id);


-- ==========================================
-- CLIENT 2: Fresh Foods Market
-- ==========================================
INSERT INTO Clients (CompanyName, AddressLine1, AddressLine2, City, PostalCode, ContactPersonName, ContactEmail, PhoneNumber)
VALUES ('Fresh Foods Market', '45 Green Street', '', 'London', 'SW1A 1AA', 'Gordon Ramsey', 'gordon@freshfoods.com', '020-7946');
SET @Client2Id = SCOPE_IDENTITY();

    -- Campaign 2.1
    INSERT INTO Campaigns (Title, PlannedStartDate, PlannedFinishDate, EstimatedCost, Budget, ActualCost, AmountPaid, Status, ClientId)
    VALUES ('Organic Summer', '2024-06-01', '2024-08-31', 15000.00, 20000.00, 18000.00, 18000.00, 'Completed', @Client2Id);
    SET @Camp3Id = SCOPE_IDENTITY();

        -- Adverts
        INSERT INTO Adverts (Title, MediaChannel, ProductionStatus, ScheduledRunDateStart, ScheduledRunDateEnd, Cost, CampaignId)
        VALUES ('Local Newspaper Full Page', 'Print', 'Finished', '2024-06-05', '2024-06-05', 1500.00, @Camp3Id);


-- ==========================================
-- CLIENT 3: Apex Motors
-- ==========================================
INSERT INTO Clients (CompanyName, AddressLine1, AddressLine2, City, PostalCode, ContactPersonName, ContactEmail, PhoneNumber)
VALUES ('Apex Motors', '88 Drift Road', 'Floor 45', 'Tokyo', '100-0001', 'Kenji Sato', 'kenji@apexmotors.jp', '03-321-4567');
SET @Client3Id = SCOPE_IDENTITY();

    -- Campaign 3.1
    INSERT INTO Campaigns (Title, PlannedStartDate, PlannedFinishDate, EstimatedCost, Budget, ActualCost, AmountPaid, Status, ClientId)
    VALUES ('New Model X Launch', '2024-09-01', '2025-01-31', 120000.00, 150000.00, 45000.00, 20000.00, 'In Progress', @Client3Id);
    SET @Camp4Id = SCOPE_IDENTITY();

        -- Adverts
        INSERT INTO Adverts (Title, MediaChannel, ProductionStatus, ScheduledRunDateStart, ScheduledRunDateEnd, Cost, CampaignId)
        VALUES ('TV Commercial Prime Time', 'TV', 'Editing', '2024-09-15', '2024-10-15', 85000.00, @Camp4Id);

        INSERT INTO Adverts (Title, MediaChannel, ProductionStatus, ScheduledRunDateStart, ScheduledRunDateEnd, Cost, CampaignId)
        VALUES ('Instagram Influencers', 'Social Media', 'Concept', '2024-09-20', '2024-10-20', 15000.00, @Camp4Id);
        
        INSERT INTO Adverts (Title, MediaChannel, ProductionStatus, ScheduledRunDateStart, ScheduledRunDateEnd, Cost, CampaignId)
        VALUES ('Billboard City Center', 'Outdoor', 'Ready', '2024-09-01', '2024-12-01', 12000.00, @Camp4Id);