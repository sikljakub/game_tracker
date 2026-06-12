CREATE TABLE IF NOT EXISTS platforms (
    id   SERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS games (
    id           SERIAL PRIMARY KEY,
    title        TEXT NOT NULL,
    platform_id  INT  NOT NULL REFERENCES platforms(id),
    release_year INT  CHECK (release_year BETWEEN 1970 AND 2100),
    note         TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS game_sessions (
    id           SERIAL PRIMARY KEY,
    game_id      INT          NOT NULL REFERENCES games(id) ON DELETE CASCADE,
    played_on    DATE         NOT NULL DEFAULT CURRENT_DATE,
    hours_played NUMERIC(5,2) NOT NULL CHECK (hours_played > 0),
    note         TEXT         NOT NULL DEFAULT ''
);
