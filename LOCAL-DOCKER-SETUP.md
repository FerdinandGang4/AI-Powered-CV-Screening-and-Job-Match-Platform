# Local Docker Setup

## What This Setup Adds

This project can now start locally with Docker using MongoDB as a document database for unstructured screening submissions such as pasted job descriptions and generated ranking reports.

The current setup keeps the working application flow and adds MongoDB persistence for:
- raw pasted or uploaded job description text
- candidate file metadata
- generated ranking report snapshots

## Services

The root `docker-compose.yml` starts:

1. `mongo`
Document database for unstructured screening submissions.

2. `mongo-express`
Browser UI for checking MongoDB documents locally at `http://localhost:8081`.

3. `backend`
ASP.NET API running on `http://localhost:5282`.

4. `frontend`
React app running on `http://localhost:5173`.

## Run Locally

From the project root:

```powershell
Copy-Item .env.example .env
# Then put your OpenAI key in .env as OPENAI_API_KEY=...
docker compose up -d --build
```

## Stop Everything

```powershell
docker compose down
```

## View Mongo Data

Open:

```text
http://localhost:8081
```

Look inside the `cvscreening` database and the `screening_submissions` collection.

## Notes

- MongoDB is being used because pasted job descriptions are naturally unstructured text and fit well in a document database.
- The application still uses in-memory auth and other prototype state for now.
- This is a good first phase before moving more of the system into persistent database-backed storage.
- OpenAI ranking still requires a valid API key. Put it in the root `.env` file before starting the stack.
