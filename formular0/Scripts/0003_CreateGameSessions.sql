CREATE TABLE IF NOT EXISTS game_sessions (
    id           SERIAL PRIMARY KEY,
    game_id      INT          NOT NULL REFERENCES games(id) ON DELETE CASCADE,
    played_on    DATE         NOT NULL DEFAULT CURRENT_DATE,
    hours_played NUMERIC(5,2) NOT NULL CHECK (hours_played > 0),
    note         TEXT         NOT NULL DEFAULT ''
);
