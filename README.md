# enterprise-agentic-ai
Enterprise-grade AI architecture
sequenceDiagram
    autonumber

    actor User as Taxpayer / Client
    participant AGW as Azure Application Gateway + WAF
    participant ORCH as Payment Orchestrator Function
    participant PREM as On-Prem Payment API
    participant PDB as On-Prem Payment Database
    participant CPS as Cloud Payment Service
    participant PII as PII Surrogation Service
    participant SQL as Payment Azure SQL Database
    participant CDC as Cloud CDC Replica
    participant RECON as Reconciliation Function

    %% ============================
    %% REAL-TIME PAYMENT SUBMISSION
    %% ============================

    User->>AGW: Submit payment request over HTTPS
    AGW->>AGW: Apply WAF inspection, TLS, and routing
    AGW->>ORCH: Forward payment request

    ORCH->>ORCH: Validate request envelope
    ORCH->>ORCH: Generate/propagate CorrelationId
    ORCH->>ORCH: Generate/propagate IdempotencyKey

    par Authoritative on-prem processing
        ORCH->>PREM: Submit payment request
        PREM->>PREM: Perform full business validation
        PREM->>PDB: Schedule and persist payment

        alt Payment accepted
            PDB-->>PREM: Payment stored
            PREM-->>ORCH: Accepted + OnPremConfirmationId
        else Payment rejected
            PREM-->>ORCH: Business rejection + reason code
        else On-prem technical failure
            PREM--xORCH: Timeout or dependency error
        end

    and Parallel cloud capture
        ORCH->>CPS: Capture payment request

        CPS->>CPS: Perform basic cloud validation
        CPS->>CPS: Check IdempotencyKey

        alt Cloud validation succeeds
            CPS->>PII: Surrogate sensitive fields
            PII-->>CPS: Taxpayer and bank surrogate IDs

            CPS->>SQL: Insert provisional payment record
            Note over SQL: CloudCaptureStatus = Persisted<br/>OnPremStatus = Unknown<br/>ReconciliationStatus = Pending

            alt Payment record created
                SQL-->>CPS: CloudPaymentId
                CPS-->>ORCH: Cloud capture successful
            else Duplicate IdempotencyKey
                SQL-->>CPS: Existing CloudPaymentId
                CPS-->>ORCH: Idempotent success
            else Azure SQL failure
                SQL--xCPS: Persistence failure
                CPS-->>ORCH: Cloud capture failed
            end

        else Cloud validation fails
            CPS-->>ORCH: Cloud validation failure
        end
    end

    %% ============================
    %% RESPONSE DECISION
    %% ============================

    ORCH->>ORCH: Apply response decision rules

    alt On-prem accepted
        ORCH-->>AGW: Success + authoritative confirmation
        AGW-->>User: Payment accepted / scheduled
    else On-prem rejected
        ORCH-->>AGW: Business rejection
        AGW-->>User: Payment rejected
    else On-prem timeout or unknown result
        ORCH-->>AGW: Pending or technical error
        AGW-->>User: Unable to confirm payment status
    end

    Note over User,ORCH: Target end-to-end response time ≤ 2 seconds

    %% ============================
    %% CDC REPLICATION
    %% ============================

    PDB-->>CDC: Replicate payment data throughout the day
    Note over CDC: Read-only cloud copy of authoritative on-prem data

    %% ============================
    %% END-OF-DAY RECONCILIATION
    %% ============================

    Note over RECON: Scheduled end-of-day reconciliation begins

    RECON->>CDC: Read replicated on-prem payments
    CDC-->>RECON: Authoritative payment records

    RECON->>SQL: Read payments with ReconciliationStatus = Pending
    SQL-->>RECON: Cloud provisional payment records

    loop For each payment
        RECON->>RECON: Match using confirmation ID,<br/>correlation ID, or idempotency key

        alt Cloud and on-prem records match
            RECON->>CPS: Update reconciliation result
            CPS->>SQL: Set ReconciliationStatus = Matched
            CPS->>SQL: Set OnPremStatus = Accepted
            CPS->>SQL: Store OnPremConfirmationId
            SQL-->>CPS: Update completed
            CPS-->>RECON: Reconciliation updated

        else On-prem record indicates rejection
            RECON->>CPS: Update rejected result
            CPS->>SQL: Set OnPremStatus = Rejected
            CPS->>SQL: Set ReconciliationStatus = Matched
            SQL-->>CPS: Update completed
            CPS-->>RECON: Reconciliation updated

        else Cloud record exists but on-prem record is missing
            RECON->>CPS: Mark missing on-prem record
            CPS->>SQL: Set ReconciliationStatus = MissingOnPrem
            SQL-->>CPS: Update completed
            CPS-->>RECON: Reconciliation updated

        else On-prem record exists but cloud record is missing
            RECON->>CPS: Create recovered cloud payment
            CPS->>SQL: Insert payment from CDC data
            CPS->>SQL: Set ReconciliationStatus = RecoveredFromCDC
            SQL-->>CPS: Recovered payment created
            CPS-->>RECON: Recovery completed

        else Payment data or status mismatch
            RECON->>CPS: Record reconciliation mismatch
            CPS->>SQL: Set ReconciliationStatus = Mismatch
            SQL-->>CPS: Update completed
            CPS-->>RECON: Mismatch recorded
        end
    end

    RECON->>CPS: Save reconciliation run summary
    CPS->>SQL: Persist reconciliation summary
    SQL-->>CPS: Summary saved
    CPS-->>RECON: Reconciliation run completed