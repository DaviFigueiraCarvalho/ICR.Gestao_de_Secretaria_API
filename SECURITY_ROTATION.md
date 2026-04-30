# Security rotation and cleanup checklist

## 1) Rotate exposed secrets immediately
- JWT signing key (`JWT_KEY`)
- Database password (`POSTGRES_PASSWORD` / DB user password)
- Root credentials (`ROOTUSERNAME` / `ROOTPASSWORD`)

## 2) Keep secrets out of versioned files
- Never commit real values in `appsettings*.json`, `docker-compose.yml` or source code.
- Use `.env` (ignored) for local dev and platform environment variables for production.

## 3) Configure secrets in platform env vars (Railway)
- `DATABASE_URL` (or `ConnectionStrings__DefaultConnection`)
- `JWT_KEY`
- `ROOTUSERNAME`
- `ROOTPASSWORD`
- `ASPNETCORE_ENVIRONMENT=Production`
 - `BASE_DOMAIN` (CORS base domain)

## 4) Rewrite Git history (optional but recommended if secrets were committed)
> Warning: rewriting history is destructive. Coordinate with team first.

### Using BFG
1. Create a mirror backup clone.
2. Replace/delete sensitive text with BFG.
3. Run GC and force push.

References:
- BFG: https://rtyley.github.io/bfg-repo-cleaner/
- git-filter-repo: https://github.com/newren/git-filter-repo

## 5) After cleanup
- Rotate secrets again (if needed).
- Update CI/CD and production environments.
- Ask team members to re-clone repository if history was rewritten.
