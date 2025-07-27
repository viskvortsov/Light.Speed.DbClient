CREATE USER backend;
GRANT pg_read_server_files to backend;
GRANT backend to postgres;
CREATE DATABASE backend OWNER backend;
\connect backend
GRANT ALL PRIVILEGES ON DATABASE backend TO backend;
GRANT ALL PRIVILEGES ON DATABASE backend TO postgres;
DROP OWNED BY backend;
