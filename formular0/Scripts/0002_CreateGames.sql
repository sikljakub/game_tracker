CREATE TABLE IF NOT EXISTS games (
    id           SERIAL PRIMARY KEY,
    title        TEXT NOT NULL,
    platform_id  INT  NOT NULL REFERENCES platforms(id),
    release_year INT  CHECK (release_year BETWEEN 1970 AND 2100),
    note         TEXT NOT NULL DEFAULT ''
);
