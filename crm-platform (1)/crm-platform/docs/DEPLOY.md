# Going live - Render (free) + Neon (free Postgres)

Render's own free Postgres now expires after a short trial window, so the
database lives on Neon instead, which has a genuinely permanent free tier.
The backend itself goes on Render, built straight from the Dockerfile - you
don't need Docker installed on your own machine for this, Render builds the
image in the cloud.

## 1. Database - Neon

1. Sign up at neon.tech (GitHub login works, same account as your repo)
2. Create a project, then copy the connection string it shows you - it
   looks like `postgresql://user:pass@host/dbname?sslmode=require`
3. Load the schema once, from your machine:
   ```
   psql "<connection string>" -f database/schema.sql
   ```

## 2. Backend - Render

1. Sign up at render.com (GitHub login)
2. Dashboard -> New + -> Web Service -> connect the `crm-platform` repo
3. **Root Directory:** `backend`
4. **Runtime:** Docker (Render auto-detects the `Dockerfile`)
5. **Instance type:** Free
6. Add environment variables under Settings -> Environment:
   - `SPRING_DATASOURCE_URL` = `jdbc:postgresql://<neon-host>/<dbname>?sslmode=require`
   - `DB_USERNAME` = your Neon username
   - `DB_PASSWORD` = your Neon password
   - `JWT_SECRET` = any long random string (don't ship the placeholder
     that's in `application.yml` - that one's for local dev only)
7. Deploy. The first build takes a few minutes since Maven is downloading
   every dependency inside the Docker build step.

Spring Boot picks up environment variables automatically and they always
win over whatever's in `application.yml` - no code changes needed for this
to work.

## 3. What to expect

- The free instance spins down after 15 minutes with no traffic. The next
  request takes 30-60 seconds to wake it back up. Normal, not a bug -
  don't panic if a demo feels slow on the very first click.
- You'll get a URL like `https://crm-platform-xxxx.onrender.com`. Check
  `https://crm-platform-xxxx.onrender.com/actuator/health` first.
- Once the repo is connected, every `git push` to `main` auto-redeploys -
  so as Phases 4 onward get built, pushing is the whole deploy step.

## Reused later, not redone

This Dockerfile is the same one Phase 12 (docker-compose, running
backend+frontend+postgres together locally) and Phase 14 (AWS ECR/ECS)
will build on. Nothing here is throwaway.
