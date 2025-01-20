## System Design 

### High-Level Architecture

```mermaid
flowchart LR
    A[Billing Service] -- publishes/consumes --> R[<b>RabbitMQ</b>]
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
