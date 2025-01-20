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
    participant U as Client
    participant DP as Database Proxy
    participant BS as Billing Service
    participant WS as Wallet Service
    participant PG as Postgres (Event Store)
    participant MG as MongoDB (Read Model)
    participant RQ as RabbitMQ
    participant SS as SQL Server
    participant R as Redis
    

    U->>DP: Send TDS Packet
    DP->>SS: Send Request
    SS->>DP: Deliver Response
    DP->>BS: Check User Balance For Extracted Data
    BS->>WS: Get Wallet Balance
    WS->>BS: Deliver Wallet Balance
    BS->>DP: Deliver Process Possibility
    DP->>U: Deliver Error/ Response Packet
    DP->>R: Store Extraxted Data (Outbox)
    DP->>R: Read Data by Worker
    R->>DP: Outbox Data
    DP->>RQ: Send Usage Events
   
    RQ->>BS: Deliver usage event
    BS->>WS: Send Withdraw Request
    WS->>PG: Store Wallet Withdrawal Event
    WS->>RQ: Wallet Updated Integration Event
    WS->>RQ: Wallet UpdatedTransaction Created Integration  Event
    WS->>BS: Deliver Withdraw Response
    RQ->>WS: Deliver Updated Integration Event
    WS->>MG: Update/Insert Wallet Read Model
    RQ->>WS: Deliver UpdatedTransaction Created Integration  Event
    WS->>MG: Insert Transaction Read Model 

   BS->>SS: Store Financial Items  

