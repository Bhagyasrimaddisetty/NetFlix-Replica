-- ============================================================
-- Enterprise CRM Platform - Schema (Phase 2)
-- Run manually once: psql -U crm_user -d crm_platform -f schema.sql
-- (Spring Boot's ddl-auto: update will also create these tables for you
-- automatically on first run - this file is for reference, manual setup,
-- and for when you switch ddl-auto to 'validate' later.)
-- ============================================================

-- Users: the people who log into the CRM (sales reps, managers, admins)
CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,           -- BCrypt hash, never plain text
    role VARCHAR(20) NOT NULL DEFAULT 'SALES_REP',  -- ADMIN, MANAGER, SALES_REP
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Customers: companies/people the CRM tracks
CREATE TABLE customers (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    email VARCHAR(150),
    phone VARCHAR(20),
    company VARCHAR(150),
    owner_id BIGINT REFERENCES users(id),      -- the rep who owns this account
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Leads: potential customers in the pipeline, always tied to a customer record
CREATE TABLE leads (
    id BIGSERIAL PRIMARY KEY,
    customer_id BIGINT NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    status VARCHAR(30) NOT NULL DEFAULT 'NEW', -- NEW, CONTACTED, QUALIFIED, LOST
    source VARCHAR(50),                        -- WEBSITE, REFERRAL, COLD_CALL, EVENT
    assigned_to BIGINT REFERENCES users(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Opportunities: a lead that's progressed to an active deal with a dollar value.
-- Added because the dashboard's "Revenue" metric has to be sourced from somewhere,
-- and your original table list didn't have one even though the feature list did.
CREATE TABLE opportunities (
    id BIGSERIAL PRIMARY KEY,
    customer_id BIGINT NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    lead_id BIGINT REFERENCES leads(id),       -- nullable: some deals skip the lead stage
    name VARCHAR(150) NOT NULL,
    stage VARCHAR(30) NOT NULL DEFAULT 'PROSPECTING', -- PROSPECTING, NEGOTIATION, CLOSED_WON, CLOSED_LOST
    amount NUMERIC(12,2) DEFAULT 0,
    close_date DATE,
    owner_id BIGINT REFERENCES users(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Tasks: to-dos, optionally linked to a customer
CREATE TABLE tasks (
    id BIGSERIAL PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    assigned_to BIGINT REFERENCES users(id),
    related_customer_id BIGINT REFERENCES customers(id),
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- PENDING, IN_PROGRESS, DONE
    deadline TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Notes: free-text notes attached to a customer
CREATE TABLE notes (
    id BIGSERIAL PRIMARY KEY,
    customer_id BIGINT NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    author_id BIGINT REFERENCES users(id),
    description TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Indexes for the lookups you'll actually run (filtering by status, by owner, etc.)
CREATE INDEX idx_leads_customer_id ON leads(customer_id);
CREATE INDEX idx_leads_status ON leads(status);
CREATE INDEX idx_opportunities_customer_id ON opportunities(customer_id);
CREATE INDEX idx_opportunities_stage ON opportunities(stage);
CREATE INDEX idx_tasks_assigned_to ON tasks(assigned_to);
CREATE INDEX idx_notes_customer_id ON notes(customer_id);
