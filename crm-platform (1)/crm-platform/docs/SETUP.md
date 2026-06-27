# Local setup - starting from zero

You said nothing's installed yet, so here's the full path for Windows and Mac.
Do this once, then you won't touch it again for the rest of the project.

## 1. Install Java 21 (JDK)

**Windows:**
```
winget install EclipseAdoptium.Temurin.21.JDK
```
**Mac:**
```
brew install openjdk@21
```
Verify: `java -version` should print `21.x.x`.

## 2. Install Maven

**Windows:** `winget install Apache.Maven`
**Mac:** `brew install maven`

Verify: `mvn -version`.

(If you use IntelliJ IDEA, it bundles its own Maven - you can skip this step
and let IntelliJ manage it, but having the CLI version is still useful.)

## 3. Install PostgreSQL

**Windows:** download the installer from postgresql.org/download/windows -
during setup it'll ask you to set a password for the `postgres` superuser,
remember it.
**Mac:** `brew install postgresql@16` then `brew services start postgresql@16`

Then create the database and a dedicated user (don't use the postgres
superuser for your app):
```sql
psql -U postgres
CREATE DATABASE crm_platform;
CREATE USER crm_user WITH PASSWORD 'crm_password';
GRANT ALL PRIVILEGES ON DATABASE crm_platform TO crm_user;
```
These credentials match `application.yml`'s defaults - if you change them
here, update `DB_USERNAME` / `DB_PASSWORD` env vars or the yml file too.

## 4. Install an IDE

**IntelliJ IDEA Community Edition** (free) has the best Spring Boot support -
auto-detects `@RestController`s, has a built-in HTTP client for testing
endpoints, and integrates Maven/run configs without extra setup.
Download: jetbrains.com/idea/download

VS Code + the "Extension Pack for Java" + "Spring Boot Extension Pack" also
works fine if you prefer it.

## 5. Run the project

```
cd crm-platform/backend
mvn spring-boot:run
```

Watch the console for:
```
Started CrmPlatformApplication in X.XXX seconds
```
That means it's alive. Then check:
- http://localhost:8080/actuator/health -> should return `{"status":"UP"}`
- http://localhost:8080/swagger-ui.html -> Swagger UI (empty for now, no
  endpoints exist yet - that's Phase 5)

If it fails on startup, 95% of the time it's the datasource connection -
double check PostgreSQL is running and the database/user from step 3 exist.

## 6. Git

```
cd crm-platform
git init
git add .
git commit -m "Phase 1-3: project scaffold, schema, dependencies"
```
Create an empty repo on GitHub first, then `git remote add origin <url>`
and `git push -u origin main`.

---
Once this is running, come back and we'll do Phase 4 (Authentication) -
that's where this actually starts looking like a CRM.
