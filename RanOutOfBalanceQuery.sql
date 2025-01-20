DECLARE @UserId AS BIGINT= 111222333
DECLARE @BillingId AS BIGINT= 1
DECLARE @MinutesAfter AS BIGINT= 5

DECLARE @CurrentBalance AS DECIMAL(18,2)

SELECT @CurrentBalance = Balance,@BillingId=Id  From Billings WHERE UserId = @UserId

;WITH ChunkedItems AS
    (
        SELECT 
            fi.Id AS FinancialItemId,
            fi.Amount,
            fi.CreatedAt,
            ChunkId = DATEDIFF(MINUTE, (SELECT MIN(fi.CreatedAt) 
                                         FROM [dbo].[FinancialItems] fi
                                         WHERE fi.BillingId = @BillingId), fi.CreatedAt) / @MinutesAfter
        FROM [dbo].[FinancialItems] fi
        WHERE fi.BillingId = @BillingId
    ),
    ChunkAverages AS
    (
        SELECT 
            ChunkId,
            AVG(Amount) AS AverageAmount,
            MIN(CreatedAt) AS ChunkStartTime
        FROM ChunkedItems
        GROUP BY ChunkId
    )
    
    SELECT TOP 1 IIF(AverageAmount > @CurrentBalance,'YES','NO') AS WillRanOutBalanceInGivenTime
      FROM ChunkAverages
     WHERE AverageAmount > @CurrentBalance
     ORDER BY ChunkStartTime;