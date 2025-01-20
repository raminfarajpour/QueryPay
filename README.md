## System Design 

### High-Level Architecture

```mermaid
flowchart LR
    A[Billing Service] -- publishes/consumes --> R[<b>RabbitMQ</b>]
    A[Billing Service] -- withdraw request --> B[Wallet Service]
    B[Wallet Service] -- publishes/consumes --> R
    D[Database Proxy Service] -- proxies user request --> SS[(SQL Server)]
    D[Database Proxy Service] -- checks user balance --> A[Billing Service]
    D[Database Proxy Service] -- publish usage events --> R[<b>RabbitMQ</b>]
    E[End User] -- send request to proxy -->  D[Database Proxy Service]
    D -- uses as outbox --> RE[Redis]
    B -- send integration events for updating read model --> R
    A -- SQL --> SS
    A -- receives events --> R

    subgraph Data Stores
      SS
      PG[(Postgres)]
      MG[(MongoDB)]
      RE[(Redis)]
    end

    B -- stores events --> PG
    B -- reads/writes read model --> MG
```
### Squence Diagram
```mermaid
sequenceDiagram
    participant U as User
    participant WS as Wallet Service
    participant PG as Postgres (Event Store)
    participant MG as MongoDB (Read Model)
    participant RQ as RabbitMQ
    participant BS as Billing Service
    participant SS as SQL Server
    participant DP as Database Proxy

    U->>WS: Create Wallet Request
    WS->>PG: Store WalletCreated Event
    WS->>MG: Update/Insert Read Model
    WS->>RQ: Publish "WalletCreated" Event

    Note over RQ: RQ routes event to subscribers

    BS->>RQ: Subscribes "WalletCreated" (optional)
    U->>BS: Create Invoice & Pay Invoice
    BS->>SS: Insert Invoice Record (via DP or direct)
    BS->>RQ: Publish "InvoicePaid"

    RQ->>WS: Deliver "InvoicePaid"
    WS->>PG: Store WalletDebited Event
    WS->>MG: Update Read Model
    WS->>RQ: Publish "WalletUpdated"

    RQ->>BS: Deliver "WalletUpdated"
    BS->>SS: Update Invoice Status (paid)
    U->>BS: Check Invoice Status
    BS->>SS: Query
    BS->>U: Return final status

