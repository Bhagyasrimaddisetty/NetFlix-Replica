# CRM platform - phase tracker

Keep this updated as we go so it's obvious where we left off, even weeks apart.

- [x] Phase 1 - Planning (features, modules defined)
- [x] Phase 2 - Database design (schema.sql + ER diagram; added the missing
      Opportunities table)
- [x] Phase 3 - Backend setup (Spring Boot project scaffolded, dependencies
      in pom.xml, application.yml configured)
- [ ] Phase 4 - Authentication (signup, login, JWT, BCrypt, role-based access)
- [ ] Phase 5 - CRUD APIs (Customers, Leads, Opportunities, Tasks, Notes)
- [ ] Phase 6 - Exception handling (global handler, custom exceptions)
- [ ] Phase 7 - Pagination, sorting, filtering, search
- [ ] Phase 8 - Testing (JUnit, Mockito, MockMvc, integration tests, 80-90% coverage)
- [ ] Phase 9 - React frontend (Login, Register, Dashboard, Customers, Leads, Tasks, Profile)
- [ ] Phase 10 - Dashboard (totals, charts, recent activity)
- [ ] Phase 11 - Telemetry (Actuator endpoints already in - add Micrometer + Prometheus + Grafana)
- [ ] Phase 12 - Docker (Dockerfile, docker-compose for backend+frontend+postgres)
- [ ] Phase 13 - GitHub Actions CI/CD
- [ ] Phase 14 - AWS deployment (ECS/EC2, RDS, S3, ECR)
- [ ] Phase 15 - AI module (bonus): summaries, email drafting, NL search, RAG

## Notes / decisions made so far

- Backend Dockerfile + Render deployment set up early (out of plan order)
  to get a live URL before Phase 4-5 exist. See docs/DEPLOY.md. This
  Dockerfile gets reused as-is in Phase 12 and Phase 14, not redone.
- Added an `opportunities` table (customer_id, lead_id, amount, stage) -
  the original plan listed "Opportunities" as a feature but had no table
  for it, and it's the natural source for the dashboard's revenue metric.
- `owner_id` / `assigned_to` foreign keys added to customers/leads/
  opportunities/tasks so role-based access in Phase 4 has something real
  to check against ("can this user see this record").
- `ddl-auto: update` for now (Phases 4-5 while entities are still moving).
  Switch to `validate` + manage schema via schema.sql or Flyway once the
  data model stabilizes, before this goes anywhere near production.
- jjwt 0.12.5 for JWT (Phase 4) and springdoc-openapi 2.5.0 for Swagger -
  both compatible with Spring Boot 3.2.x / Java 21.
